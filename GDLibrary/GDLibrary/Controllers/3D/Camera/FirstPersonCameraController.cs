/*
Function: 		First person camera controller allows movement in any XZ direction (no y-axis movement is allowed)
Author: 		NMCG
Version:		1.0
Date Updated:	30/8/17
Bugs:			None
Fixes:			None
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary
{

    public class FirstPersonCameraController : UserInputController
    {
        #region Fields
        //local vars
        private Vector3 translation;
        Vector2 mousePosition = Vector2.Zero;
        Vector2 mouseDelta = Vector2.Zero;


        #endregion

        #region Properties
        #endregion

        public FirstPersonCameraController(string id, ControllerType controllerType, Keys[] moveKeys, float moveSpeed, float strafeSpeed, float rotationSpeed, ManagerParameters managerParameters)
            : base(id, controllerType, moveKeys, moveSpeed, strafeSpeed, rotationSpeed, managerParameters)
        {

        }

        public override void HandleGamePadInput(GameTime gameTime, Actor3D parentActor)
        {
            //only override this method if we want to use the gamepad
            //if (this.gamePadManager.IsButtonPressed(PlayerIndex.One, Buttons.RightTrigger))
            //{
            //    //do something....
            //}
        }


        public override void HandleMouseInput(GameTime gameTime, Actor3D parentActor)
        {

            mousePosition = -this.ManagerParameters.MouseManager.GetDeltaFromCentre(this.ManagerParameters.CameraManager.ActiveCamera.ViewportCentre);
            mousePosition *= gameTime.ElapsedGameTime.Milliseconds * this.RotationSpeed;
            System.Diagnostics.Debug.Write("Delta" + mousePosition);


            if(mousePosition.X != 0 && mousePosition.Y != 0)
            {
                mouseDelta = mouseDelta + mousePosition;


                //only rotate if something has changed with the mouse

                parentActor.Transform.RotateBy(new Vector3(mouseDelta.X,mouseDelta.Y, 0));
                

            }
            



        } 


        public override void HandleKeyboardInput(GameTime gameTime, Actor3D parentActor)
        {
            translation = Vector3.Zero;

            if (this.ManagerParameters.KeyboardManager.IsKeyDown(this.MoveKeys[0]))
            {
                translation = gameTime.ElapsedGameTime.Milliseconds
                             * this.MoveSpeed * parentActor.Transform.Look;
            }
            else if (this.ManagerParameters.KeyboardManager.IsKeyDown(this.MoveKeys[1]))
            {
                translation = -gameTime.ElapsedGameTime.Milliseconds
                            * this.MoveSpeed * parentActor.Transform.Look;
            }

            if (this.ManagerParameters.KeyboardManager.IsKeyDown(this.MoveKeys[2]))
            {
                //What's the significance of the +=? Remove it and see if we can move forward/backward AND strafe.
                translation += -gameTime.ElapsedGameTime.Milliseconds
                             * this.StrafeSpeed * parentActor.Transform.Right;
            }
            else if (this.ManagerParameters.KeyboardManager.IsKeyDown(this.MoveKeys[3]))
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