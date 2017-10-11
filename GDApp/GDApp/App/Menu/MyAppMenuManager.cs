using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDApp
{
    public class MyAppMenuManager : MenuManager
    {
        public MyAppMenuManager(Game game, MouseManager mouseManager, KeyboardManager keyboardManager, 
            SpriteBatch spriteBatch, StatusType statusType) : base(game, mouseManager, keyboardManager, spriteBatch, statusType)
        {

        }

        protected override void HandleMouseOver(UIObject currentUIObject)
        {
            //to do - play a sound - click sound
        }



        //add the code here to say how click events are handled by your code
        protected override void HandleMouseClick(UIObject clickedUIObject)
        {
            if(clickedUIObject.ActorType == ActorType.UIButton)
            {
                //notice that the IDs are the same as the button IDs specified when we created the menu in Main::AddMenuElements()
                switch (clickedUIObject.ID)
                {
                    case "startbtn":
                        DoStart();
                        break;

                    case "exitbtn":
                        DoExit();
                        break;

                    case "audiobtn":
                        SetActiveList("audio menu"); //use sceneIDs specified when we created the menu scenes in Main::AddMenuElements()
                        break;

                    case "volumeUpbtn":
                        //to do - generate an event to increase volume
                        break;

                    case "volumeDownbtn":
                        //to do - generate an event to decrease volume
                        break;

                    case "volumeMutebtn":
                        //to do - generate an event to mute volume
                        break;

                    case "backbtn":
                        SetActiveList("main menu"); //use sceneIDs specified when we created the menu scenes in Main::AddMenuElements()
                        break;

                    case "controlsbtn":
                        SetActiveList("controls menu"); //use sceneIDs specified when we created the menu scenes in Main::AddMenuElements()
                        break;

                    default:
                        break;
                }
                
            }
        }     

        private void DoStart()
        {
            //to do - generate an event to pause menu and unpause game
        }

        private void DoExit()
        {
            this.Game.Exit();
            //to do - add exit Yes|No comfirmation
        }

    }
}
