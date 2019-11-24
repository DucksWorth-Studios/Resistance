/*
Function: 		Represents a combination of texture and text for a mouse cursor which supports object picking, placement, firing new objects (i.e.CDCR related activity).
Author: 		NMCG
Version:		1.0
Date Updated:	25/11/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class UIMouseObject : UITextureObject
    {
        public UIMouseObject(string id, ActorType actorType, StatusType statusType, Transform2D transform,
            Color color, SpriteEffects spriteEffects, SpriteFont spriteFont, string text, Vector2 textOffsetPosition,
            Color textColor,
            float layerDepth, Texture2D texture, Rectangle sourceRectangle, Vector2 origin, MouseManager mouseManager)
            : base(id, actorType, statusType, transform, color, spriteEffects, layerDepth, texture, sourceRectangle,
                origin)
        {
            SpriteFont = spriteFont;
            Text = text;
            this.textOffsetPosition = textOffsetPosition;
            TextColor = textColor;

            //used to update pointer position
            this.mouseManager = mouseManager;
        }


        public override void Update(GameTime gameTime)
        {
            Transform.Translation = mouseManager.Position;
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //draw mouse reticule
            spriteBatch.Draw(Texture, Transform.Translation,
                SourceRectangle, Color,
                MathHelper.ToRadians(Transform.RotationInDegrees),
                Origin, //bug fix for off centre rotation - uses explicitly specified origin and not this.Transform.Origin
                Transform.Scale, SpriteEffects, LayerDepth);

            //draw any additional text
            if (text != null)
                spriteBatch.DrawString(SpriteFont, text,
                    Transform.Translation + textOffsetPosition, TextColor, 0, textOrigin, 1, SpriteEffects.None,
                    LayerDepth);
        }

        #region Fields

        private string text;
        private readonly Vector2 textOffsetPosition;
        private Vector2 textDimensions;
        private Vector2 textOrigin;
        private readonly MouseManager mouseManager;

        #endregion

        #region Properties

        public string Text
        {
            get => text;
            set
            {
                text = value;
                textDimensions = SpriteFont.MeasureString(text);
                textOrigin = new Vector2(textDimensions.X / 2, textDimensions.Y / 2);
            }
        }

        public Color TextColor { get; set; }

        public SpriteFont SpriteFont { get; set; }

        #endregion
    }
}