using GDLibrary;
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDApp
{
    public class MyUIMouseObject : UIMouseObject
    {
        public MyUIMouseObject(string id, ActorType actorType, StatusType statusType, Transform2D transform, 
            Color color, SpriteEffects spriteEffects, SpriteFont spriteFont, string text, 
            Vector2 textOffsetPosition, Color textColor, float layerDepth, Texture2D texture, Rectangle sourceRectangle, 
            Vector2 origin, MouseManager mouseManager, CameraManager cameraManager, float pickStartDistance, float pickEndDistance) 
            : base(id, actorType, statusType, transform, color, spriteEffects, spriteFont, text, textOffsetPosition, textColor, layerDepth, texture, sourceRectangle, origin, mouseManager, cameraManager, pickStartDistance, pickEndDistance)
        {

        }

        protected override void HandleCollision(CollidableObject collidableObject, Vector3 pos, Vector3 normal)
        {
            if (collidableObject.ActorType == ActorType.CollidablePickup)
            {
                float distanceToObject = Vector3.Distance(this.CameraManager.ActiveCamera.Transform.Translation, pos);
                distanceToObject = (float)Math.Round(distanceToObject, 1);
                this.Text = collidableObject.ID + "- distance[" + distanceToObject + "]";

                if (this.MouseManager.IsLeftButtonClickedOnce())
                {
                    //what would we like to do here? remove the item since its ammo or some sort of pickup?
                    EventDispatcher.Publish(new EventData(collidableObject, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));

                    //increase the appropriate controller
                    object[] additionalParameters = { "boing" };
                    EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));

                }

                //make the reticule rotate and change color when over a collidable object
                this.Transform.RotationInDegrees += 1;
                this.Color = Microsoft.Xna.Framework.Color.Red;
                //to do add object pick and placement and/or firing a projectile here...
            }
          
            //base does nothing so there's no point in calling this code
            //base.HandlePickedObject(collidableObject, pos, normal);
        }

        protected override void HandleNoCollision()
        {
            //do repeat over and over if no collidable object was picked in the previous update
            if (!this.Text.Equals("no object selected"))
            {
                this.Text = "no object selected";
                //reset the rotation and color when not over collidable object
                this.Transform.RotationInDegrees = this.Transform.OriginalTransform2D.RotationInDegrees;
                this.Color = this.OriginalColor;
                //set collidable object back to null
            }
        }
    }
}
