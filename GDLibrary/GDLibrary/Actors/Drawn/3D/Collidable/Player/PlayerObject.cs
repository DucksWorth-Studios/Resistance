using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary
{
    /// <summary>
    /// Represents your MOVEABLE player in the game. 
    /// </summary>
    public class PlayerObject : CharacterObject
    {
        #region Variables
        private Keys[] moveKeys;
        private Vector3 translationOffset;
        private KeyboardManager keyboardManager;
        private float jumpHeight;
        #endregion

        #region Properties
        public KeyboardManager KeyboardManager
        {
            get
            {
                return keyboardManager;
            }
        }
        public float JumpHeight
        {
            get
            {
                return jumpHeight;
            }
            set
            {
                jumpHeight = (value > 0) ? value : 1;
            }
        }
        public Vector3 TranslationOffset
        {
            get
            {
                return translationOffset;
            }
            set
            {
                translationOffset = value;
            }
        }
        public Keys[] MoveKeys
        {
            get
            {
                return moveKeys;
            }
            set
            {
                moveKeys = value;
            }
        }
        #endregion

        public PlayerObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, Model model, 
            Keys[] moveKeys, float radius, float height, float accelerationRate, float decelerationRate, float jumpHeight, 
            Vector3 translationOffset, KeyboardManager keyboardManager)
            : base(id, actorType, transform, effectParameters, model, radius, height, accelerationRate, decelerationRate)
        {
            this.moveKeys = moveKeys;
            this.translationOffset = translationOffset;
            this.keyboardManager = keyboardManager;
            this.jumpHeight = jumpHeight;
        }

        public override Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(this.Transform.Scale) *
                this.Collision.GetPrimitiveLocal(0).Transform.Orientation *
                this.Body.Orientation *
                this.Transform.Orientation *
                Matrix.CreateTranslation(this.Body.Position + translationOffset);
        }


        public override void Update(GameTime gameTime)
        {
            HandleKeyboardInput(gameTime);
            HandleMouseInput(gameTime);
            base.Update(gameTime);
        }

        protected virtual void HandleMouseInput(GameTime gameTime)
        {
          //perhaps rotate using mouse pointer distance from centre?
        }

        protected virtual void HandleKeyboardInput(GameTime gameTime)
        {
            /*
                         //jump
            if (this.keyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveJump]))
            {
                this.CharacterBody.DoJump(this.JumpHeight);
            }
            //crouch
            else if (this.keyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveCrouch]))
            {
                this.CharacterBody.IsCrouching = !this.CharacterBody.IsCrouching;
            }

            //forward/backward
            if (this.keyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveForward]))
            {
                this.CharacterBody.Velocity += this.Transform.Look * 1 * gameTime.ElapsedGameTime.Milliseconds;
            }
            else if (this.keyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexMoveBackward]))
            {
                this.CharacterBody.Velocity -= this.Transform.Look * 1 * gameTime.ElapsedGameTime.Milliseconds;
            }
            else //decelerate to zero when not pressed
            {
                this.CharacterBody.DesiredVelocity = Vector3.Zero;
            }

            //strafe left/right
            if (this.keyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexRotateLeft]))
            {
                this.Transform.RotateAroundYBy(0.1f * gameTime.ElapsedGameTime.Milliseconds);
            }
            else if (this.keyboardManager.IsKeyDown(this.MoveKeys[AppData.IndexRotateRight]))
            {
                this.Transform.RotateAroundYBy(-0.1f * gameTime.ElapsedGameTime.Milliseconds);
            }
            else //decelerate to zero when not pressed
            {
                this.CharacterBody.DesiredVelocity = Vector3.Zero;
            }

            //update the camera position to reflect the collision skin position
            this.Transform.Translation = this.CharacterBody.Position;
             */
        }

        //add clone, equals, gethashcode, remove...
    }
}
