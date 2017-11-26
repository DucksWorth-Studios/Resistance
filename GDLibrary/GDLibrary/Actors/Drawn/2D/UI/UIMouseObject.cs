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

        //allows us to access lots of input types etc
        private ManagerParameters managerParameters;
        //this allows us to say something like "im interested in ActorType.Pickup" or "ActorType.Pickup and ActorType.Decorator"
        private Predicate<CollidableObject> collisionPredicate;

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
        public ManagerParameters ManagerParameters
        {
            get
            {
                return this.managerParameters;
            }
        }
        #endregion

        public UIMouseObject(string id, ActorType actorType, StatusType statusType, Transform2D transform,
            Color color, SpriteEffects spriteEffects, SpriteFont spriteFont,
            string text, Vector2 textOffsetPosition, Color textColor,
            float layerDepth, Texture2D texture, Rectangle sourceRectangle, Vector2 origin,
            ManagerParameters managerParameters,
            float pickStartDistance, float pickEndDistance, Predicate<CollidableObject> collisionPredicate)
            : base(id, actorType, statusType, transform, color, spriteEffects, layerDepth, texture, sourceRectangle, origin)
        {
            this.spriteFont = spriteFont;
            this.Text = text;
            this.textOffsetPosition = textOffsetPosition;
            this.textColor = textColor;
            this.managerParameters = managerParameters;
            this.pickStartDistance = pickStartDistance;
            this.pickEndDistance = pickEndDistance;

            this.collisionPredicate = collisionPredicate;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            //draw mouse reticule
            spriteBatch.Draw(this.Texture, this.Transform.Translation,
                this.SourceRectangle, this.Color,
                MathHelper.ToRadians(this.Transform.RotationInDegrees),
                Origin, //bug fix for off centre rotation - uses explicitly specified origin and not this.Transform.Origin
                this.Transform.Scale, this.SpriteEffects, this.LayerDepth);

            //draw any additional text
            if (this.text != null)
                spriteBatch.DrawString(this.spriteFont, this.text,
                    this.Transform.Translation + textOffsetPosition, this.textColor, 0, this.textOrigin, 1, SpriteEffects.None, this.LayerDepth);
        }

        public override void Update(GameTime gameTime)
        {
            UpdateMousePosition(gameTime);
            HandleMousePick(gameTime);
            base.Update(gameTime);
        }

        private void HandleMousePick(GameTime gameTime)
        {
            if (this.managerParameters.CameraManager.ActiveCamera != null)
            {
                Camera3D camera = this.managerParameters.CameraManager.ActiveCamera;
                CollidableObject collidableObject = this.managerParameters.MouseManager.GetPickedObject(camera, camera.ViewportCentre,
                    this.pickStartDistance, this.pickEndDistance, out pos, out normal) as CollidableObject;


                //if the collision was valid (i.e. validity as defined by the if() statement in MyUIMouseObject::HandleCollision()) 
                if (IsValidCollision(collidableObject, pos, normal))
                {
                    HandleCollision(gameTime, collidableObject, pos, normal);
                    UpdateMouseAppearanceOnCollision(gameTime, collidableObject, pos, normal);
                }
                else  //if not colliding with anything of interest
                {
                    HandleNoCollision(gameTime);
                    ResetMouseAppearanceOnNoCollision(gameTime);
                }
            }
        }

        //move the texture for the mouse object to be where the mouse pointer is
        protected virtual void UpdateMousePosition(GameTime gameTime)
        {
            this.Transform.Translation = this.managerParameters.MouseManager.Position;
        }

        //called when over collidable/pickable object
        protected virtual bool IsValidCollision(CollidableObject collidableObject, Vector3 pos, Vector3 normal)
        {
            if (collidableObject != null)
                return collisionPredicate(collidableObject);
            else
                return false;
        }

        //handle collision and listen for input from all possibly input modalities (e.g. keyboard etc)
        protected virtual void HandleCollision(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal)
        {
            float distanceToObject = 0;
            HandleMouseInputOnCollision(gameTime, collidableObject, pos, normal, out distanceToObject);
            HandleKeyboardInputOnCollision(gameTime, collidableObject, pos, normal, distanceToObject);
            HandleGamePadInputOnCollision(gameTime, collidableObject, pos, normal, distanceToObject);
            UpdateMouseText(gameTime, collidableObject, pos, normal, distanceToObject);
        }

        //update the text associated with the mouse accordingly
        protected virtual void UpdateMouseText(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal, float distanceToObject)
        {

        }

        //call this method if mouse is over a valid collidable object to listen for any mouse events e.g. left click
        protected virtual void HandleMouseInputOnCollision(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal, out float distanceToObject)
        {
            distanceToObject = (float)Math.Round(Vector3.Distance(this.ManagerParameters.CameraManager.ActiveCamera.Transform.Translation, pos), 1);
        }

        //call this method if mouse is over a valid collidable object to listen for any keyboard events e.g. Enter
        protected virtual void HandleKeyboardInputOnCollision(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal, float distanceToObject)
        {

        }

        //call this method if mouse is over a valid collidable object to listen for any gamepad events e.g. DPad button press
        protected virtual void HandleGamePadInputOnCollision(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal, float distanceToObject)
        {

        }

        //resets when no mouse over valid collidable object (i.e. validity as defined by the if() statement in MyUIMouseObject::HandleCollision()) 
        protected virtual void HandleNoCollision(GameTime gameTime)
        {

        }

        //define what mouse does when we are OVER a valid collidable object
        protected virtual void UpdateMouseAppearanceOnCollision(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal)
        {
        }

        //define what mouse does when we are NOT OVER a valid collidable object
        protected virtual void ResetMouseAppearanceOnNoCollision(GameTime gameTime)
        {
        }
    }
}
