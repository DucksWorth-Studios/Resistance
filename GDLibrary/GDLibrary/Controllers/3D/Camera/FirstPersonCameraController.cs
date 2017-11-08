/*
Function: 		First person camera controller allows movement in any XZ direction (no y-axis movement is allowed)
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
    
    public class FirstPersonCameraController : UserInputController
    {

        #region Fields
        private CameraManager cameraManager;

        //temp vars
        private Vector2 mouseDelta;
        private Vector3 translation;
        #endregion

        #region Properties
        #endregion

        public FirstPersonCameraController(string id, ControllerType controllerType, Keys[] moveKeys, float moveSpeed, float strafeSpeed, float rotationSpeed, 
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
            CompassDirectionType compassDirectionType = this.MouseManager.GetCompassDirection(10, this.cameraManager.ActiveCamera.Viewport.Bounds);
            int count = 0;
            if (compassDirectionType == CompassDirectionType.Centre)
            {
                count = 0;
                mouseDelta = Vector2.Zero;
                mouseDelta = -this.MouseManager.GetDeltaFromCentre(this.cameraManager.ActiveCamera.ViewportCentre);
                mouseDelta *= gameTime.ElapsedGameTime.Milliseconds;
                mouseDelta *= this.RotationSpeed * 0.03f;
                if (mouseDelta.Length() != 0)
                    parentActor.Transform.RotateBy(new Vector3(mouseDelta.X, mouseDelta.Y, 0));
            }
            //else if (compassDirectionType == CompassDirectionType.East)
            //{
            //    count++;

            //   // if (this.KeyboardManager.IsKeyDown(Keys.G))
            //  //  {
            //        parentActor.Transform.RotateBy(new Vector3(-count, 0, 0));
            //  //  }
            //}
            //else if (compassDirectionType == CompassDirectionType.West)
            //{
            //    count++;

            //    // if (this.KeyboardManager.IsKeyDown(Keys.G))
            //    //  {
            //    parentActor.Transform.RotateBy(new Vector3(count, 0, 0));
            //    //  }
            //}


        }

        public override void HandleKeyboardInput(GameTime gameTime, Actor3D parentActor)
        {
            translation = Vector3.Zero;

            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[0]))
            {
                translation = gameTime.ElapsedGameTime.Milliseconds
                             * this.MoveSpeed * parentActor.Transform.Look;
            }
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[1]))
            {
                translation = -gameTime.ElapsedGameTime.Milliseconds
                            * this.MoveSpeed * parentActor.Transform.Look;
            }

            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[2]))
            {
                //What's the significance of the +=? Remove it and see if we can move forward/backward AND strafe.
                translation += -gameTime.ElapsedGameTime.Milliseconds
                             * this.StrafeSpeed * parentActor.Transform.Right;
            }
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[3]))
            {
                //What's the significance of the +=? Remove it and see if we can move forward/backward AND strafe.
                translation += gameTime.ElapsedGameTime.Milliseconds
                            * this.StrafeSpeed * parentActor.Transform.Right;
            }

            //Was a move button(s) pressed?
            if (translation != Vector3.Zero)
            {
                //remove y-axis component of the translation
                translation.Y = 0;
                //apply
                parentActor.Transform.TranslateBy(translation);
            }
        }

        //Add Equals, Clone, ToString, GetHashCode...
    }
}