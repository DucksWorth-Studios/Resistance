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
        Vector2 OldmousePosition = Vector2.Zero;
        Vector2 mouseDelta = Vector2.Zero;
        Vector2 mouseDeltaTemp = Vector2.One;

        bool Paused = true;
        protected bool inPopUp = false;



        #endregion

        #region Properties
        #endregion

        public FirstPersonCameraController(string id, ControllerType controllerType, Keys[] moveKeys, float moveSpeed, float strafeSpeed, float rotationSpeed, ManagerParameters managerParameters,EventDispatcher eventDispatcher)
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

        public void  Mouselockbool(EventData eventData)
        {

            System.Diagnostics.Debug.Write("IS Being Called");

            if (!Paused) { Paused = true; }
            else { Paused = false; }
  
           
        }

        public void MovementBlock(EventData eventData)
        {
            Console.WriteLine("Movement");
            if(!inPopUp)
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
            { 
                    EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.mouseLock));
                
            }


            if (!Paused)
            {

                mousePosition = -this.ManagerParameters.MouseManager.GetDeltaFromCentre(this.ManagerParameters.CameraManager.ActiveCamera.ViewportCentre);
                mouseDelta = mouseDelta + mousePosition * gameTime.ElapsedGameTime.Milliseconds * this.RotationSpeed;

                if (OldmousePosition == mousePosition && OldmousePosition != Vector2.Zero)
                {
                    mouseDelta = mouseDeltaTemp;

                    parentActor.Transform.RotateBy(new Vector3(mouseDelta.X, mouseDelta.Y, 0));
                    this.ManagerParameters.MouseManager.SetPosition(new Vector2(this.ManagerParameters.ScreenManager.ScreenResolution.X / 2, this.ManagerParameters.ScreenManager.ScreenResolution.Y / 2));
                }

                parentActor.Transform.RotateBy(new Vector3(mouseDelta.X, mouseDelta.Y, 0));
                OldmousePosition = mousePosition;

                if (mousePosition != Vector2.Zero) { mouseDeltaTemp = mouseDelta; }

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