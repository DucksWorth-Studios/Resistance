/*
Function: 		Flight controllers allows movement in any XYZ direction 
Author: 		NMCG
Version:		1.0
Date Updated:	30/8/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary
{
    
    public class FlightCameraController : UserInputController
    {
        private CameraManager cameraManager;
        #region Variables
        #endregion

        #region Properties
        #endregion

        public FlightCameraController(string id, ControllerType controllerType, Keys[] moveKeys, float moveSpeed, float strafeSpeed, float rotationSpeed, 
            MouseManager mouseManager, KeyboardManager keyboardManager, CameraManager cameraManager)
            : base(id, controllerType, moveKeys, moveSpeed, strafeSpeed, rotationSpeed, mouseManager, keyboardManager)
        {
            /*
             * Used to access the viewport for the active camera. Knowing the centre of the active viewport allows us to determine how far the mouse is from the centre.
             * We need to determine the mouse position in relation to the viewport centre in order to know how much rotation to apply in HandleMouseInput().
             */
            this.cameraManager = cameraManager;
        }

        public override void HandleMouseInput(GameTime gameTime, Actor3D parentActor)
        {
            Vector2 mouseDelta = Vector2.Zero;
            mouseDelta = -this.MouseManager.GetDeltaFromCentre(this.cameraManager.ActiveCamera.ViewportCentre);
            mouseDelta *= gameTime.ElapsedGameTime.Milliseconds;
            mouseDelta *= this.RotationSpeed;

            //only rotate if something has changed with the mouse
            if(mouseDelta.Length() != 0) 
                parentActor.Transform3D.RotateBy(new Vector3(mouseDelta.X, mouseDelta.Y, 0));
        }

        public override void HandleKeyboardInput(GameTime gameTime, Actor3D parentActor)
        {
            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[0]))
            {
                parentActor.Transform3D.TranslateBy(gameTime.ElapsedGameTime.Milliseconds
                             * this.MoveSpeed * parentActor.Transform3D.Look);
            }
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[1]))
            {
                parentActor.Transform3D.TranslateBy(-gameTime.ElapsedGameTime.Milliseconds
                             * this.MoveSpeed * parentActor.Transform3D.Look);
            }

            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[2]))
            {
                parentActor.Transform3D.TranslateBy(-gameTime.ElapsedGameTime.Milliseconds
                             * this.StrafeSpeed * parentActor.Transform3D.Right);
            }
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[3]))
            {
                parentActor.Transform3D.TranslateBy(gameTime.ElapsedGameTime.Milliseconds
                    * this.StrafeSpeed * parentActor.Transform3D.Right);
            }
        }

        //add clone...
    }
}