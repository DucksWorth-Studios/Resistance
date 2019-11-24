using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class MenuManager : PausableDrawableGameComponent
    {
        public MenuManager(Game game, MouseManager mouseManager, KeyboardManager keyboardManager,
            CameraManager cameraManager, SpriteBatch spriteBatch, EventDispatcher eventDispatcher,
            StatusType statusType)
            : base(game, eventDispatcher, statusType)
        {
            menuDictionary = new Dictionary<string, List<UIObject>>();

            //used to listen for input
            this.mouseManager = mouseManager;
            this.keyboardManager = keyboardManager;
            this.cameraManager = cameraManager;

            //used to render menu and UI elements
            this.spriteBatch = spriteBatch;
        }

        public void Add(string menuSceneID, UIObject actor)
        {
            if (menuDictionary.ContainsKey(menuSceneID))
            {
                menuDictionary[menuSceneID].Add(actor);
            }
            else
            {
                var newList = new List<UIObject>();
                newList.Add(actor);
                menuDictionary.Add(menuSceneID, newList);
            }

            //if the user forgets to set the active list then set to the sceneID of the last added item
            if (ActiveList == null) SetActiveList(menuSceneID);
        }

        public UIObject Find(string menuSceneID, Predicate<UIObject> predicate)
        {
            if (menuDictionary.ContainsKey(menuSceneID)) return menuDictionary[menuSceneID].Find(predicate);
            return null;
        }

        public bool Remove(string menuSceneID, Predicate<UIObject> predicate)
        {
            var foundUIObject = Find(menuSceneID, predicate);

            if (foundUIObject != null)
                return menuDictionary[menuSceneID].Remove(foundUIObject);

            return false;
        }

        //e.g. return all the actor2D objects associated with the "main menu" or "audio menu"
        public List<UIObject> FindAllBySceneID(string menuSceneID)
        {
            if (menuDictionary.ContainsKey(menuSceneID)) return menuDictionary[menuSceneID];
            return null;
        }

        public bool SetActiveList(string menuSceneID)
        {
            if (menuDictionary.ContainsKey(menuSceneID))
            {
                ActiveList = menuDictionary[menuSceneID];
                Console.WriteLine(menuSceneID);
                return true;
            }

            Console.WriteLine(menuSceneID);
            return false;
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            if (ActiveList != null)
            {
                //update all the updateable menu items (e.g. make buttons pulse etc)
                foreach (var currentUIObject in ActiveList)
                    if ((currentUIObject.GetStatusType() & StatusType.Update) != 0) //if update flag is set
                        currentUIObject.Update(gameTime);
                //check for mouse over and mouse click on a menu item
                CheckMouseOverAndClick(gameTime);
            }
        }

        private void CheckMouseOverAndClick(GameTime gameTime)
        {
            foreach (var currentUIObject in ActiveList)
                //only handle mouseover and mouse click for buttons
                if (currentUIObject.ActorType == ActorType.UIButton)
                {
                    //add an if to check that this is a interactive UIButton object
                    if (currentUIObject.Transform.Bounds.Intersects(mouseManager.Bounds))
                    {
                        //if mouse is over a new ui object then set old to "IsMouseOver=false"
                        if (OldUIObjectMouseOver != null && OldUIObjectMouseOver != currentUIObject)
                            OldUIObjectMouseOver.MouseOverState.Update(false);

                        //update the current state of the currently mouse-over'ed ui object
                        currentUIObject.MouseOverState.Update(true);

                        //apply any mouse over or mouse click actions
                        HandleMouseOver(currentUIObject, gameTime);
                        if (mouseManager.IsLeftButtonClickedOnce())
                            HandleMouseClick(currentUIObject, gameTime);

                        //store the current as old for the next update
                        OldUIObjectMouseOver = currentUIObject;
                    }
                    else
                    {
                        //set the mouse as not being over the current ui object
                        currentUIObject.MouseOverState.Update(false);
                    }
                }
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            if (ActiveList != null)
            {
                spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend, SamplerState.LinearClamp,
                    DepthStencilState.Default, RasterizerState.CullCounterClockwise);
                foreach (var currentUIObject in ActiveList)
                    if ((currentUIObject.GetStatusType() & StatusType.Drawn) != 0) //if drawn flag is set
                        currentUIObject.Draw(gameTime, spriteBatch);
                spriteBatch.End();
            }
        }

        protected virtual void HandleMouseOver(UIObject currentUIObject, GameTime gameTime)
        {
            //developer implements in subclass of MenuManager - see MyMenuManager.cs
        }

        protected virtual void HandleMouseClick(UIObject clickedUIObject, GameTime gameTime)
        {
            //developer implements in subclass of MenuManager - see MyMenuManager.cs
        }

        #region Fields

        //stores the actors shown for a particular menu scene (e.g. for the "main menu" scene we would have actors: startBtn, ExitBtn, AudioBtn)
        private readonly Dictionary<string, List<UIObject>> menuDictionary;

        private readonly SpriteBatch spriteBatch;
        private readonly MouseManager mouseManager;
        private KeyboardManager keyboardManager;
        private readonly CameraManager cameraManager;

        //tracks last object mouse-ed over by the cursor

        #endregion

        #region Properties

        protected UIObject OldUIObjectMouseOver { get; private set; }

        public List<UIObject> ActiveList { get; private set; }

        #endregion

        #region Event Handling

        //See ScreenManager::EventDispatcher_MenuChanged to see how it does the reverse i.e. they are mutually exclusive
        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
            {
                //turn off update and draw i.e. hide the menu
                StatusType = StatusType.Off;
                //hide the mouse - comment out this line if you want to see the mouse cursor in-game
                Game.IsMouseVisible = false;
            }
            //did the event come from the main menu and is it a start game event
            else if (eventData.EventType == EventActionType.OnPause)
            {
                //turn on update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Update | StatusType.Drawn;
                //show the mouse
                Game.IsMouseVisible = true;
            }
            else if (eventData.EventType == EventActionType.OnLose)
            {
                SetActiveList("lose-screen");
                //turn on update and draw i.e. show the menu since the game is paused
                Console.WriteLine("HELLO FRIEND");
                StatusType = StatusType.Update | StatusType.Drawn;
                //Set the ActiveList BeforeCalling this

                //show the mouse
                Game.IsMouseVisible = true;
            }
            else if (eventData.EventType == EventActionType.OnWin)
            {
                SetActiveList("win-screen");
                StatusType = StatusType.Update | StatusType.Drawn;
                Game.IsMouseVisible = true;
            }

            //set the mouse to look directly forward otherwise the camera would move forward based on some random mouse orientation
            mouseManager.SetPosition(cameraManager.ActiveCamera.ViewportCentre);
        }

        protected void EventDispatcher_Onlose(EventData eventData)
        {
            SetActiveList("lose-screen");
        }

        #endregion
    }
}