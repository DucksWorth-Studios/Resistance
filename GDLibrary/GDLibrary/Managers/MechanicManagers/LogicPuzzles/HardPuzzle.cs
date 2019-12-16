using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class HardPuzzle : LogicTemplate
    {
        private bool XORGate;
        private bool ANDGate;
        private bool NOTGate;
        private bool NANDGate1;
        private bool NANDGate2;

        public HardPuzzle(Game game, EventDispatcher eventDispatcher) : base(game, eventDispatcher)
        {
            this.XORGate = false;
            this.ANDGate = false;
            this.NOTGate = false;
            this.NANDGate1 = false;
            this.NANDGate2 = false;
        }

        public override void changeState(string ID)
        {
            base.changeState(ID);
        }

        protected override void checkStatus()
        {
            if (!IsSolved)
            {
                //XOR GATE
                if ((this.switchFour || this.switchTwo) && !(this.switchTwo && this.switchFour) && !this.XORGate)
                {
                    this.XORGate = true;
                    //Publish Event
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "hard-gate-1" }));
                }
                else if (((this.switchTwo && this.switchFour) || (!this.switchTwo && !this.switchFour)) && this.XORGate)
                {
                    this.XORGate = false;
                    //Publish Event
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "hard-gate-1" }));
                }

                //AND GATE
                if (this.XORGate && this.switchOne && !this.ANDGate)
                {
                    this.ANDGate = true;
                    //Publish Event
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "hard-gate-2" }));
                }
                else if ((!this.XORGate || !this.switchOne) && this.ANDGate)
                {
                    this.ANDGate = false;
                    //Publish Event
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "hard-gate-2" }));
                }

                //FIRST NAND GATE
                if ((this.switchFour && this.switchThree) && this.NANDGate1)
                {
                    this.NANDGate1 = false;
                    //Publish Event
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "hard-gate-3" }));
                }
                else if ((!this.switchFour || !this.switchThree) && !this.NANDGate1)
                {
                    this.NANDGate1 = true;
                    //Publish Event
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "hard-gate-3" }));
                }

                //NOT GATE
                if (this.ANDGate && this.NOTGate)
                {
                    this.NOTGate = false;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "hard-gate-4" }));
                }
                else if ((this.ANDGate && !this.NOTGate))
                {
                    this.NOTGate = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "hard-gate-4" }));
                }



                //SECOND NAND GATE
                if ((this.NANDGate1 && this.NOTGate) && this.NANDGate2)
                {
                    this.NANDGate2 = false;
                    this.IsSolved = true;

                    //sets active camera to door cutscene camera
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, new object[] { "Door Cutscene Camera" }));
                    EventDispatcher.Publish(new EventData(EventActionType.OnObjective, EventCategoryType.Objective));
                    //sends off the event to ensure camera switches back to player once event has concluded
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Cutscene, new object[] { 3, "collidable first person camera" }));
                    EventDispatcher.Publish(new EventData(EventActionType.OpenDoor, EventCategoryType.Animator));
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "hard-gate-5" }));
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "hard-gate-6" }));


                }
                else if ((!this.NOTGate || !this.NANDGate1) && !this.NANDGate2)
                {
                    this.NANDGate2 = true;
                    //Publish Event
                    
                }
            }
        }

    }
}
