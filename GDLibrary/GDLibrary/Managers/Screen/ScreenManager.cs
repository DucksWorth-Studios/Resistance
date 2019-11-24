/*
Function: 		Enables support for split screen and overlapping (e.g. rear-view mirror) camera viewports 
Author: 		NMCG
Version:		1.0
Date Updated:	24/8/17
Bugs:			Need to address bug when one camera view is "inside another" i.e. over-lapping screen space
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary
{
    public class ScreenManager : PausableDrawableGameComponent
    {
        //removes necessity to specify starting screen type layout (e.g. single, multi)
        public ScreenManager(Game game, GraphicsDeviceManager graphics, Integer2 screenResolution,
            ObjectManager objectManager, CameraManager cameraManager, KeyboardManager keyboardManager,
            Keys pauseKey, EventDispatcher eventDispatcher, StatusType statusType)
            : this(game, graphics, screenResolution, ScreenUtility.ScreenType.SingleScreen, objectManager,
                cameraManager,
                keyboardManager, pauseKey, eventDispatcher, statusType)
        {
        }

        public ScreenManager(Game game, GraphicsDeviceManager graphics, Integer2 screenResolution,
            ScreenUtility.ScreenType screenType, ObjectManager objectManager, CameraManager cameraManager,
            KeyboardManager keyboardManager,
            Keys pauseKey, EventDispatcher eventDispatcher, StatusType statusType)
            : base(game, eventDispatcher, statusType)
        {
            ScreenType = screenType;
            this.objectManager = objectManager;
            this.cameraManager = cameraManager;

            //showing and hiding the menu - see ApplyUpdate()
            this.keyboardManager = keyboardManager;
            this.pauseKey = pauseKey;

            this.graphics = graphics;

            //set the resolution using the property
            ScreenResolution = screenResolution;
            FullScreenViewport = new Viewport(0, 0, screenResolution.X, screenResolution.Y);
        }

        public bool ToggleFullScreen()
        {
            //flip the screen mode
            graphics.IsFullScreen = !graphics.IsFullScreen;
            graphics.ApplyChanges();

            //return new state
            return graphics.IsFullScreen;
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            #region Update Views

            //if one camera needs to be drawn on top of another then we need to do a depth sort each time we change the layout
            if (bLayoutDirty && screenType == ScreenUtility.ScreenType.MultiPictureInPicture)
            {
                //sort so that the top-most camera (i.e. closest draw depth to 0 will be the last camera drawn)
                cameraManager.SortByDepth(SortDirectionType.Ascending);
                bLayoutDirty = false;
            }

            //explicit call, as mentioned in Wiki - 2.4. Camera Viewports
            objectManager.Update(gameTime);

            #endregion
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            if (screenType == ScreenUtility.ScreenType.SingleScreen)
                objectManager.Draw(gameTime, cameraManager.ActiveCamera);
            else
                //foreach is enabled by making CameraManager implement IEnumerator
                foreach (var camera in cameraManager)
                    objectManager.Draw(gameTime, camera);

            //reset the viewport to fullscreen
            Game.GraphicsDevice.Viewport = FullScreenViewport;
        }

        //Check Status type to be onlose and disable this
        protected override void HandleInput(GameTime gameTime)
        {
            #region Menu Handling

            //if user presses menu button then either show or hide the menu
            if (keyboardManager != null && keyboardManager.IsFirstKeyPress(pauseKey))
                if (!lose)
                {
                    if (StatusType == StatusType.Off)
                        //will be received by the menu manager and screen manager and set the menu to be shown and game to be paused
                        EventDispatcher.Publish(new EventData(EventActionType.OnStart, EventCategoryType.MainMenu));
                    //if game is playing then publish a pause event
                    else if (StatusType != StatusType.Off)
                        //will be received by the menu manager and screen manager and set the menu to be shown and game to be paused
                        EventDispatcher.Publish(new EventData(EventActionType.OnPause, EventCategoryType.MainMenu));
                }

            #endregion
        }

        #region Fields

        private ScreenUtility.ScreenType screenType;
        private readonly ObjectManager objectManager;
        private readonly CameraManager cameraManager;

        //showing and hiding the menu - see ApplyUpdate()
        private readonly KeyboardManager keyboardManager;
        private readonly Keys pauseKey;

        private bool bLayoutDirty = true;
        private readonly GraphicsDeviceManager graphics;
        private bool lose;

        #endregion

        #region Properties 

        public Integer2 ScreenResolution
        {
            get => new Integer2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            set
            {
                graphics.PreferredBackBufferWidth = value.X;
                graphics.PreferredBackBufferHeight = value.Y;
                //if we forget to apply the changes then our resolution wont be set!
                graphics.ApplyChanges();
            }
        }

        public ScreenUtility.ScreenType ScreenType
        {
            get => screenType;
            set
            {
                screenType = value;
                bLayoutDirty = true;
            }
        }

        public float AspectRatio => (float) graphics.PreferredBackBufferWidth / graphics.PreferredBackBufferHeight;

        public Viewport FullScreenViewport { get; }

        #endregion

        #region Event Handling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.ScreenChanged += EventDispatcher_ScreenChanged;
            base.RegisterForEventHandling(eventDispatcher);
        }

        protected virtual void EventDispatcher_ScreenChanged(EventData eventData)
        {
            //has someone requested a change to the screen layout
            if (eventData.EventType == EventActionType.OnScreenLayoutChange)
                //this.ScreenType = (ScreenUtilityScreenType)eventData.AdditionalEventParameters[0];

                ScreenType = ScreenUtility.ScreenType.SingleScreen;
        }

        //See MenuManager::EventDispatcher_MenuChanged to see how it does the reverse i.e. they are mutually exclusive
        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
            {
                //turn on update and draw i.e. hide the menu
                StatusType = StatusType.Update | StatusType.Drawn;
                lose = false;
            }
            //did the event come from the main menu and is it a start game event
            else if (eventData.EventType == EventActionType.OnPause)
            {
                //turn off update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Off;
                lose = false;
            }
            else if (eventData.EventType == EventActionType.OnLose)
            {
                //turn off update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Off;
                lose = true;
            }
            else if (eventData.EventType == EventActionType.OnWin)
            {
                StatusType = StatusType.Off;
                lose = true;
            }
        }

        #endregion
    }
}