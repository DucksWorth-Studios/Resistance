/*
Function: 		Represents a your player. We override the  CollisionSkin_callbackFn() to define how the HeroPlayerObject responds to collidable objects in the environment.
Author: 		NMCG
Version:		1.0
Date Updated:	13/11/17
Bugs:			None
Fixes:			None
*/

using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JigLibX.Collision;
using System;

namespace GDApp
{
    public class HeroPlayerObject : PlayerObject
    {
        private string progressControllerID;

        public HeroPlayerObject(string id, string progressControllerID, ActorType actorType, Transform3D transform, Effect effect, 
            ColorParameters colorParameters, Texture2D texture, Model model, Keys[] moveKeys, float radius, 
            float height, float accelerationRate, float decelerationRate, float jumpHeight, Vector3 translationOffset,
            KeyboardManager keyboardManager) 
            : base(id, actorType, transform, effect, colorParameters, texture, model, moveKeys, radius, 
                  height, accelerationRate, decelerationRate, jumpHeight, translationOffset, keyboardManager)
        {
            //id of the progress controller associated with this player object - see HandleCollisions()
            this.progressControllerID = progressControllerID;
            //register for callback on CDCR
            this.CharacterBody.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
        }

        #region Event Handling
        protected virtual bool CollisionSkin_callbackFn(CollisionSkin collider, CollisionSkin collidee)
        {
            HandleCollisions(collider.Owner.ExternalData as CollidableObject, collidee.Owner.ExternalData as CollidableObject);
            return true;
        }
        //how do we want this object to respond to collisions?
        private void HandleCollisions(CollidableObject collidableObjectCollider, CollidableObject collidableObjectCollidee)
        {
            if(collidableObjectCollidee.ActorType == ActorType.CollidablePickup)
            {
                //remove the object?
                EventDispatcher.Publish(new EventData("removing bla", collidableObjectCollidee, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));
                //play a sound?

                //decrement/increment score to the controller
                object[] additionalEventParams = { this.progressControllerID, (Integer)1 };
                EventDispatcher.Publish(new EventData("bla", this, EventActionType.OnHealthChange, EventCategoryType.Player, additionalEventParams));

            }
        }
        #endregion

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

                //to do - reduce the height of the collision primitive
            }

            //forward/backward
            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveForward]))
            {
                this.CharacterBody.Velocity += this.Transform.Look * 1 * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveBackward]))
            {
                this.CharacterBody.Velocity -= this.Transform.Look * 1 * gameTime.ElapsedGameTime.Milliseconds;
            }
            else //decelerate to zero when not pressed
            {
                this.CharacterBody.DesiredVelocity = Vector3.Zero;
            }

            //strafe left/right
            if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexRotateLeft]))
            {
                this.Transform.RotateAroundYBy(0.1f * gameTime.ElapsedGameTime.Milliseconds);
            }
            else if (this.KeyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexRotateRight]))
            {
                this.Transform.RotateAroundYBy(-0.1f * gameTime.ElapsedGameTime.Milliseconds);
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
