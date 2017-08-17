/*
Function: 		Stores and organises the cameras available within the game (used single and split screen layouts) 
                WORK IN PROGRESS - at present this class is only a barebones class to be used by the ObjectManager 
Author: 		NMCG
Version:		1.0
Date Updated:	
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class CameraManager : GameComponent
    {

        #region Variables
        private Camera3D activeCamera = null;
        #endregion

        #region Properties
        #endregion

        public CameraManager(Game game) : base(game)
        {

        }

        public Camera3D ActiveCamera
        {
            get
            {
                return this.activeCamera;
            }
            set
            {
                this.ActiveCamera = value;
            }

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }
    }
}
