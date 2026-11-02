using System.Linq;
using Frontier.Core;

namespace Frontier.Runtime
{
    public sealed partial class CommandPanel
    {
        // Presentation cues use the filtered view and existing console action IDs.
        private int tutorialCueEntity,tutorialCueResource;
        private Point? tutorialCuePoint;
        private string tutorialCueAction;

        private void UpdateTutorialContent()
        {
            var lesson=Session.Tutorial;
            if(lesson==null||Session.View==null)return;
            tutorialCueEntity=tutorialCueResource=0;tutorialCuePoint=null;tutorialCueAction=null;
            ((LocalizedText)tutorialProgress).SetKey("PLAY GUIDE · {0}/{1}",lesson.StepNumber,lesson.StepCount);
            tutorialState.text=lesson.Briefing?"Ready? Let's try it!":Session.IsPaused?"Take your time":"Your turn!";
            string select=Bindings.Name(Quest.Active,"select"),order=Bindings.Name(Quest.Active,"order");
            string definition=lesson.ExpectedDefinition;
            string name=string.IsNullOrEmpty(definition)?"":Session.Catalog[definition].name;
            if(!lesson.Briefing&&Page!=MenuPage.Actions)
            {
                TutorialText("Game paused","Your units wait while you explore the game menu.");
                tutorialState.text="Take your time";
                tutorialCueAction=Page==MenuPage.Pause?"resume":"back";
                TutorialInstruction(Page==MenuPage.Pause?"Choose Resume match to keep playing.":"Go back to the match menu.");
                return;
            }
            switch(lesson.Step)
            {
                case TutorialStep.Welcome:
                    TutorialText("Your first victory!","Build a little base. Lead your army to victory.");
                    TutorialInstruction(Quest.Active?"Point at Continue, then press Trigger / {0}.":"Click Continue. Let's play!",Bindings.MenuConfirmName);break;
                case TutorialStep.SelectWorker:
                    TutorialText("Meet your worker","This little builder gathers resources and grows your base.");
                    TutorialSelectEntity(lesson.FocusEntity,"Point at this worker and press {0}.",select);break;
                case TutorialStep.MoveWorker:
                    TutorialText("Off you go!","Your selected units follow orders on the ground.");
                    if(lesson.AcceptedOrderCount>0)
                    {tutorialCueEntity=lesson.FocusEntity;TutorialInstruction("On the way! Let your worker reach the marker.");}
                    else if(!TutorialWorkerSelected())TutorialSelectEntity(lesson.FocusEntity,"First, select this worker with {0}.",select);
                    else{tutorialCuePoint=lesson.FocusPoint;TutorialInstruction("Point at the violet marker and press {0}.",order);}break;
                case TutorialStep.GatherMinerals:
                    TutorialText("Bring home minerals","Minerals buy buildings and soldiers. Two workers per patch!");
                    TutorialGather(select,order);break;
                case TutorialStep.SelectHeadquarters:
                    TutorialText("Home sweet base","Your headquarters receives resources and trains new workers.");
                    TutorialSelectEntity(lesson.FocusEntity,"Select your headquarters with {0}.",select);break;
                case TutorialStep.TrainWorker:
                    TutorialText("One more helper","Training costs minerals and reserves space for your new worker.");
                    if(lesson.AcceptedOrderCount>0)
                    {tutorialCueEntity=lesson.FocusEntity;TutorialInstruction("A helper is on the way! Wait for training to finish.");}
                    else if(!TutorialEntitySelected(lesson.FocusEntity))TutorialSelectEntity(lesson.FocusEntity,"First, select your headquarters with {0}.",select);
                    else if(TutorialWaitForCost(definition))break;
                    else
                    {
                        tutorialCueAction="train:"+definition;
                        TutorialInstruction(Quest.Active?"Choose {0} in the console with Trigger / {1}.":"Click {0} in the console.",name,Bindings.MenuConfirmName);
                    }
                    break;
                case TutorialStep.BuildSupply:
                    TutorialText("Room for everyone","Supply makes room for more workers and soldiers.");
                    TutorialBuild(select,name,definition);break;
                case TutorialStep.BuildBarracks:
                    TutorialText("Time for soldiers","Military buildings train troops to protect your busy workers.");
                    TutorialBuild(select,name,definition);break;
                case TutorialStep.TrainArmy:
                    TutorialText("Your little legion","These three ranged soldiers will lead your first attack.");
                    int remaining=System.Math.Max(0,GuidedTutorial.ArmySize-lesson.AcceptedOrderCount);
                    if(remaining==0)
                    {tutorialCueEntity=lesson.FocusEntity;TutorialInstruction("Your squad is training. Wait for all three soldiers.");}
                    else if(!TutorialEntitySelected(lesson.FocusEntity))TutorialSelectEntity(lesson.FocusEntity,"Select this military building with {0}.",select);
                    else if(TutorialWaitForCost(definition))break;
                    else
                    {
                        tutorialCueAction="train:"+definition;
                        if(Quest.Active&&remaining==1)
                            TutorialInstruction("Choose {0} in the console once with Trigger / {1}.",name,Bindings.MenuConfirmName);
                        else TutorialInstruction(Quest.Active?"Choose {0} in the console {1} times with Trigger / {2}.":
                            remaining==1?"Click {0} once in the console.":"Click {0} {1} times in the console.",name,remaining,Bindings.MenuConfirmName);
                    }
                    break;
                case TutorialStep.SelectArmy:
                    TutorialText("Round up the squad","Soldiers fight together. Leave your workers gathering at home.");
                    tutorialCueEntity=lesson.FocusEntity;
                    TutorialInstruction("Hold {0} and drag a box around your three soldiers.",select);break;
                case TutorialStep.AttackMove:
                    TutorialText("March into action","Attack Move travels forward and fights enemies along the way.");
                    TutorialAttack(select,false);break;
                case TutorialStep.DestroyEnemy:
                    TutorialText("Go for victory!","Destroy every enemy building to win the match.");
                    TutorialAttack(select,true);break;
                case TutorialStep.Complete:
                    TutorialText("You did it!","Your first victory! You're ready to challenge the AI.");
                    tutorialProgress.text="VICTORY!";tutorialState.text="Nicely played!";
                    TutorialInstruction("Head to the main menu, or play this tutorial again.");break;
            }
            if(lesson.Briefing&&!lesson.Complete)
            {
                tutorialCueEntity=tutorialCueResource=0;tutorialCuePoint=null;tutorialCueAction=null;
                tutorialInstruction.text="";
            }
        }

        private void TutorialBuild(string select,string name,string definition)
        {
            var lesson=Session.Tutorial;
            if(lesson.AcceptedOrderCount>0)
            {
                tutorialCueEntity=lesson.FocusEntity;
                TutorialText("Building in progress!","Your worker has this. Buildings need a builder nearby.");
                TutorialInstruction("Wait here until construction finishes.");return;
            }
            if(!TutorialWorkerSelected())
            {
                if(!TutorialCancelForSelection(true))TutorialSelectEntity(lesson.FocusEntity,"First, select this worker with {0}.",select);
                return;
            }
            if(TutorialWaitForCost(definition))return;
            if(Session.BuildDefinition!=definition)
            {
                tutorialCueAction="build:"+definition;
                TutorialInstruction(Quest.Active?"Choose {0} in the console with Trigger / {1}.":"Click {0} in the console.",name,Bindings.MenuConfirmName);return;
            }
            tutorialCuePoint=lesson.FocusPoint;
            TutorialInstruction("Point at the violet outline and press {0}.",select);
        }

        private void TutorialGather(string select,string order)
        {
            var lesson=Session.Tutorial;
            if(lesson.AcceptedOrderCount>0)
            {
                tutorialCueEntity=lesson.FocusEntity;
                TutorialInstruction("Minerals on the move! Let your worker bring them home.");return;
            }
            if(!TutorialWorkerSelected())
            {TutorialSelectEntity(lesson.FocusEntity,"First, select this worker with {0}.",select);return;}
            tutorialCueResource=lesson.FocusResource;
            TutorialInstruction("Point at these minerals and press {0}.",order);
        }

        private void TutorialAttack(string select,bool finalAttack)
        {
            var lesson=Session.Tutorial;
            if(lesson.AcceptedOrderCount>0)
            {
                tutorialCuePoint=lesson.FocusPoint;
                TutorialInstruction(finalAttack?"Your soldiers take it from here. Watch the enemy fall!":"Your squad is marching! Wait until everyone reaches the marker.");return;
            }
            if(!TutorialArmySelected()){TutorialSelectArmy(select);return;}
            if(!Session.AttackMoveMode)
            {
                tutorialCueAction="attack-move";
                TutorialInstruction(Quest.Active?"Choose Attack Move in the console with Trigger / {0}.":"Click Attack Move in the console.",Bindings.MenuConfirmName);return;
            }
            tutorialCuePoint=lesson.FocusPoint;
            TutorialInstruction(finalAttack?"Aim at the enemy marker and press {0}.":"Aim at the ground marker and press {0}.",select);
        }

        private bool TutorialArmySelected()=>Session.Selection.Count>=GuidedTutorial.ArmySize&&Session.View.entities
            .Where(e=>Session.Selection.Contains(e.id)).All(e=>e.owner==Session.LocalPlayer&&!Session.Catalog[e.definition].building&&Session.Catalog[e.definition].role!="worker");
        private void TutorialSelectArmy(string select)
        {
            if(TutorialCancelForSelection(false))return;
            tutorialCueEntity=Session.View.entities.FirstOrDefault(e=>e.owner==Session.LocalPlayer&&!Session.Catalog[e.definition].building
                &&Session.Catalog[e.definition].role!="worker")?.id??0;
            TutorialInstruction("Hold {0} and drag a box around your three soldiers.",select);
        }
        private bool TutorialCancelForSelection(bool worker)
        {
            if(Session.BuildDefinition==null&&!Session.AttackMoveMode)return false;
            tutorialCueEntity=tutorialCueResource=0;tutorialCuePoint=null;tutorialCueAction="cancel";
            TutorialInstruction(worker?"Press {0} to cancel, then select your worker.":"Press {0} to cancel, then select your squad.",Bindings.Name(Quest.Active,"back"));
            return true;
        }
        private bool TutorialWaitForCost(string definition)
        {
            var type=Session.Catalog[definition];
            if(Session.View.minerals>=type.minerals&&Session.View.catalyst>=type.catalyst)return false;
            tutorialCueEntity=Session.View.entities.FirstOrDefault(e=>e.owner==Session.LocalPlayer&&e.order==CommandKind.Gather)?.id??0;
            TutorialInstruction("Your workers are gathering the cost. Give them a moment!");return true;
        }
        private bool TutorialEntitySelected(int id)=>Session.Selection.Count==1&&Session.Selection.Contains(id);
        private bool TutorialWorkerSelected()=>Session.Selection.Count==1&&Session.View.entities.Any(e=>Session.Selection.Contains(e.id)
            &&e.owner==Session.LocalPlayer&&Session.Catalog[e.definition].role=="worker");
        private void TutorialSelectEntity(int id,string key,string select)
        {tutorialCueEntity=id;TutorialInstruction(key,select);}
        private void TutorialText(string title,string description)
        {tutorialTitle.text=title;tutorialDescription.text=description;}
        private void TutorialInstruction(string key,params object[] arguments)
        {((LocalizedText)tutorialInstruction).SetKey(key,arguments);}
    }
}
