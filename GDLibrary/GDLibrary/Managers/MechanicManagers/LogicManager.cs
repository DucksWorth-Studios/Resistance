/*
 Author: Tomas
 Manages the state of the logic puzzle and handles any chanes made to it via the interactive element of it
 */


using System;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class LogicManager : GameComponent
    {
        private bool gateFour;

        private bool gateOne;
        private bool gateThree;
        private bool gateTwo;

        private bool IsSolved;
        private bool switchFour;
        private bool switchOne;
        private bool switchThree;
        private bool switchTwo;

        public LogicManager(Game game, EventDispatcher eventDispatcher) : base(game)
        {
            switchOne = false;
            switchTwo = false;
            switchThree = false;
            switchFour = false;
            IsSolved = false;

            gateOne = false;
            gateTwo = false;
            gateThree = false;
            gateFour = false;

            RegisterForHandling(eventDispatcher);
        }

        private void RegisterForHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.Reset += Reset;
        }

        private void Reset(EventData eventData)
        {
            switchOne = false;
            switchTwo = false;
            switchThree = false;
            switchFour = false;
            IsSolved = false;

            gateOne = false;
            gateTwo = false;
            gateThree = false;
            gateFour = false;
        }

        public void changeState(string ID)
        {
            switch (ID)
            {
                case "switch-1":
                    if (switchOne)
                        switchOne = false;
                    else
                        switchOne = true;
                    break;
                case "switch-2":
                    if (switchTwo)
                        switchTwo = false;
                    else
                        switchTwo = true;
                    break;
                case "switch-3":
                    if (switchThree)
                        switchThree = false;
                    else
                        switchThree = true;
                    break;
                case "switch-4":
                    if (switchFour)
                        switchFour = false;
                    else
                        switchFour = true;
                    break;
            }
        }


        /**
         * The Brain of the manager it contains all switches and all changes that may occur to the models are fired off from here
         */
        public void checkStatus()
        {
            if (!IsSolved)
            {
                //AND Gate
                if (switchOne && switchFour && !gateOne)
                {
                    gateOne = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle,
                        new object[] {"gate-1"}));
                    Console.WriteLine("Gate One");
                }
                else if ((!switchOne || !switchFour) && gateOne)
                {
                    gateOne = false;
                    //Send Event to delight Gate One;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle,
                        new object[] {"gate-1"}));
                    Console.WriteLine("Gate One Off");
                }

                //XOR Gate
                if ((switchThree || switchTwo) && !(switchThree && switchTwo) && !gateTwo)
                {
                    gateTwo = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle,
                        new object[] {"gate-2"}));
                    Console.WriteLine("Gate Two");
                }
                else if ((switchThree && switchTwo || !switchThree && !switchTwo) && gateTwo)
                {
                    gateTwo = false;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle,
                        new object[] {"gate-2"}));
                    Console.WriteLine("Gate Two OFF");
                }

                //Tri-State gate
                if (gateOne && switchTwo && !gateThree)
                {
                    gateThree = true;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle,
                        new object[] {"gate-3"}));
                    Console.WriteLine("Gate Three");
                }
                else if ((!gateOne || !switchTwo) && gateThree)
                {
                    gateThree = false;
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle,
                        new object[] {"gate-3"}));
                    Console.WriteLine("Gate Three Off");
                }

                //AND Gate
                if (gateThree && gateTwo && !gateFour)
                {
                    gateFour = true;
                    IsSolved = true;

                    //Changes the final Gate state
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle,
                        new object[] {"gate-4"}));

                    //Changes the end light to signify puzzle solved
                    EventDispatcher.Publish(new EventData(EventActionType.OnLight, EventCategoryType.LogicPuzzle,
                        new object[] {"gate-5"}));

                    //sets active camera to door cutscene camera
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera,
                        new object[] {"Door Cutscene Camera"}));

                    //sends off the event to ensure camera switches back to player once event has concluded
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Cutscene,
                        new object[] {3, "collidable first person camera"}));
                    EventDispatcher.Publish(new EventData(EventActionType.OpenDoor, EventCategoryType.Animator));

                    EventDispatcher.Publish(new EventData(EventActionType.OnObjective, EventCategoryType.Objective));
                }
                else if ((!gateThree || !gateTwo) && gateFour)
                {
                    gateFour = false;
                }
            }
        }

        //gets current state of puzzle solved or unsolved(true/false)
        public bool getState()
        {
            return IsSolved;
        }


        //Every turn it checks the status of the game to see if any changes are made
        public override void Update(GameTime gameTime)
        {
            if (!IsSolved) checkStatus();
            base.Update(gameTime);
        }
    }
}