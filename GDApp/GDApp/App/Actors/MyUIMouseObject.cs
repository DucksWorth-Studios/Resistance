using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GDApp
{
    public class MyUIMouseObject : UIMouseObject
    {
        private static readonly string NoObjectSelectedText = "no object selected";

        /* A slightly(!) more succinct version of the constructor which doesnt require us to provide quite so many arguments
         * Note: Since the sourceRectangle is now hard-coded (i.e. new Rectangle(0, 0, texture.Width, texture.Height)) then this
         * version of the constructor will NOT allow us to specify a reticule image in a single texture containing multiple reticule textures
         * as in the texture mouseicons.png in the content folder.
         */
        public MyUIMouseObject(string id, ActorType actorType, Transform2D transform,
            SpriteFont spriteFont, string text, Vector2 textOffsetPosition, Texture2D texture, 
            ManagerParameters managerParameters, float pickStartDistance, float pickEndDistance, Predicate<CollidableObject> collisionPredicate)
            : this(id, actorType, StatusType.Update | StatusType.Drawn, transform, Color.White, SpriteEffects.None, spriteFont, 
                  text, textOffsetPosition, Color.White, 0, texture, new Rectangle(0, 0, texture.Width, texture.Height),
                    new Vector2(texture.Width/2, texture.Height/2), managerParameters, pickStartDistance, pickEndDistance, collisionPredicate)
        {

        }

        public MyUIMouseObject(string id, ActorType actorType, StatusType statusType, Transform2D transform, 
            Color color, SpriteEffects spriteEffects, SpriteFont spriteFont, string text, 
            Vector2 textOffsetPosition, Color textColor, float layerDepth, Texture2D texture, Rectangle sourceRectangle, 
            Vector2 origin, ManagerParameters managerParameters, float pickStartDistance, float pickEndDistance, Predicate<CollidableObject> collisionPredicate) 
            : base(id, actorType, statusType, transform, color, spriteEffects, spriteFont, text, textOffsetPosition, 
                  textColor, layerDepth, texture, sourceRectangle, origin, managerParameters, pickStartDistance, pickEndDistance, collisionPredicate)
        {

        }

        protected override void HandleMouseInputOnCollision(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal, 
            out float distanceToObject)
        {
            if (this.ManagerParameters.MouseManager.IsLeftButtonClickedOnce())
            {
                //what would we like to do here? remove the item since its ammo or some sort of pickup?
                EventDispatcher.Publish(new EventData(collidableObject, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));

                //increase the appropriate controller
                object[] additionalParameters = { "boing" };
                EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
            }

            //call the base to calculate distance to target
            base.HandleMouseInputOnCollision(gameTime, collidableObject, pos, normal, out distanceToObject);
        }

        protected override void UpdateMouseText(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal, float distanceToObject)
        {
            this.Text = collidableObject.ID + "- distance[" + distanceToObject + "]";
        }
        //make the reticule rotate and change color when over a collidable object
        int rotationSpeedInDegreesPerSecond = 45; //8 seconds for a full rotation
        protected override void UpdateMouseAppearanceOnCollision(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal)
        {
            this.Transform.RotationInDegrees += rotationSpeedInDegreesPerSecond * gameTime.ElapsedGameTime.Milliseconds/1000.0f;
            this.Color = Color.Yellow;
        }

        //reset the rotation and color when not over collidable object
        protected override void ResetMouseAppearanceOnNoCollision(GameTime gameTime)
        {
            this.Text = NoObjectSelectedText;
            this.Transform.RotationInDegrees = this.Transform.OriginalTransform2D.RotationInDegrees;
            this.Color = this.OriginalColor;
        }
    }
}

