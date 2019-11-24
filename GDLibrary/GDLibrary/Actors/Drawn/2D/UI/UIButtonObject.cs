using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class UIButtonObject : UITextureObject
    {
        public UIButtonObject(string id, ActorType actorType, StatusType statusType, Transform2D transform,
            Color color, SpriteEffects spriteEffects, float layerDepth, Texture2D texture,
            string text, SpriteFont spriteFont, Color textColor, Vector2 textOffset)
            : base(id, actorType, statusType, transform, color, spriteEffects, layerDepth, texture)
        {
            SpriteFont = spriteFont;
            //set using the property to also set the text origin
            Text = text;
            this.textColor = textColor;
            this.textOffset = textOffset;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //draw the texture first
            base.Draw(gameTime, spriteBatch);

            //draw the overlay text
            spriteBatch.DrawString(SpriteFont,
                text,
                Transform.Translation + textOffset,
                textColor,
                0,
                textOrigin,
                Transform.Scale,
                SpriteEffects.None,
                0.9f * LayerDepth); //reduce the layer depth slightly so text is always in front of the texture (remember that 0 = front, 1 = back)
        }

        public override bool Equals(object obj)
        {
            var other = obj as UIButtonObject;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return text.Equals(other.Text)
                   && SpriteFont.Equals(other.SpriteFont)
                   && textColor.Equals(other.TextColor)
                   && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + text.GetHashCode();
            hash = hash * 17 + SpriteFont.GetHashCode();
            hash = hash * 11 + textColor.GetHashCode();
            hash = hash * 7 + base.GetHashCode();
            return hash;
        }

        public new object Clone()
        {
            IActor actor = new UIButtonObject("clone - " + ID, //deep
                ActorType, //deep
                StatusType, //deep - enum type
                (Transform2D) Transform.Clone(), //deep - calls the clone for Transform3D explicitly
                Color, //deep 
                SpriteEffects, //deep - enum type
                LayerDepth, //deep
                Texture, //shallow
                text, //deep
                SpriteFont, //shallow
                textColor, //deep
                textOffset); //deep

            //clone each of the (behavioural) controllers, if we have any controllers attached
            if (ControllerList != null)
                foreach (var controller in ControllerList)
                    actor.AttachController((IController) controller.Clone());
            return actor;
        }

        public override bool Remove()
        {
            text = null;
            return base.Remove();
        }

        #region Fields

        private string text;
        private Color textColor;
        private Vector2 textOrigin;
        private readonly Vector2 textOffset;

        #endregion

        #region Properties

        public string Text
        {
            get => text;
            set
            {
                text = value.Length >= 0 ? value : "Default";
                textOrigin = SpriteFont.MeasureString(text) / 2.0f;
            }
        }

        public SpriteFont SpriteFont { get; set; }

        public Color TextColor
        {
            get => textColor;
            set => textColor = value;
        }

        #endregion
    }
}