using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GDApp
{
    public class MyAppMenuManager : MenuManager
    {
        private int lastPlayTime;
        string oldID = "";

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
            if (eventData.EventType == EventActionType.OnStart)
            {
                ////add event to stop background menu music here...
                //object[] additionalParameters = { "menu elevator music", 1 };
                //EventDispatcher.Publish(new EventData(EventActionType.OnStop, EventCategoryType.Sound2D, additionalParameters));
                this.SetActiveList("pause menu");
            }
            else if (eventData.EventType == EventActionType.OnPause)
            {
                //add event to play background menu music here...
                //object[] additionalParameters = { "menu elevator music" };
                //EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
            }
            else if (eventData.EventType == EventActionType.OnLose)
            {
                //SetActiveList("lose-screen");
                //turn off update and draw i.e. show the menu since the game is paused
                //this.StatusType = StatusType.Off;
            }
            else if (eventData.EventType == EventActionType.OnWin)
            {

                //this.SetActiveList("win-screen");
            }
        }
        #endregion

        protected override void HandleMouseOver(UIObject currentUIObject, GameTime gameTime)
        {
            //accumulate time over menu item
            //if greater than X milliseconds then play a boing and reset accumulated time
            object[] additionalParameters = { "morse_button1" };

            //EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
            if(oldID != currentUIObject.GetID())
            {
                EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.SoundStart, additionalParameters));
                oldID = currentUIObject.GetID();
            }
        }



        //add the code here to say how click events are handled by your code
        protected override void HandleMouseClick(UIObject clickedUIObject, GameTime gameTime)
        {
                //notice that the IDs are the same as the button IDs specified when we created the menu in Main::AddMenuElements()
                switch (clickedUIObject.ID)
                {
                    case "startbtn":
                    SetActiveList("story menu");
                    break;

                    case "resumebtn":
                        DoStart();
                        EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.mouseLock));
                        break;

                    case "exitbtn":
                        DoExit();
                        break;

                    case "audiobtn":
                        SetActiveList("audio menu"); //use sceneIDs specified when we created the menu scenes in Main::AddMenuElements()
                        break;

                    case "volumeUpbtn":
                        { //curly brackets scope additionalParameters to be local to this case
                            object[] additionalParameters = { 0.25f };
                            EventDispatcher.Publish(new EventData(EventActionType.OnVolumeUp, EventCategoryType.GlobalSound, additionalParameters));
                            EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.volume, additionalParameters));
                        }
                        break;

                    case "volumeDownbtn":
                        {  
                            object[] additionalParameters = { -0.25f };
                            EventDispatcher.Publish(new EventData(EventActionType.OnVolumeDown, EventCategoryType.GlobalSound, additionalParameters));
                            EventDispatcher.Publish(new EventData(EventActionType.OnVolumeChange, EventCategoryType.volume, additionalParameters));
                    }
                        break;

                    case "volumeMutebtn":
                        {
                            object[] additionalParameters = { 0.0f, "Xact category name for game sounds goes here..."};
                            EventDispatcher.Publish(new EventData(EventActionType.OnMute, EventCategoryType.GlobalSound, additionalParameters));
                        }
                        break;

                    case "volumeUnMutebtn":
                    {
                        object[] additionalParameters = { 0.5f, "Xact category name for game sounds goes here..." };
                        EventDispatcher.Publish(new EventData(EventActionType.OnUnMute, EventCategoryType.GlobalSound, additionalParameters));
                    }
                    break;

                    case "backbtn":
                        SetActiveList("main menu"); //use sceneIDs specified when we created the menu scenes in Main::AddMenuElements()
                        break;
                    case "main-Menu":
                        SetActiveList("main menu"); //use sceneIDs specified when we created the menu scenes in Main::AddMenuElements()
                        DoRestart();
                        break;

                    case "controlsbtn":
                        SetActiveList("controls menu"); //use sceneIDs specified when we created the menu scenes in Main::AddMenuElements()
                        break;
                    case "restart-Button":
                    
                    DoRestart();
                    EventDispatcher.Publish(new EventData(EventActionType.OnLose, EventCategoryType.mouseLock));
                    EventDispatcher.Publish(new EventData(EventActionType.OnStart, EventCategoryType.MainMenu));
                    EventDispatcher.Publish(new EventData(EventActionType.OnClick, EventCategoryType.StartSound));
                    //SetActiveList("main menu"); //use sceneIDs specified when we created the menu scenes in Main::AddMenuElements()
                    break;
                case "playbtn":
                    DoStart();
                    EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.mouseLock));
                    EventDispatcher.Publish(new EventData(EventActionType.OnClick, EventCategoryType.StartSound));
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

        private void DoRestart()
        {
            EventDispatcher.Publish(new EventData(EventActionType.OnRestart, EventCategoryType.Reset));
            
        }

        private void DoExit()
        {
            this.Game.Exit();
        }

    }
}
