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
                    //Send Event To light up Gate One
                    Console.Write("Gate One");
                }
                else if((!this.switchOne || !this.switchFour) && this.gateOne)
                {
                    this.gateOne = false;
                    //Send Event to delight Gate One;
                    Console.Write("Gate One Off");

                }

                //XOR Gate
                if ( (this.switchThree || this.switchTwo) && !(this.switchThree && this.switchTwo) && !this.gateTwo)
                {
                    this.gateTwo = true;
                    //Send Event To light up Gate Two
                    Console.Write("Gate Two");

                }
                else if (((this.switchThree && this.switchFour) || (!this.switchThree && !this.switchFour)) && this.gateTwo)
                {
                    this.gateTwo = false;
                    //Send Event to delight Gate Two;
                    Console.Write("Gate Two OFF");

                }

                //Tri-State gate
                if (this.gateOne && this.switchTwo && !this.gateThree)
                {
                    this.gateThree = true;
                    //Event for Gate Three
                    Console.Write("Gate Three");

                }
                else if((!this.gateOne || !this.switchTwo) && this.gateThree)
                {
                    this.gateThree = false;
                    //Event For Gate Three
                    Console.Write("Gate Three Off");

                }

                if (this.gateThree && this.gateTwo && !this.gateFour)
                {
                    this.gateFour = true;
                    this.IsSolved = true;
                    Console.Write("Gate Four");

                }
                else if((!this.gateThree || !this.gateTwo) && this.gateFour)
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
