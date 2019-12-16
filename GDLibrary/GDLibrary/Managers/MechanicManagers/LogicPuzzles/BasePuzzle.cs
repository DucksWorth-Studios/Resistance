using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class BasePuzzle : LogicTemplate
    {
        private bool gateOne;
        private bool gateTwo;
        private bool gateThree;
        private bool gateFour;

        public BasePuzzle(Game game, EventDispatcher eventDispatcher) : base(game, eventDispatcher)
        {
            this.gateOne = false;
            this.gateTwo = false;
            this.gateThree = false;
            this.gateFour = false;

            RegisterForHandling(eventDispatcher);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void changeState(string ID)
        {
            base.changeState(ID);
        }

        protected override void checkStatus()
        {
            if (!IsSolved)
            {
                //AND Gate
                if (this.switchOne && this.switchFour && !this.gateOne)
                {
                    this.gateOne = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "base-gate-1" }));
                    Console.WriteLine("BASE Gate One");
                }
                else if ((!this.switchOne || !this.switchFour) && this.gateOne)
                {
                    this.gateOne = false;
                    //Send Event to delight Gate One;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "base-gate-1" }));
                    

                }

                //XOR Gate
                if ((this.switchThree || this.switchTwo) && !(this.switchThree && this.switchTwo) && !this.gateTwo)
                {
                    this.gateTwo = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "base-gate-2" }));
                    

                }
                else if (((this.switchThree && this.switchTwo) || (!this.switchThree && !this.switchTwo)) && this.gateTwo)
                {
                    this.gateTwo = false;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "base-gate-2" }));
                    
                }

                //Tri-State gate
                if (this.gateOne && this.switchTwo && !this.gateThree)
                {
                    this.gateThree = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "base-gate-3" }));
                    

                }
                else if ((!this.gateOne || !this.switchTwo) && this.gateThree)
                {
                    this.gateThree = false;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "base-gate-3" }));
                    

                }

                //AND Gate
                if (this.gateThree && this.gateTwo && !this.gateFour)
                {
                    this.gateFour = true;
                    this.IsSolved = true;

                    EventDispatcher.Publish(new EventData(EventActionType.OnObjective, EventCategoryType.Objective));



                    //Changes the final Gate state
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "base-gate-4" }));

                    //Changes the end light to signify puzzle solved
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "base-gate-5" }));

                    //sets active camera to door cutscene camera
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, new object[] { "Door Cutscene Camera" }));

                    //sends off the event to ensure camera switches back to player once event has concluded
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Cutscene, new object[] { 3, "collidable first person camera" }));
                    EventDispatcher.Publish(new EventData(EventActionType.OpenDoor, EventCategoryType.Animator));

                    EventDispatcher.Publish(new EventData(EventActionType.OnObjective, EventCategoryType.Objective));

                }
                else if ((!this.gateThree || !this.gateTwo) && this.gateFour)
                {
                    this.gateFour = false;
                }
            }

        }

    }
}
