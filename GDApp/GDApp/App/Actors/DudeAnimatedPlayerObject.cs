using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using JigLibX.Collision;

namespace GDApp
{
    public class DudeAnimatedPlayerObject : AnimatedPlayerObject
    {
        private float moveSpeed, rotationSpeed;
        private readonly float DefaultMinimumMoveVelocity = 1;

        public DudeAnimatedPlayerObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, Keys[] moveKeys, float radius, float height,
            float accelerationRate, float decelerationRate,
            float moveSpeed, float rotationSpeed,
            float jumpHeight, Vector3 translationOffset,
            KeyboardManager keyboardManager)
            : base(id, actorType, transform, effectParameters, moveKeys, radius, height,
                  accelerationRate, decelerationRate, jumpHeight, translationOffset, keyboardManager)
        {
            //add extra constructor parameters like health, inventory etc...
            this.moveSpeed = moveSpeed;
            this.rotationSpeed = rotationSpeed;

            //register for callback on CDCR
            this.CharacterBody.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
        }


        //this methods defines how your player interacts with ALL collidable objects in the world - its really the players complete behaviour
        private bool CollisionSkin_callbackFn(CollisionSkin collider, CollisionSkin collidee)
        {
            HandleCollisions(collider.Owner.ExternalData as CollidableObject, collidee.Owner.ExternalData as CollidableObject);
            return true;
        }

        //want do we want to do now that we have collided with an object?
        private void HandleCollisions(CollidableObject collidableObjectCollider, CollidableObject collidableObjectCollidee)
        {
            //did the "as" typecast return a valid object?
            if (collidableObjectCollidee != null)
            {
                if (collidableObjectCollidee.ActorType == ActorType.CollidablePickup)
                {
                    #region DEMO COLLISION RESPONSE FOR PICKUPS
                    EventDispatcher.Publish(new EventData(collidableObjectCollidee, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));

                    //after fixing the event dispatcher update() method we can not successfully increment UI and/or send other events (e.g. play sound)
                    object[] additionalEventParams = { AppData.PlayerOneProgressControllerID, 1 };
                    EventDispatcher.Publish(new EventData(EventActionType.OnHealthDelta, EventCategoryType.Player, additionalEventParams));

                    object[] additionalParameters = { "boing" };
                    EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.SoundStart, additionalParameters));

                    //do whatever you want here when you hit a collidable pickup...
                    #endregion


                }
                //add else if statements here for all the responses that you want your player to have
                else if (collidableObjectCollidee.ActorType == ActorType.CollidableDoor)
                {

                }
                else if (collidableObjectCollidee.ActorType == ActorType.CollidableAmmo)
                {

                }
            }
        }

        protected override void HandleKeyboardInput(GameTime gameTime)
        {

            //jump
            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveJump]))
            {
                this.CharacterBody.DoJump(this.JumpHeight);
                this.AnimationState = AnimationStateType.Jumping;
            }
            //crouch
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveCrouch]))
            {
                //we don't do anything with crounching really///could consider making capsule height of the collision skin smaller and playing a cround animation.
                this.CharacterBody.IsCrouching = !this.CharacterBody.IsCrouching;
                this.AnimationState = AnimationStateType.Crouching;
            }

            //forward/backward
            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveForward]))
            {
                this.CharacterBody.Velocity += this.Transform.Look * this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
                this.AnimationState = AnimationStateType.Walking;
            }
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveBackward]))
            {
                this.CharacterBody.Velocity -= this.Transform.Look * this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
                this.AnimationState = AnimationStateType.Walking;
            }

            //strafe left/right
            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexRotateLeft]))
            {
                this.Transform.RotateAroundYBy(this.rotationSpeed * gameTime.ElapsedGameTime.Milliseconds);
            }
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexRotateRight]))
            {
                this.Transform.RotateAroundYBy(-this.rotationSpeed * gameTime.ElapsedGameTime.Milliseconds);
            }

            //slow to a stop on no key press
            if (!this.KeyboardManager.IsAnyKeyPressed())
                this.CharacterBody.DesiredVelocity = Vector3.Zero;


            //if no velocity then set idle animation
            if (this.CharacterBody.Velocity.Length() < DefaultMinimumMoveVelocity)
                this.AnimationState = AnimationStateType.Idle;

            //update the camera position to reflect the collision skin position
            this.Transform.Translation = this.CharacterBody.Position;

            SetAnimationByInput();

        }

        protected override void SetAnimationByInput()
        {
            switch (this.AnimationState)
            {
                case AnimationStateType.Running:
                    //call SetAnimation() with your Running FBX file name and Take 001
                    break;

                case AnimationStateType.Walking:
                    SetAnimation("Take 001", "dude");
                    break;

                case AnimationStateType.Jumping:
                    //call SetAnimation() with your Jumping FBX file name and Take 001
                    break;

                case AnimationStateType.Idle:
                    //call SetAnimation() with your Idle FBX file name and Take 001
                    break;
            }

        }
    }
}
