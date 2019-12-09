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
            this.NOTGate = true;
            this.NANDGate1 = true;
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
                if ((this.switchOne || this.switchThree) && !(this.switchThree && this.switchOne) && !this.XORGate)
                {
                    this.XORGate = true;
                    //Publish Event
                    Console.WriteLine("HARD XOR ON");
                }
                else if (((this.switchThree && this.switchOne) || (!this.switchThree && !this.switchOne)) && this.XORGate)
                {
                    this.XORGate = false;
                    //Publish Event
                    Console.WriteLine("HARD XOROFF");
                }

                //AND GATE
                if (this.XORGate && this.switchFour && !this.ANDGate)
                {
                    this.ANDGate = true;
                    //Publish Event
                    Console.WriteLine("HARD AND GATE ON");
                }
                else if ((!this.XORGate || !this.switchFour) && this.ANDGate)
                {
                    this.ANDGate = false;
                    //Publish Event
                    Console.WriteLine("HARD AND GATE ON");
                }

                //FIRST NAND GATE
                if ((this.switchOne && this.switchTwo) && this.NANDGate1)
                {
                    this.NANDGate1 = false;
                    //Publish Event
                    Console.WriteLine("HARD FIRST NAND GATE ON");
                }
                else if ((!this.switchOne || !this.switchTwo) && !this.NANDGate1)
                {
                    this.NANDGate1 = true;
                    //Publish Event
                    Console.WriteLine("HARD FIRST NAND GATE ON");
                }

                //NOT GATE
                if (this.ANDGate && this.NOTGate)
                {
                    this.NOTGate = false;
                }
                else if ((this.ANDGate && !this.NOTGate))
                {
                    this.NOTGate = true;
                }



                //SECOND NAND GATE
                if ((this.NANDGate1 && this.NOTGate) && this.NANDGate2)
                {
                    this.NANDGate2 = false;
                    this.IsSolved = true;

                    //sets active camera to door cutscene camera
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, new object[] { "Door Cutscene Camera" }));

                    //sends off the event to ensure camera switches back to player once event has concluded
                    EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Cutscene, new object[] { 3, "collidable first person camera" }));
                    EventDispatcher.Publish(new EventData(EventActionType.OpenDoor, EventCategoryType.Animator));

                    Console.WriteLine("HARD SECOND NAND GATE ON");
                }
                else if ((!this.NOTGate || !this.NANDGate1) && !this.NANDGate2)
                {
                    this.NANDGate2 = true;
                    //Publish Event
                    Console.WriteLine("HARD FIRST NAND GATE ON");
                }
            }
        }

    }
}
