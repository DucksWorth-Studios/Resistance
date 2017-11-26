using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDApp
{
    public class MyAppMenuManager : MenuManager
    {
        public MyAppMenuManager(Game game, MouseManager mouseManager, KeyboardManager keyboardManager, CameraManager cameraManager,
            SpriteBatch spriteBatch, EventDispatcher eventDispatcher, 
            StatusType statusType) : base(game, mouseManager, keyboardManager, cameraManager, spriteBatch, eventDispatcher, statusType)
        {

        }

        #region Event Handling
        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //call base method to show/hide the menu
            base.EventDispatcher_MenuChanged(eventData);

            //then generate sound events particular to your game e.g. play background music in a menu
            if (eventData.EventType == EventActionType.OnPlay)
            {
                //add event to stop background menu music here...
            }
            else if (eventData.EventType == EventActionType.OnPause)
            {
                //add event to play background menu music here...
            }
        }
        #endregion

        protected override void HandleMouseOver(UIObject currentUIObject)
        {
            //add event to play mouse over sound here...
        }



        //add the code here to say how click events are handled by your code
        protected override void HandleMouseClick(UIObject clickedUIObject)
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

            //add event to play mouse click sound here...

        }

        private void DoStart()
        {
            //will be received by the menu manager and screen manager and set the menu to be shown and game to be paused
            EventDispatcher.Publish(new EventData(EventActionType.OnStart, EventCategoryType.MainMenu));
        }

        private void DoExit()
        {
            this.Game.Exit();
            //to do - add exit Yes|No comfirmation
        }

    }
}
