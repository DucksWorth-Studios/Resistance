/*
Function: 		Represents a player in the game. We override the CollisionSkin_callbackFn() to define how the HeroPlayerObject responds to collidable objects in the environment.
Author: 		NMCG
Version:		1.0
Date Updated:	13/11/17
Bugs:			None
Fixes:			None
*/

using GDLibrary;
using JigLibX.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GDApp
{
    public class HeroPlayerObject : PlayerObject
    {
        private string progressControllerID;

        public HeroPlayerObject(string id, string progressControllerID, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters,
            Model model, Keys[] moveKeys, float radius,
            float height, float accelerationRate, float decelerationRate, float jumpHeight, Vector3 translationOffset,
            KeyboardManager keyboardManager)
            : base(id, actorType, transform, effectParameters, model, moveKeys, radius,
                height, accelerationRate, decelerationRate, jumpHeight, translationOffset, keyboardManager)
        {
            //id of the progress controller associated with this player object - see HandleCollisions()
            this.progressControllerID = progressControllerID;

            //register for callback on CDCR
            CharacterBody.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
        }

        protected override void HandleKeyboardInput(GameTime gameTime)
        {
            //jump
            if (KeyboardManager.IsKeyDown(MoveKeys[AppData.IndexMoveJump]))
                CharacterBody.DoJump(JumpHeight);
            //crouch
            else if (KeyboardManager.IsKeyDown(MoveKeys[AppData.IndexMoveCrouch]))
                CharacterBody.IsCrouching = !CharacterBody.IsCrouching;

            //to do - reduce the height of the collision primitive

            //forward/backward
            if (KeyboardManager.IsKeyDown(MoveKeys[AppData.IndexMoveForward]))
                CharacterBody.Velocity += Transform.Look * 1 * gameTime.ElapsedGameTime.Milliseconds;
            else if (KeyboardManager.IsKeyDown(MoveKeys[AppData.IndexMoveBackward]))
                CharacterBody.Velocity -= Transform.Look * 1 * gameTime.ElapsedGameTime.Milliseconds;
            else //decelerate to zero when not pressed
                CharacterBody.DesiredVelocity = Vector3.Zero;

            //strafe left/right
            if (KeyboardManager.IsKeyDown(MoveKeys[AppData.IndexRotateLeft]))
                Transform.RotateAroundYBy(0.1f * gameTime.ElapsedGameTime.Milliseconds);
            else if (KeyboardManager.IsKeyDown(MoveKeys[AppData.IndexRotateRight]))
                Transform.RotateAroundYBy(-0.1f * gameTime.ElapsedGameTime.Milliseconds);
            else //decelerate to zero when not pressed
                CharacterBody.DesiredVelocity = Vector3.Zero;

            //update the camera position to reflect the collision skin position
            Transform.Translation = CharacterBody.Position;
        }

        #region Event Handling

        protected virtual bool CollisionSkin_callbackFn(CollisionSkin collider, CollisionSkin collidee)
        {
            HandleCollisions(collider.Owner.ExternalData as CollidableObject,
                collidee.Owner.ExternalData as CollidableObject);
            return true;
        }

        //how do we want this object to respond to collisions?
        private void HandleCollisions(CollidableObject collidableObjectCollider,
            CollidableObject collidableObjectCollidee)
        {
            if (collidableObjectCollidee.ActorType == ActorType.CollidablePickup)
                //remove the object?
                EventDispatcher.Publish(new EventData(collidableObjectCollidee, EventActionType.OnRemoveActor,
                    EventCategoryType.SystemRemove));
            //publish an event to play a sound, increment a score
        }

        #endregion
    }
}