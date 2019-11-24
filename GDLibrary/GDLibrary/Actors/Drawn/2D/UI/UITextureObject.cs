/*
Function: 		Represents texture drawn in a 2D menu or UI element. 
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
    public class UITextureObject : UIObject
    {
        //draws texture using full source rectangle with origin in centre
        public UITextureObject(string id, ActorType actorType, StatusType statusType, Transform2D transform,
            Color color, SpriteEffects spriteEffects, float layerDepth, Texture2D texture)
            : this(id, actorType, statusType, transform, color, spriteEffects, layerDepth, texture,
                new Rectangle(0, 0, texture.Width, texture.Height),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f))
        {
        }

        public UITextureObject(string id, ActorType actorType, StatusType statusType, Transform2D transform,
            Color color, SpriteEffects spriteEffects, float layerDepth, Texture2D texture,
            Rectangle sourceRectangle, Vector2 origin)
            : base(id, actorType, statusType, transform, color, spriteEffects, layerDepth)
        {
            Texture = texture;
            SourceRectangle = sourceRectangle;
            OriginalSourceRectangle = SourceRectangle;
            Origin = origin;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Transform.Translation,
                sourceRectangle, Color,
                MathHelper.ToRadians(Transform.RotationInDegrees),
                Transform.Origin, Transform.Scale, SpriteEffects, LayerDepth);
        }

        public override bool Equals(object obj)
        {
            var other = obj as UITextureObject;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return Texture.Equals(other.Texture)
                   && sourceRectangle.Equals(other.SourceRectangle)
                   && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + Texture.GetHashCode();
            hash = hash * 17 + sourceRectangle.GetHashCode();
            hash = hash * 7 + base.GetHashCode();
            return hash;
        }

        public new object Clone()
        {
            IActor actor = new UITextureObject("clone - " + ID, //deep
                ActorType, //deep
                StatusType, //deep - enum type
                (Transform2D) Transform.Clone(), //deep - calls the clone for Transform3D explicitly
                Color, //deep 
                SpriteEffects, //deep - enum type
                LayerDepth, //deep 
                Texture, //shallow
                sourceRectangle, //deep 
                Origin); //deep 

            //clone each of the (behavioural) controllers, if we have any controllers attached
            if (ControllerList != null)
                foreach (var controller in ControllerList)
                    actor.AttachController((IController) controller.Clone());

            return actor;
        }

        #region Fields

        private Rectangle sourceRectangle;

        #endregion

        #region Properties

        public Vector2 Origin { get; set; }

        public Rectangle OriginalSourceRectangle { get; }

        public Rectangle SourceRectangle
        {
            get => sourceRectangle;
            set => sourceRectangle = value;
        }

        public int SourceRectangleWidth
        {
            get => sourceRectangle.Width;
            set => sourceRectangle.Width = value;
        }

        public int SourceRectangleHeight
        {
            get => sourceRectangle.Height;
            set => sourceRectangle.Height = value;
        }

        public Texture2D Texture { get; set; }

        #endregion
    }
}