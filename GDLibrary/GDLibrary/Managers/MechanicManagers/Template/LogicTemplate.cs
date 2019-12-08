using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDLibrary
{
    public class LogicTemplate : GameComponent
    {
        protected bool IsSolved;

        protected bool switchOne;
        protected bool switchTwo;
        protected bool switchThree;
        protected bool switchFour;


        public LogicTemplate(Game game, EventDispatcher eventDispatcher) : base(game)
        {
            this.switchOne = false;
            this.switchTwo = false;
            this.switchThree = false;
            this.switchFour = false;

            this.IsSolved = false;
        }

        protected virtual void RegisterForHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.Reset += Reset;
        }

        protected virtual void Reset(EventData eventData)
        {

        }

        public virtual void changeState(string ID)
        {
            switch (ID)
            {
                case "switch-1":
                    if (this.switchOne)
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


        /**
         * The Brain of the manager it contains all switches and all changes that may occur to the models are fired off from here
         */
        protected virtual void checkStatus()
        {

        }

        //gets current state of puzzle solved or unsolved(true/false)
        protected bool getState()
        {
            return this.IsSolved;
        }


        //Every turn it checks the status of the game to see if any changes are made
        public override void Update(GameTime gameTime)
        {
            if (!this.IsSolved)
            {
                checkStatus();
            }
            base.Update(gameTime);
        }
    }
}
