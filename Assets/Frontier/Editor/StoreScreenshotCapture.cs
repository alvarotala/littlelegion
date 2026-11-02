using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Frontier.Core;
using Frontier.Runtime;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Frontier.Editor
{
    // Captures the runtime BoardView and unmodified simulation. Both sides use
    // normal AI commands: no spawned showcase units, resource grants or fog edits.
    public static class StoreScreenshotCapture
    {
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        private const string Output="art/store/meta-horizon/2026-11-02/screenshots";
        private static void Call(object target,string method)=>target.GetType().GetMethod(method,Private).Invoke(target,null);
        private static void Set(GameSession session,string property,object value)=>typeof(GameSession).GetProperty(property).SetValue(session,value);

        [MenuItem("Frontier/Capture store screenshots")]
        public static void Render()
        {
            Directory.CreateDirectory(Output);
            Capture("01-forest-economy","rift","industrial",150,false,false);
            Capture("02-biological-base","desert","biological",240,false,false);
            Capture("03-forest-combat","rift","industrial",330,true,false);
            Capture("04-neon-army","training","biological",300,false,true);
            Capture("05-desert-expansion","desert","industrial",480,false,false);
            Debug.Log("LITTLE_LEGION_STORE_SCREENSHOTS_COMPLETE: 5 runtime captures, 2560x1440");
        }
        public static void RenderCombat()=>Capture("03-forest-combat","rift","industrial",330,true,false);

        private static void Capture(string name,string map,string faction,int seconds,bool battle,bool army)
        {
            var preview=new PreviewRenderUtility();
            var fixture=new GameObject("Store screenshot simulation");
            var boardObject=new GameObject("Store screenshot board");
            var bounds=Shader.GetGlobalVector("_FrontierBounds");
            var matrix=Shader.GetGlobalMatrix("_FrontierWorldToBoard");
            var light=Shader.GetGlobalFloat("_FrontierEnhancedLighting");
            try
            {
                var session=fixture.AddComponent<GameSession>();
                if(session.Catalog==null)Call(session,"Awake");
                session.Faction=faction;session.EnemyFaction=faction=="industrial"?"biological":"industrial";
                session.LoadMap(map);
                var sim=new Simulation(session.Map,session.Catalog,session.Faction,session.EnemyFaction);
                Set(session,"Match",sim);Set(session,"Running",true);
                var bots=new[]{new Strategist(0,AiDifficulty.Hard,session.Catalog,session.Map),
                    new Strategist(1,AiDifficulty.Hard,session.Catalog,session.Map)};
                int first=seconds*Simulation.TickRate,limit=(battle?720:seconds)*Simulation.TickRate;
                PlayerView view=null;
                bool foundBattle=false;
                Point battleFocus=default;
                for(int tick=0;tick<limit&&sim.Winner<0;tick++)
                {
                    if(sim.Tick%20==0)
                        for(int player=0;player<2;player++)
                            foreach(var command in bots[player].Decide(sim.View(player)))sim.Submit(command);
                    sim.Step();
                    if(battle&&sim.Tick>=first)
                    {
                        view=sim.View(0);
                        if(view.attacks.Length>=2)
                        {
                            foreach(var attack in view.attacks.Where(a=>a.owner==0))
                            {
                                var nearby=view.entities.Where(e=>e.position.Distance(attack.origin)<14&&!session.Catalog[e.definition].building
                                    &&session.Catalog[e.definition].role!="worker").ToArray();
                                if(nearby.Count(e=>e.owner==0)>=4&&nearby.Count(e=>e.owner==1)>=3)
                                {
                                    battleFocus=new Point(nearby.Average(e=>e.position.x),nearby.Average(e=>e.position.y));
                                    foundBattle=true;break;
                                }
                            }
                            if(foundBattle)break;
                        }
                    }
                }
                view=sim.View(0);Set(session,"View",view);
                if(battle&&!foundBattle)throw new Exception("No visible combat found for "+name);
                var hq=view.entities.Where(e=>e.owner==0&&session.Catalog[e.definition].role=="hq").OrderBy(e=>e.id).ToArray();
                if(hq.Length==0)throw new Exception("No surviving headquarters for "+name);
                Point focus=hq[name.StartsWith("05")?hq.Length-1:0].position;
                float radius=11;
                if(battle)
                {
                    focus=battleFocus;radius=11;
                }
                else if(army)
                {
                    var troops=view.entities.Where(e=>e.owner==0&&!session.Catalog[e.definition].building&&session.Catalog[e.definition].role!="worker").ToArray();
                    if(troops.Length==0)throw new Exception("No army for "+name);
                    focus=new Point(troops.Average(e=>e.position.x),troops.Average(e=>e.position.y));radius=16;
                }
                else
                {
                    var nearby=view.entities.Where(e=>e.owner==0&&e.position.Distance(focus)<16).ToArray();
                    focus=new Point(nearby.Average(e=>e.position.x)+2,nearby.Average(e=>e.position.y)+2);
                }
                var board=boardObject.AddComponent<BoardView>();board.Session=session;board.Viewer=preview.camera;board.Width=1.6f;board.Placed=true;
                Call(board,"Start");preview.AddSingleGO(boardObject);
                // Enhanced lighting is an existing in-game option; do not change saved preferences.
                typeof(BoardView).GetProperty("EnhancedLighting").SetValue(board,true);
                for(int i=0;i<30;i++)Call(board,"LateUpdate");
                var center=board.Content.TransformPoint(new Vector3(focus.x,0,focus.y));
                preview.camera.orthographic=true;preview.camera.orthographicSize=board.CellScale*radius;
                preview.camera.nearClipPlane=.001f;preview.camera.farClipPlane=10;
                preview.camera.transform.position=center+new Vector3(.12f,.75f,-.9f);preview.camera.transform.LookAt(center);
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.018f,.025f,.035f);
                for(int i=0;i<4;i++)Call(board,"LateUpdate");
                preview.BeginStaticPreview(new Rect(0,0,2560,1440));preview.Render(true);
                var image=preview.EndStaticPreview();File.WriteAllBytes(Output+"/"+name+".png",image.EncodeToPNG());Object.DestroyImmediate(image);
                File.WriteAllText(Output+"/"+name+".json",JsonUtility.ToJson(new CaptureInfo{map=map,faction=faction,tick=sim.Tick,
                    visibleEntities=view.entities.Length,focusX=focus.x,focusY=focus.y,radius=radius},true));
                Debug.Log("STORE_SCREENSHOT_CAPTURED "+name+" tick="+sim.Tick+" visible="+view.entities.Length);
            }
            finally
            {
                Object.DestroyImmediate(boardObject);Object.DestroyImmediate(fixture);preview.Cleanup();
                Shader.SetGlobalVector("_FrontierBounds",bounds);Shader.SetGlobalMatrix("_FrontierWorldToBoard",matrix);
                Shader.SetGlobalFloat("_FrontierEnhancedLighting",light);
            }
        }

        [Serializable]
        private sealed class CaptureInfo
        {
            public string map,faction;
            public int tick,visibleEntities;
            public float focusX,focusY,radius;
        }
    }
}
