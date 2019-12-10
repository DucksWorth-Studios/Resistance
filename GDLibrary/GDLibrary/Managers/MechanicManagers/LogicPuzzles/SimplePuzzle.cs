using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class SimplePuzzle : LogicTemplate
    {
        private bool NOTGate;
        private bool XORGate;
        private bool ANDGate1;
        private bool ANDGate2;

        public SimplePuzzle(Game game, EventDispatcher eventDispatcher) : base(game, eventDispatcher)
        {
            this.NOTGate = false;
            this.XORGate = false;
            this.ANDGate1 = false;
            this.ANDGate2 = false;
        }

        public override void changeState(string ID)
        {
            base.changeState(ID);
        }

        protected override void checkStatus()
        {
            if(!IsSolved)
            {

                //XOR Gate
                if ((this.switchTwo || this.switchFour) && !(this.switchTwo && this.switchFour) && !this.XORGate)
                {
                    this.XORGate = true;
                    //Publish Event
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "simple-gate-1" }));
                }
                else if (((this.switchTwo && this.switchFour) || (!this.switchTwo && !this.switchFour)) && this.XORGate)
                {
                    this.XORGate = false;
                    //Publish Event
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "simple-gate-1" }));
                }

                //Not Gate
                if(this.switchThree && this.NOTGate)
                {
                    this.NOTGate = false;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "simple-gate-2" }));

                }
                else if(!this.switchThree && !this.NOTGate)
                {
                    this.NOTGate = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "simple-gate-2" }));

                }

                //First AND Gate
                if (this.NOTGate && this.switchOne && !this.ANDGate1)
                {
                    this.ANDGate1 = true;
                    //Publish Event
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "simple-gate-3" }));

                }
                else if((!this.NOTGate || !this.switchOne) && this.ANDGate1)
                {
                    this.ANDGate1 = false;
                    //Publish Event
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "simple-gate-3" }));

                }


                //Second AND Gate
                if (this.ANDGate1 && this.XORGate && !this.ANDGate2)
                {
                    this.ANDGate2 = true;
                    this.IsSolved = true;

                    //sets active camera to door cutscene camera
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, new object[] { "Door Cutscene Camera" }));

                    //sends off the event to ensure camera switches back to player once event has concluded
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Cutscene, new object[] { 3, "collidable first person camera" }));
                    EventDispatcher.Publish(new EventData(EventActionType.OpenDoor, EventCategoryType.Animator));
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "simple-gate-4" }));
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "simple-gate-5" }));

                }
                else if ((!this.ANDGate1 || !this.XORGate) && this.ANDGate2)
                {
                    this.ANDGate2 = false;
                    //Publish Event
                    
                }

            }
        }

    }
}
