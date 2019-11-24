using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary
{
    /// <summary>
    ///     Represents your MOVEABLE player in the game.
    /// </summary>
    public class PlayerObject : CharacterObject
    {
        public PlayerObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, Model model,
            Keys[] moveKeys, float radius, float height, float accelerationRate, float decelerationRate,
            float jumpHeight,
            Vector3 translationOffset, KeyboardManager keyboardManager)
            : base(id, actorType, transform, effectParameters, model, radius, height, accelerationRate,
                decelerationRate)
        {
            MoveKeys = moveKeys;
            TranslationOffset = translationOffset;
            KeyboardManager = keyboardManager;
            this.jumpHeight = jumpHeight;
        }

        public override Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(Transform.Scale) *
                   Collision.GetPrimitiveLocal(0).Transform.Orientation *
                   Body.Orientation *
                   Transform.Orientation *
                   Matrix.CreateTranslation(Body.Position + TranslationOffset);
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
        }

        #region Variables

        private float jumpHeight;

        #endregion

        #region Properties

        public KeyboardManager KeyboardManager { get; }

        public float JumpHeight
        {
            get => jumpHeight;
            set => jumpHeight = value > 0 ? value : 1;
        }

        public Vector3 TranslationOffset { get; set; }

        public Keys[] MoveKeys { get; set; }

        #endregion

        //add clone, equals, gethashcode, remove...
    }
}