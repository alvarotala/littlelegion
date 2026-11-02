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
    // Uses only normal simulation commands and the shipped player/replay views.
    public static class StoreActionScreenshotCapture
    {
        private const string Output="art/store/meta-horizon/2026-11-02/screenshots-v2";
        private const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        private static void Call(object target,string method)=>target.GetType().GetMethod(method,Private).Invoke(target,null);
        private static void Set(GameSession session,string property,object value)=>typeof(GameSession).GetProperty(property).SetValue(session,value);

        [Serializable]
        private sealed class Frame
        {
            public string name,map,faction,perspective;
            public PlayerView view;
            public float focusX,focusY,radius,score;
            public int ownUnits,enemyUnits,working;
        }

        [MenuItem("Frontier/Capture action store screenshots")]
        public static void Render()
        {
            Directory.CreateDirectory(Output);
            Scan("training","industrial","01-neon-battle",null,null);
            Scan("desert","biological","02-desert-battle",null,"05-biological-workers");
            Scan("rift","industrial",null,"03-industrial-army","04-industrial-workers");
            Debug.Log("STORE_ACTION_SCREENSHOTS_COMPLETE: five native 2560x1440 captures");
        }

        private static void Scan(string map,string faction,string battleName,string armyName,string workersName)
        {
            var fixture=new GameObject("Screenshot match scan");
            try
            {
                var session=fixture.AddComponent<GameSession>();if(session.Catalog==null)Call(session,"Awake");
                session.Faction=faction;session.EnemyFaction=faction=="industrial"?"biological":"industrial";session.LoadMap(map);
                var sim=new Simulation(session.Map,session.Catalog,session.Faction,session.EnemyFaction);
                Set(session,"Match",sim);Set(session,"Running",true);
                var bots=new[]{new Strategist(0,AiDifficulty.Hard,session.Catalog,session.Map),new Strategist(1,AiDifficulty.Hard,session.Catalog,session.Map)};
                Frame battle=null,army=null,workers=null;
                for(int tick=0;tick<Simulation.TickRate*900&&sim.Winner<0;tick++)
                {
                    if(sim.Tick%20==0)for(int p=0;p<2;p++)foreach(var command in bots[p].Decide(sim.View(p)))sim.Submit(command);
                    sim.Step();
                    if(sim.Tick<Simulation.TickRate*120||sim.Tick%10!=0)continue;
                    var player=sim.View(0);
                    if(battleName!=null)
                    {
                        var replay=sim.ObserverView(out _);
                        foreach(var attack in replay.attacks.Where(a=>a.owner==0).Take(8))
                        {
                            var group=replay.entities.Where(e=>e.health>0&&!session.Catalog[e.definition].building
                                &&session.Catalog[e.definition].role!="worker"&&e.position.Distance(attack.origin)<11).ToArray();
                            int own=group.Count(e=>e.owner==0),enemy=group.Count(e=>e.owner==1);
                            if(own<4||enemy<4)continue;
                            float score=Math.Min(own,enemy)*12+group.Length+replay.attacks.Count(a=>a.origin.Distance(attack.origin)<12)*2;
                            if(battle!=null&&score<=battle.score)continue;
                            var focus=new Point(group.Average(e=>e.position.x),group.Average(e=>e.position.y));
                            float depth=group.Max(e=>Math.Abs(e.position.y-focus.y));
                            battle=Make(battleName,replay,focus,Mathf.Clamp(depth*.85f+3.5f,7.5f,12),score,"replay",own,enemy,0);
                        }
                    }
                    if(armyName!=null)
                    {
                        var troops=player.entities.Where(e=>e.owner==0&&e.health>0&&!session.Catalog[e.definition].building&&session.Catalog[e.definition].role!="worker").ToArray();
                        foreach(var unit in troops)
                        {
                            var group=troops.Where(e=>e.position.Distance(unit.position)<10).ToArray();
                            float score=group.Length*5+group.Select(e=>e.definition).Distinct().Count()*4;
                            if(group.Length<10||army!=null&&score<=army.score)continue;
                            army=Make(armyName,player,new Point(group.Average(e=>e.position.x),group.Average(e=>e.position.y)),8,score,"player",group.Length,0,0);
                        }
                    }
                    if(workersName!=null&&sim.Tick<Simulation.TickRate*480)
                    {
                        foreach(var hq in player.entities.Where(e=>e.owner==0&&session.Catalog[e.definition].role=="hq"))
                        {
                            var group=player.entities.Where(e=>e.owner==0&&e.position.Distance(hq.position)<10).ToArray();
                            var busy=group.Where(e=>session.Catalog[e.definition].role=="worker"&&(e.working||e.depositing)).ToArray();
                            float score=busy.Length*5+group.Count(e=>session.Catalog[e.definition].building)*2;
                            if(busy.Length<8||workers!=null&&score<=workers.score)continue;
                            workers=Make(workersName,player,new Point((busy.Average(e=>e.position.x)+hq.position.x)*.5f,
                                (busy.Average(e=>e.position.y)+hq.position.y)*.5f),7.5f,score,"player",group.Length,0,busy.Length);
                        }
                    }
                }
                if(battleName!=null&&battle==null||armyName!=null&&army==null||workersName!=null&&workers==null)
                    throw new Exception("Could not find a sufficiently active real match frame: "+map);
                foreach(var frame in new[]{battle,army,workers}.Where(f=>f!=null))
                {
                    File.WriteAllText(Output+"/"+frame.name+".json",JsonUtility.ToJson(frame,true));
                    Draw(session,frame);
                }
                Frame Make(string name,PlayerView view,Point focus,float radius,float score,string perspective,int own,int enemy,int busy)
                    =>new Frame{name=name,map=map,faction=faction,view=view,focusX=focus.x,focusY=focus.y,radius=radius,
                        score=score,perspective=perspective,ownUnits=own,enemyUnits=enemy,working=busy};
            }
            finally{Object.DestroyImmediate(fixture);}
        }

        private static void Draw(GameSession session,Frame frame)
        {
            var preview=new PreviewRenderUtility();var boardObject=new GameObject("Action screenshot board");
            var bounds=Shader.GetGlobalVector("_FrontierBounds");var matrix=Shader.GetGlobalMatrix("_FrontierWorldToBoard");
            try
            {
                Set(session,"View",frame.view);
                var board=boardObject.AddComponent<BoardView>();board.Session=session;board.Viewer=preview.camera;board.Width=1.6f;board.Placed=true;
                Call(board,"Start");preview.AddSingleGO(boardObject);
                typeof(BoardView).GetProperty("EnhancedLighting").SetValue(board,true);
                for(int i=0;i<30;i++)Call(board,"LateUpdate");
                var center=board.Content.TransformPoint(new Vector3(frame.focusX,0,frame.focusY));
                preview.camera.orthographic=false;preview.camera.fieldOfView=42;
                preview.camera.nearClipPlane=.001f;preview.camera.farClipPlane=10;
                float distance=board.CellScale*frame.radius/Mathf.Tan(preview.camera.fieldOfView*Mathf.Deg2Rad*.5f)*1.15f;
                preview.camera.transform.position=center+new Vector3(.18f,.8f,-1).normalized*distance;
                preview.camera.transform.LookAt(center+Vector3.up*board.CellScale*.6f);
                preview.camera.clearFlags=CameraClearFlags.SolidColor;preview.camera.backgroundColor=new Color(.018f,.025f,.035f);
                for(int i=0;i<4;i++)Call(board,"LateUpdate");
                preview.BeginStaticPreview(new Rect(0,0,2560,1440));preview.Render(true);
                var image=preview.EndStaticPreview();File.WriteAllBytes(Output+"/"+frame.name+".png",image.EncodeToPNG());Object.DestroyImmediate(image);
                Debug.Log("STORE_ACTION_CAPTURED "+frame.name+" tick="+frame.view.tick+" own="+frame.ownUnits+" enemy="+frame.enemyUnits+" working="+frame.working);
            }
            finally
            {
                Object.DestroyImmediate(boardObject);preview.Cleanup();
                Shader.SetGlobalVector("_FrontierBounds",bounds);Shader.SetGlobalMatrix("_FrontierWorldToBoard",matrix);
            }
        }
    }
}
