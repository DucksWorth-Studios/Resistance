/*
Function: 		Parent class for all controllers which accept keyboard input and apply to an actor (e.g. a FirstPersonCameraController inherits from this class).
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary
{
    public class UserInputController : Controller
    {
        public UserInputController(string id,
            ControllerType controllerType, Keys[] moveKeys,
            float moveSpeed, float strafeSpeed, float rotationSpeed, ManagerParameters managerParameters)
            : base(id, controllerType)
        {
            MoveKeys = moveKeys;
            MoveSpeed = moveSpeed;
            StrafeSpeed = strafeSpeed;
            RotationSpeed = rotationSpeed;

            ManagerParameters = managerParameters;
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            var parentActor = actor as Actor3D;
            HandleMouseInput(gameTime, parentActor);
            HandleKeyboardInput(gameTime, parentActor);
            HandleGamePadInput(gameTime, parentActor);
            base.Update(gameTime, actor);
        }

        public virtual void HandleGamePadInput(GameTime gameTime, Actor3D parentActor)
        {
        }

        public virtual void HandleMouseInput(GameTime gameTime, Actor3D parentActor)
        {
        }

        public virtual void HandleKeyboardInput(GameTime gameTime, Actor3D parentActor)
        {
        }

        #region Fields

        #endregion

        #region Properties

        public ManagerParameters ManagerParameters { get; }

        public Keys[] MoveKeys { get; set; }

        public float MoveSpeed { get; set; }

        public float StrafeSpeed { get; set; }

        public float RotationSpeed { get; set; }

        #endregion

        //Add Equals, Clone, ToString, GetHashCode...
    }
}