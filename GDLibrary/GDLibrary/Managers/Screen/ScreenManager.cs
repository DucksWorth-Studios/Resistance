/*
Function: 		Enables support for split screen and overlapping (e.g. rear-view mirror) camera viewports 
Author: 		NMCG
Version:		1.0
Date Updated:	24/8/17
Bugs:			Need to address bug when one camera view is "inside another" i.e. over-lapping screen space
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class ScreenManager : DrawableGameComponent
    {
        #region Variables
        private ScreenUtility.ScreenType screenType;
        private ObjectManager objectManager;
        private CameraManager cameraManager;
        private bool bFirstTime = true;
        private GraphicsDeviceManager graphics;
        #endregion

        #region Properties 
        public Integer2 ScreenResolution
        {
            get
            {
                return new Integer2(this.graphics.PreferredBackBufferWidth, this.graphics.PreferredBackBufferHeight);
            }
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
            get
            {
                return this.screenType;
            }
            set
            {
                this.screenType = value;
            }
        }
        #endregion

        public ScreenManager(Game game, GraphicsDeviceManager graphics, Integer2 screenResolution, ScreenUtility.ScreenType screenType, ObjectManager objectManager, CameraManager cameraManager)
            : base(game)
        {
            
            this.screenType = screenType;
            this.objectManager = objectManager;
            this.cameraManager = cameraManager;
            this.graphics = graphics;

            //set the resolution using the property
            this.ScreenResolution = screenResolution;
        }

        public bool ToggleFullScreen()
        {
            //flip the screen mode
            this.graphics.IsFullScreen = !this.graphics.IsFullScreen;
            this.graphics.ApplyChanges();

            //return new state
            return this.graphics.IsFullScreen;
        }

        public override void Update(GameTime gameTime)
        {
            //if one camera needs to be drawn on top of another then we need to do a depth sort the first time the game is run
            if (this.bFirstTime && this.screenType == ScreenUtility.ScreenType.MultiPictureInPicture)
            {
                //sort so that the top-most camera (i.e. closest draw depth to 0 will be the last camera drawn)
                this.cameraManager.SortByDepth(SortDirectionType.Descending);
                this.bFirstTime = false;
            }

            //explicit call, as mentioned in Wiki - 2.4. Camera Viewports
            this.objectManager.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (this.screenType == ScreenUtility.ScreenType.SingleScreen)
            {
                this.objectManager.Draw(gameTime, this.cameraManager.ActiveCamera);
            }
            else
            {
                //foreach is enabled by making CameraManager implement IEnumerator
                foreach (Camera3D camera in cameraManager)
                {
                    this.objectManager.Draw(gameTime, camera);
                }
            }
            base.Draw(gameTime);
        }
    }
}
