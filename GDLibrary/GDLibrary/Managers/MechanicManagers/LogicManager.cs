using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace GDLibrary
{ 
    public class LogicManager : GameComponent
    {
        private bool switchOne;
        private bool switchTwo;
        private bool switchThree;
        private bool switchFour;

        private bool IsSolved;

        private bool gateOne;
        private bool gateTwo;
        private bool gateThree;
        private bool gateFour;

        public LogicManager(Game game) : base(game)
        {
            this.switchOne = false;
            this.switchTwo = false;
            this.switchThree = false;
            this.switchFour = false;
            this.IsSolved = false;

            this.gateOne = false;
            this.gateTwo = false;
            this.gateThree = false;
            this.gateFour = false;
        }

        public void changeState(string ID)
        {
            switch (ID)
            {
                case"switch-1":
                    if(this.switchOne)
                    {
                        this.switchOne = false;
                    }
                    else
                    {
                        this.switchOne = true;
                    }
                    break;
                case "switch-2":
                    if (this.switchTwo)
                    {
                        this.switchTwo = false;
                    }
                    else
                    {
                        this.switchTwo = true;
                    }
                    break;
                case "switch-3":
                    if (this.switchThree)
                    {
                        this.switchThree = false;
                    }
                    else
                    {
                        this.switchThree = true;
                    }
                    break;
                case "switch-4":
                    if (this.switchFour)
                    {
                        this.switchFour = false;
                    }
                    else
                    {
                        this.switchFour = true;
                    }
                    break;

                default:
                    break;
            }

        }

        public void checkStatus()
        {

            if(!IsSolved)
            {
                //AND Gate
                if(this.switchOne && this.switchFour && !this.gateOne)
                {
                    this.gateOne = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "gate-1" }));
                    Console.WriteLine("Gate One");
                }
                else if((!this.switchOne || !this.switchFour) && this.gateOne)
                {
                    this.gateOne = false;
                    //Send Event to delight Gate One;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight,EventCategoryType.LogicPuzzle,new object[] { "gate-1" }));
                    Console.WriteLine("Gate One Off");

                }

                //XOR Gate
                if ((this.switchThree || this.switchTwo) && !(this.switchThree && this.switchTwo) && !this.gateTwo)
                {
                    this.gateTwo = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "gate-2" }));
                    Console.WriteLine("Gate Two");

                }
                else if (((this.switchThree && this.switchTwo) || (!this.switchThree && !this.switchTwo)) && this.gateTwo)
                {
                    this.gateTwo = false;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "gate-2" }));
                    Console.WriteLine("Gate Two OFF");

                }

                //Tri-State gate
                if (this.gateOne && this.switchTwo && !this.gateThree)
                {
                    this.gateThree = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "gate-3" }));
                    Console.WriteLine("Gate Three");

                }
                else if ((!this.gateOne || !this.switchTwo) && this.gateThree)
                {
                    this.gateThree = false;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "gate-3" }));
                    Console.WriteLine("Gate Three Off");

                }

                if (this.gateThree && this.gateTwo && !this.gateFour)
                {
                    this.gateFour = true;
                    this.IsSolved = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "gate-4" }));
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle, new object[] { "gate-5" }));
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, new object[] { "Door Cutscene Camera" }));
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive,EventCategoryType.Cutscene, new object[] {10, "collidable first person camera" }));
                    Console.WriteLine("Gate Four Logic Solved");

                }
                else if ((!this.gateThree || !this.gateTwo) && this.gateFour)
                {
                    this.gateFour = false;
                }
            }

        }

        public bool getState()
        {
            return this.IsSolved;
        }

        public override void Update(GameTime gameTime)
        {
            if(!this.IsSolved)
            {
                checkStatus();
            }
            base.Update(gameTime);
        }
    }
}
