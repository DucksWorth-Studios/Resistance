/*
Function: 		Creates a simple controller for drivable objects taking input from the keyboard
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
    public class DriveController : UserInputController
    {
        public DriveController(string id, ControllerType controllerType, Keys[] moveKeys, float moveSpeed,
            float strafeSpeed, float rotationSpeed,
            ManagerParameters managerParameters)
            : base(id, controllerType, moveKeys, moveSpeed, strafeSpeed, rotationSpeed, managerParameters)
        {
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            base.Update(gameTime, actor);
        }

        public override void HandleKeyboardInput(GameTime gameTime, Actor3D parentActor)
        {
            var translation = Vector3.Zero;

            //move forward/backward
            if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[0]))
                translation = gameTime.ElapsedGameTime.Milliseconds
                              * MoveSpeed * parentActor.Transform.Look;
            else if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[1]))
                translation = -gameTime.ElapsedGameTime.Milliseconds
                              * MoveSpeed * parentActor.Transform.Look;

            //strafe
            if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[4]))
                //What's the significance of the +=? Remove it and see if we can move forward/backward AND strafe.
                translation += -gameTime.ElapsedGameTime.Milliseconds
                               * StrafeSpeed * parentActor.Transform.Right;
            else if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[5]))
                //What's the significance of the +=? Remove it and see if we can move forward/backward AND strafe.
                translation += gameTime.ElapsedGameTime.Milliseconds
                               * StrafeSpeed * parentActor.Transform.Right;

            //rotate
            if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[2]))
                parentActor.Transform.RotateAroundYBy(gameTime.ElapsedGameTime.Milliseconds * RotationSpeed);
            else if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[3]))
                parentActor.Transform.RotateAroundYBy(-gameTime.ElapsedGameTime.Milliseconds * RotationSpeed);

            //Was a move button(s) pressed?
            if (translation != Vector3.Zero)
            {
                //remove y-axis component of the translation
                translation.Y = 0;
                //apply
                parentActor.Transform.TranslateBy(translation);
            }
        }

        #region Fields

        #endregion

        #region Properties

        #endregion

        //Add Equals, Clone, ToString, GetHashCode...
    }
}