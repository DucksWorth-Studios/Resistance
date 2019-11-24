/*
Function: 		First person COLLIDABLE camera controller.
Author: 		NMCG
Version:		1.0
Date Updated:	6/10/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary
{
    /// <summary>
    ///     A collidable camera has a body and collision skin from a player object but it has no modeldata or texture
    /// </summary>
    public class CollidableFirstPersonCameraController : FirstPersonCameraController
    {
        //uses the default PlayerObject as the collidable object for the camera
        public CollidableFirstPersonCameraController(string id, ControllerType controllerType, Keys[] moveKeys,
            float moveSpeed, float strafeSpeed, float rotationSpeed,
            ManagerParameters managerParameters, EventDispatcher eventDispatcher,
            IActor parentActor, float radius, float height, float accelerationRate, float decelerationRate,
            float mass, float jumpHeight, Vector3 translationOffset)
            : this(id, controllerType, moveKeys, moveSpeed, strafeSpeed, rotationSpeed,
                managerParameters, eventDispatcher,
                parentActor, radius, height, accelerationRate, decelerationRate,
                mass, jumpHeight, translationOffset, null)
        {
        }

        //allows developer to specify the type of collidable object to be used as basis for the camera
        public CollidableFirstPersonCameraController(string id, ControllerType controllerType, Keys[] moveKeys,
            float moveSpeed, float strafeSpeed, float rotationSpeed,
            ManagerParameters managerParameters, EventDispatcher eventDispatcher,
            IActor parentActor, float radius, float height, float accelerationRate, float decelerationRate,
            float mass, float jumpHeight, Vector3 translationOffset, PlayerObject collidableObject)
            : base(id, controllerType, moveKeys, moveSpeed, strafeSpeed, rotationSpeed, managerParameters,
                eventDispatcher)
        {
            Radius = radius;
            this.height = height;
            AccelerationRate = accelerationRate;
            DecelerationRate = decelerationRate;
            Mass = mass;
            JumpHeight = jumpHeight;

            //allows us to tweak the camera position within the player object 
            this.translationOffset = translationOffset;

            /* Create the collidable player object which comes with a collision skin and position the parentActor (i.e. the camera) inside the player object.
             * notice that we don't pass any effect, model or texture information, since in 1st person perspective we dont want to look from inside a model.
             * Therefore, we wont actually render any drawn object - the effect, texture, model (and also Color) information are unused.
             * 
             * This code allows the user to pass in their own PlayerObject (e.g. HeroPlayerObject) to be used for the collidable object basis for the camera.
             */
            if (collidableObject != null)
                playerObject = collidableObject;
            else
                playerObject = new PlayerObject(ID + " - player object", ActorType.CollidableCamera,
                    (parentActor as Actor3D).Transform,
                    null, null, MoveKeys, radius, height, accelerationRate, decelerationRate, jumpHeight,
                    translationOffset, ManagerParameters.KeyboardManager);

            playerObject.Enable(false, mass);
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            //updates the sound manager to tell it where the 1st person camera is right now for any 3D sounds
            //did NOT use an event here as the frequency of calls to this event would FLOOD the system
            ManagerParameters.SoundManager.UpdateListenerPosition((actor as Actor3D).Transform.Translation);
            HandleMouseInput(gameTime, actor as Actor3D);
            base.Update(gameTime, actor);
        }

        public override void HandleMouseInput(GameTime gameTime, Actor3D parentActor)
        {
            if (!inPopUp) base.HandleMouseInput(gameTime, parentActor);
        }


        public override void HandleKeyboardInput(GameTime gameTime, Actor3D parentActor)
        {
            /* Notice in the code below that we are NO LONGER simply changing the camera translation value. 
             * Since this is now a collidable camera we, instead, modify the camera position by calling the PlayerObject move methods.
             * 
             * Q. Why do we still use the rotation methods of Transform3D? 
             * A. Rotating the camera doesnt affect CD/CR since the camera is modelled by a player object which has a capsule shape.
             *    A capsule's collision response won't alter as a result of any rotation since its cross-section is spherical across the XZ-plane.
             */

            if (parentActor != null && parentActor != null)
            {
                if (!inPopUp)
                {
                    //crouch
                    if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[5]))
                        playerObject.CharacterBody.IsCrouching = !playerObject.CharacterBody.IsCrouching;

                    //forward/backward
                    if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[0]))
                    {
                        var restrictedLook = parentActor.Transform.Look;
                        restrictedLook.Y = 0;
                        playerObject.CharacterBody.Velocity +=
                            restrictedLook * MoveSpeed * gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[1]))
                    {
                        var restrictedLook = parentActor.Transform.Look;
                        restrictedLook.Y = 0;
                        playerObject.CharacterBody.Velocity -=
                            restrictedLook * MoveSpeed * gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else //decelerate to zero when not pressed
                    {
                        playerObject.CharacterBody.DesiredVelocity = Vector3.Zero;
                    }

                    //strafe left/right
                    if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[2]))
                    {
                        var restrictedRight = parentActor.Transform.Right;
                        restrictedRight.Y = 0;
                        playerObject.CharacterBody.Velocity -=
                            restrictedRight * StrafeSpeed * gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else if (ManagerParameters.KeyboardManager.IsKeyDown(MoveKeys[3]))
                    {
                        var restrictedRight = parentActor.Transform.Right;
                        restrictedRight.Y = 0;
                        playerObject.CharacterBody.Velocity +=
                            restrictedRight * StrafeSpeed * gameTime.ElapsedGameTime.Milliseconds;
                    }
                    else //decelerate to zero when not pressed
                    {
                        playerObject.CharacterBody.DesiredVelocity = Vector3.Zero;
                    }

                    //update the camera position to reflect the collision skin position
                    parentActor.Transform.Translation = playerObject.CharacterBody.Position;
                }
                else
                {
                    if (ManagerParameters.KeyboardManager.IsKeyPushed(Keys.Tab))
                    {
                        inPopUp = false;
                        ManagerParameters.MouseManager.SetPosition(new Vector2(
                            ManagerParameters.ScreenManager.ScreenResolution.X / 2,
                            ManagerParameters.ScreenManager.ScreenResolution.Y / 2));
                        EventDispatcher.Publish(new EventData(EventActionType.OnOpen, EventCategoryType.PopUpDown));
                    }
                }
            }
        }

        #region Fields

        private readonly PlayerObject playerObject;
        private float radius, height;
        private float accelerationRate, decelerationRate, mass, jumpHeight;
        private Vector3 translationOffset;

        #endregion

        #region Properties

        public float Radius
        {
            get => radius;
            set => radius = value > 0 ? value : 1;
        }

        public float Height
        {
            get => height;
            set => height = value > 0 ? value : 1;
        }

        public float AccelerationRate
        {
            get => accelerationRate;
            set => accelerationRate = value != 0 ? value : 1;
        }

        public float DecelerationRate
        {
            get => decelerationRate;
            set => decelerationRate = value != 0 ? value : 1;
        }

        public float Mass
        {
            get => mass;
            set => mass = value > 0 ? value : 1;
        }

        public float JumpHeight
        {
            get => jumpHeight;
            set => jumpHeight = value > 0 ? value : 1;
        }

        #endregion


        //to do - clone, dispose
    }
}