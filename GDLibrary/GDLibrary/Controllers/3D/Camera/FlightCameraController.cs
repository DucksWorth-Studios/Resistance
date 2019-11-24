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
        public FlightCameraController(string id, ControllerType controllerType, Keys[] moveKeys, float moveSpeed,
            float strafeSpeed, float rotationSpeed,
            ManagerParameters managerParameters)
            : base(id, controllerType, moveKeys, moveSpeed, strafeSpeed, rotationSpeed, managerParameters)
        {
        }

        public override void HandleMouseInput(GameTime gameTime, Actor3D parentActor)
        {
            var mouseDelta = Vector2.Zero;
            mouseDelta =
                -ManagerParameters.MouseManager.GetDeltaFromCentre(ManagerParameters.CameraManager.ActiveCamera
                    .ViewportCentre);
            mouseDelta *= gameTime.ElapsedGameTime.Milliseconds;
            mouseDelta *= RotationSpeed;

            //only rotate if something has changed with the mouse
            if (mouseDelta.Length() != 0)
                parentActor.Transform.RotateBy(new Vector3(mouseDelta.X, mouseDelta.Y, 0));
        }

        public override void HandleKeyboardInput(GameTime gameTime, Actor3D parentActor)
        {
            if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[0]))
                parentActor.Transform.TranslateBy(gameTime.ElapsedGameTime.Milliseconds
                                                  * MoveSpeed * parentActor.Transform.Look);
            else if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[1]))
                parentActor.Transform.TranslateBy(-gameTime.ElapsedGameTime.Milliseconds
                                                  * MoveSpeed * parentActor.Transform.Look);

            if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[2]))
                parentActor.Transform.TranslateBy(-gameTime.ElapsedGameTime.Milliseconds
                                                  * StrafeSpeed * parentActor.Transform.Right);
            else if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[3]))
                parentActor.Transform.TranslateBy(gameTime.ElapsedGameTime.Milliseconds
                                                  * StrafeSpeed * parentActor.Transform.Right);
        }

        #region Fields

        #endregion

        #region Properties

        #endregion

        //Add Equals, Clone, ToString, GetHashCode...
    }
}