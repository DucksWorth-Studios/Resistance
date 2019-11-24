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
        public FirstPersonCameraController(string id, ControllerType controllerType, Keys[] moveKeys, float moveSpeed,
            float strafeSpeed, float rotationSpeed, ManagerParameters managerParameters,
            EventDispatcher eventDispatcher)
            : base(id, controllerType, moveKeys, moveSpeed, strafeSpeed, rotationSpeed, managerParameters)
        {
            RegisterForEventHandling(eventDispatcher);
        }


        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.lockChanged += Mouselockbool;
            eventDispatcher.RiddleChanged += MovementBlock;

            //eventDispatcher.PopUpChanged += changeState;
        }

        public void changeState(EventData eventData)
        {
            inPopUp = false;
        }

        public void Mouselockbool(EventData eventData)
        {
            //System.Diagnostics.Debug.Write("IS Being Called");

            if (!Paused)
                Paused = true;
            else
                Paused = false;
        }

        public void MovementBlock(EventData eventData)
        {
            Console.WriteLine("Movement");
            if (!inPopUp)
            {
                Console.WriteLine("Movement2");
                inPopUp = true;
            }
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
            if (ManagerParameters.KeyboardManager.IsKeyPushed(Keys.Escape))
                EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.mouseLock));


            if (!Paused && !inPopUp)
            {
                mousePosition =
                    -ManagerParameters.MouseManager.GetDeltaFromCentre(ManagerParameters.CameraManager.ActiveCamera
                        .ViewportCentre);
                mouseDelta = mouseDelta + mousePosition * gameTime.ElapsedGameTime.Milliseconds * RotationSpeed;

                if (OldmousePosition == mousePosition && OldmousePosition != Vector2.Zero)
                {
                    mouseDelta = mouseDeltaTemp;

                    parentActor.Transform.RotateBy(new Vector3(mouseDelta.X, mouseDelta.Y, 0));
                    ManagerParameters.MouseManager.SetPosition(new Vector2(
                        ManagerParameters.ScreenManager.ScreenResolution.X / 2,
                        ManagerParameters.ScreenManager.ScreenResolution.Y / 2));
                }

                parentActor.Transform.RotateBy(new Vector3(mouseDelta.X, mouseDelta.Y, 0));
                OldmousePosition = mousePosition;

                if (mousePosition != Vector2.Zero) mouseDeltaTemp = mouseDelta;
            }
        }


        public override void HandleKeyboardInput(GameTime gameTime, Actor3D parentActor)
        {
            translation = Vector3.Zero;


            if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[0]))
                translation = gameTime.ElapsedGameTime.Milliseconds
                              * MoveSpeed * parentActor.Transform.Look;
            else if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[1]))
                translation = -gameTime.ElapsedGameTime.Milliseconds
                              * MoveSpeed * parentActor.Transform.Look;

            if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[2]))
                //What's the significance of the +=? Remove it and see if we can move forward/backward AND strafe.
                translation += -gameTime.ElapsedGameTime.Milliseconds
                               * StrafeSpeed * parentActor.Transform.Right;
            else if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[3]))
                //What's the significance of the +=? Remove it and see if we can move forward/backward AND strafe.
                translation += gameTime.ElapsedGameTime.Milliseconds
                               * StrafeSpeed * parentActor.Transform.Right;

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

        //local vars
        private Vector3 translation;
        private Vector2 mousePosition = Vector2.Zero;
        private Vector2 OldmousePosition = Vector2.Zero;
        private Vector2 mouseDelta = Vector2.Zero;
        private Vector2 mouseDeltaTemp = Vector2.One;

        private bool Paused = true;
        protected bool inPopUp;

        #endregion

        #region Properties

        #endregion

        //Add Equals, Clone, ToString, GetHashCode...
    }
}