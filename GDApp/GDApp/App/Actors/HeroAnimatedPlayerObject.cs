using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JigLibX.Collision;
using System.Collections.Generic;

namespace GDApp
{
    public class HeroAnimatedPlayerObject : AnimatedPlayerObject
    {
        private float moveSpeed, rotationSpeed;

        public HeroAnimatedPlayerObject(string id, ActorType actorType, Transform3D transform, 
            EffectParameters effectParameters, Keys[] moveKeys, float radius, float height, 
            float accelerationRate, float decelerationRate,
            float moveSpeed, float rotationSpeed,
            float jumpHeight, Vector3 translationOffset, 
            KeyboardManager keyboardManager, string startAnimationName, Dictionary<string, Model> modelDictionary) 
            : base(id, actorType, transform, effectParameters, moveKeys, radius, height, 
                  accelerationRate, decelerationRate, jumpHeight, translationOffset, keyboardManager, 
                  startAnimationName, modelDictionary)
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

        private void HandleCollisions(CollidableObject collidableObjectCollider, CollidableObject collidableObjectCollidee)
        {
            if (collidableObjectCollidee != null)
            {
                if (collidableObjectCollidee.ActorType == ActorType.CollidablePickup)
                {
                    //publish an event to remove, play a sound etc...
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
            }
            //crouch
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveCrouch]))
            {
                this.CharacterBody.IsCrouching = !this.CharacterBody.IsCrouching;
            }

            //forward/backward
            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveForward]))
            {
                this.CharacterBody.Velocity += this.Transform.Look * this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveBackward]))
            {
                this.CharacterBody.Velocity -= this.Transform.Look * this.moveSpeed * gameTime.ElapsedGameTime.Milliseconds;
            }
            else //decelerate to zero when not pressed
            {
                this.CharacterBody.DesiredVelocity = Vector3.Zero;
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
            else //decelerate to zero when not pressed
            {
                this.CharacterBody.DesiredVelocity = Vector3.Zero;
            }

            //update the camera position to reflect the collision skin position
            this.Transform.Translation = this.CharacterBody.Position;

        }
    }
}
