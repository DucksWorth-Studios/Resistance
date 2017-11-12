using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

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
        private readonly MouseManager mouseManager;
        private readonly CameraManager cameraManager;
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

            //put the reticule in the centre of the screen
            this.Transform.Translation = this.cameraManager.ActiveCamera.ViewportCentre;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //draw icon
            spriteBatch.Draw(this.Texture, this.Transform.Translation, 
                this.SourceRectangle, this.ColorParameters.Color, this.Transform.RotationInRadians, this.Origin, 
                    this.Transform.Scale, this.SpriteEffects, this.LayerDepth);

            //draw any additional text
            if (this.text != null)
                spriteBatch.DrawString(this.spriteFont, this.text,
                    ((this.Transform.Translation - this.textOrigin) - this.textOffsetPosition), this.textColor);
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
                                                this.pickStartDistance, 
                                                this.pickEndDistance, out pos, out normal) as CollidableObject;

                //did we pick something?
                if(collidableObject != null)
                    HandlePickedObject(collidableObject, pos, normal);
            }
        }

        private void HandlePickedObject(CollidableObject collidableObject, Vector3 pos, Vector3 normal /*unused - could use for bullet decals*/)
        {
            if ((collidableObject != null) && (collidableObject.ActorType == ActorType.CollidablePickup))
            {
                float distanceToObject = Vector3.Distance(this.cameraManager.ActiveCamera.Transform.Translation, pos);
                distanceToObject = (float)Math.Round(distanceToObject, 1);
                this.Text = collidableObject.ID + "- distance[" + distanceToObject + "]";

                if (this.mouseManager.IsLeftButtonClickedOnce())
                {
                    //what would we like to do here? remove the item since its ammo or some sort of pickup?
                    EventDispatcher.Publish(new EventData("bla", collidableObject, EventActionType.Remove, EventCategoryType.SystemRemove));               
                }
            }
            else
            {
                this.Text = "no object selected";
            }
        }
    }
}
