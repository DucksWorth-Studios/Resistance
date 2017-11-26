using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GDApp
{
    public class MyUIMouseObject : UIMouseObject
    {
        #region Fields
        //statics
        private static readonly int rotationSpeedInDegreesPerSecond = 45; //8 seconds for a full rotation
        #endregion

        #region Properties
        #endregion

        /* A slightly(!) more succinct version of the constructor which doesnt require us to provide quite so many arguments
         * Note: Since the sourceRectangle is now hard-coded (i.e. new Rectangle(0, 0, texture.Width, texture.Height)) then this
         * version of the constructor will NOT allow us to specify a reticule image in a single texture containing multiple reticule textures
         * as in the texture mouseicons.png in the content folder.
         */
        public MyUIMouseObject(string id, ActorType actorType, Transform2D transform,
            SpriteFont spriteFont, string text, Vector2 textOffsetPosition, Texture2D texture, 
            ManagerParameters managerParameters, float pickStartDistance, float pickEndDistance, bool bPickAndPlaceEnabled, Predicate<CollidableObject> collisionPredicate)
            : this(id, actorType, StatusType.Update | StatusType.Drawn, transform, Color.White, SpriteEffects.None, spriteFont, 
                  text, textOffsetPosition, Color.White, 0, texture, new Rectangle(0, 0, texture.Width, texture.Height),
                    new Vector2(texture.Width/2, texture.Height/2), managerParameters, pickStartDistance, pickEndDistance, bPickAndPlaceEnabled, collisionPredicate)
        {

        }

        public MyUIMouseObject(string id, ActorType actorType, StatusType statusType, Transform2D transform, 
            Color color, SpriteEffects spriteEffects, SpriteFont spriteFont, string text, 
            Vector2 textOffsetPosition, Color textColor, float layerDepth, Texture2D texture, Rectangle sourceRectangle, 
            Vector2 origin, ManagerParameters managerParameters, float pickStartDistance, float pickEndDistance, bool bPickAndPlaceEnabled, Predicate<CollidableObject> collisionPredicate) 
            : base(id, actorType, statusType, transform, color, spriteEffects, spriteFont, text, textOffsetPosition, 
                  textColor, layerDepth, texture, sourceRectangle, origin, managerParameters, pickStartDistance, pickEndDistance, bPickAndPlaceEnabled, collisionPredicate)
        {

        }

        protected override void HandleMouseInputOnCollision(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal, 
            out float distanceToObject)
        {
            //remove and play sound
            if (this.ManagerParameters.MouseManager.IsLeftButtonClicked())
            {
                //create a projectile
                    //pass in an archetypal projectile of type CollidableObject
                    //clone the archetype
                    //set the velocity of the clone.Body i.e.collidableObject.Body.Velocity
                    //add to the objectmanager


                ////not fully functional but its not far off!
                //if(collidableObject.ActorType == ActorType.CollidableRecording)
                //{
                //    MoveablePickupObject pickupObject = collidableObject as MoveablePickupObject;
                //    string audioRecording = pickupObject.PickupParameters.AdditionalParameters[0] as string;

                //    object[] additionalParameters = { audioRecording };
                //    EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));

                //    EventDispatcher.Publish(new EventData(collidableObject, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));
                //}


                //do what you want here...

                #region Remove Object - Uncomment to try out
                if (!this.IsPickAndPlaceEnabled)
                {
                    //remove the object - obviously if you're picking and placing it makes no sense to remove the object
                    EventDispatcher.Publish(new EventData(collidableObject, EventActionType.OnRemoveActor, EventCategoryType.SystemRemove));
                    //if you do remove dont forget to reset this variable to say that we're no longer picking anything
                    this.CurrentPickedCollidableObject = null;
                }
                #endregion

                #region Play Sound- Uncomment to try out - Bug - Sound not playing - 25/11/17 - NMCG
                //play a sound - you could store a audio cue in the PickupParameter of the Im-/MoveablePickupObject to play a recording like a tape recording in a game
                //object[] additionalParameters = { "boing" };
                //EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));
                #endregion

            }

            //call the base to calculate distance to target
            base.HandleMouseInputOnCollision(gameTime, collidableObject, pos, normal, out distanceToObject);
        }

        protected override void UpdateMouseText(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal, float distanceToObject)
        {
            this.Text = collidableObject.ID + "- distance[" + distanceToObject + "]";
        }

        protected override void SetAppearanceOnCollision(GameTime gameTime, CollidableObject collidableObject, Vector3 pos, Vector3 normal)
        {
            this.Transform.RotationInDegrees += rotationSpeedInDegreesPerSecond * gameTime.ElapsedGameTime.Milliseconds/1000.0f;
            this.Color = Color.Yellow;

            //texture of 120 x 40 with three reticule icons
           // this.SourceRectangle = new Rectangle(80, 0, 40, 40);
        }

        //reset the rotation and color when not over collidable object
        protected override void ResetAppearanceNoCollision(GameTime gameTime)
        {
            this.Text = NoObjectSelectedText;
            this.Transform.RotationInDegrees = this.Transform.OriginalTransform2D.RotationInDegrees;
            this.Color = this.OriginalColor;

            //texture of 120 x 40 with three reticule icons
            // this.SourceRectangle = new Rectangle(0, 0, 40, 40);
        }
    }
}

