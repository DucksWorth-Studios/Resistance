using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class UIMouseObject : UITextureObject
    {

        #region Variables
        private string text;
        private SpriteFont spriteFont;
        private Vector2 textOffsetPosition;
        private Color textColor;
        private Vector2 textDimensions;
        private Vector2 textOrigin;
        private MouseManager mouseManager;
        private CameraManager cameraManager;
        private float pickStartDistance;
        private float pickEndDistance;


        //temp vars
        private Vector3 pos, normal;

        #endregion

        #region Properties
        public string Text
        {
            get
            {
                return this.text;
            }
            set
            {
                this.text = value;
                this.textDimensions = this.spriteFont.MeasureString(this.text);
                this.textOrigin = new Vector2(this.textDimensions.X / 2, this.textDimensions.Y / 2);
            }
        }
        public SpriteFont SpriteFont
        {
            get
            {
                return this.spriteFont;
            }
            set
            {
                this.spriteFont = value;
            }
        }
        public MouseManager MouseManager
        {
            get
            {
                return this.mouseManager;
            }
        }
        public CameraManager CameraManager
        {
            get
            {
                return this.cameraManager;
            }
        }
        #endregion

        public UIMouseObject(string id, ActorType actorType, StatusType statusType, Transform2D transform,
            ColorParameters colorParameters, SpriteEffects spriteEffects, SpriteFont spriteFont, 
            string text, Vector2 textOffsetPosition, Color textColor,
            float layerDepth, Texture2D texture, Rectangle sourceRectangle, Vector2 origin, 
            MouseManager mouseManager, CameraManager cameraManager,
            float pickStartDistance, float pickEndDistance)
            : base(id, actorType, statusType, transform, colorParameters, spriteEffects, layerDepth, texture, sourceRectangle, origin)
        {
            this.spriteFont = spriteFont;
            this.Text = text;
            this.textOffsetPosition = textOffsetPosition;
            this.textColor = textColor;
            this.mouseManager = mouseManager;
            this.cameraManager = cameraManager;
            this.pickStartDistance = pickStartDistance;
            this.pickEndDistance = pickEndDistance;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //draw mouse reticule
            spriteBatch.Draw(this.Texture, this.Transform.Translation,
                this.SourceRectangle, this.ColorParameters.Color,
                MathHelper.ToRadians(this.Transform.RotationInDegrees),
                this.Origin, this.Transform.Scale, this.SpriteEffects, this.LayerDepth);

            //draw any additional text
            if (this.text != null)
                spriteBatch.DrawString(this.spriteFont, this.text,(this.Transform.Translation + this.textOrigin), this.textColor);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateMouseObject(gameTime);
            DoMousePick(gameTime);
            base.Update(gameTime);
        }

        //move the texture for the mouse object to be where the mouse pointer is
        private void UpdateMouseObject(GameTime gameTime)
        {
            this.Transform.Translation = this.mouseManager.Position;
        }

        public virtual void DoMousePick(GameTime gameTime)
        {
            if (this.cameraManager.ActiveCamera != null)
            {
                CollidableObject collidableObject = this.mouseManager.GetPickedObject(this.cameraManager.ActiveCamera, this.cameraManager.ActiveCamera.ViewportCentre, 
                                                this.pickStartDistance, this.pickEndDistance, out pos, out normal) as CollidableObject;

                //did we collide with something?
                if (collidableObject != null)
                {
                    HandleCollision(collidableObject, pos, normal);
                }
                else
                    HandleNoCollision();
            }
        }

        //resets when no mouse over collidable
        protected virtual void HandleNoCollision()
        {
            
        }

        //called when over collidable/pickable object
        protected virtual void HandleCollision(CollidableObject collidableObject, Vector3 pos, Vector3 normal /*unused - could use for bullet decals*/)
        {
            
        }
    }
}
