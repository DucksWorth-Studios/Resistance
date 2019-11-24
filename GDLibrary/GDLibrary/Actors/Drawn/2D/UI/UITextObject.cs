/*
Function: 		Represents text drawn in a 2D menu or UI element. 
Author: 		NMCG
Version:		1.0
Date Updated:	27/9/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class UITextObject : UIObject
    {
        public UITextObject(string id, ActorType actorType, StatusType statusType, Transform2D transform,
            Color color, SpriteEffects spriteEffects, float layerDepth, string text, SpriteFont spriteFont)
            : base(id, actorType, statusType, transform, color, spriteEffects, layerDepth)
        {
            SpriteFont = spriteFont;
            this.text = text;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(SpriteFont, text, Transform.Translation, Color,
                MathHelper.ToRadians(Transform.RotationInDegrees),
                Transform.Origin, Transform.Scale, SpriteEffects, LayerDepth);
        }

        public override bool Equals(object obj)
        {
            var other = obj as UITextObject;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return text.Equals(other.Text)
                   && SpriteFont.Equals(other.SpriteFont)
                   && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + text.GetHashCode();
            hash = hash * 17 + SpriteFont.GetHashCode();
            hash = hash * 7 + base.GetHashCode();
            return hash;
        }

        public new object Clone()
        {
            IActor actor = new UITextObject("clone - " + ID, //deep
                ActorType, //deep
                StatusType, //deep - enum type
                (Transform2D) Transform.Clone(), //deep - calls the clone for Transform3D explicitly
                Color, //deep 
                SpriteEffects, //deep - enum type
                LayerDepth, //deep
                text, //deep
                SpriteFont); //shallow

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

        #endregion

        #region Properties

        public string Text
        {
            get => text;
            set => text = value.Length >= 0 ? value : "Default";
        }

        public SpriteFont SpriteFont { get; set; }

        #endregion
    }
}