/*
Function: 		Rotates the door model based on animation triggers
Author: 		Cameron
*/

using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Math;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class DoorController : Controller
    {
        private bool opened = false;
        private bool opening = false;
        private CollidableObject parent;
        private Box doorCollision;
        private MaterialProperties collisionProperties;

        /*
         * Authors: Cameron & Tomas
         */
        public DoorController(string id, ControllerType controllerType, EventDispatcher eventDispatcher, 
            Box doorCollision, MaterialProperties collisionProperties) : base(id, controllerType)
        {
            RegisterForEventHandling(eventDispatcher);
            this.doorCollision = doorCollision;
            this.collisionProperties = collisionProperties;
        }

        #region Event Handeling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.animationTriggered += OpenDoor;
            eventDispatcher.Reset += Reset;
            base.RegisterForEventHandling(eventDispatcher);
        }

        protected void OpenDoor(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OpenDoor)
                opening = true;
        }

        protected void Reset(EventData eventData)
        {
            if (this.parent != null  && parent.Transform.Rotation.Y <= 90)
            {
                opened = false;
                this.parent.Transform.RotateAroundYBy(90);

                //TODO - Find a better way of updating collision
                this.parent.Collision.RemoveAllPrimitives();
                this.parent.Collision.AddPrimitive(doorCollision, collisionProperties);
            }
        }

        #endregion

        public override void Update(GameTime gameTime, IActor actor)
        {
            CollidableObject parent = actor as CollidableObject;

            if (this.parent == null)
                this.parent = parent;

            if (opening && !opened)
            {
                if (parent.Transform.Rotation.Y > 90)
                    parent.Transform.RotateAroundYBy(-1);
                else if (parent.Transform.Rotation.Y <= 90)
                {
                    opened = true;
                    opening = false;

                    parent.Collision.RemoveAllPrimitives();
                    
                    parent.Collision.AddPrimitive(new Box(this.doorCollision.Position, Matrix.Identity,
                            new Vector3(this.doorCollision.SideLengths.Z, 
                                this.doorCollision.SideLengths.X, 
                                this.doorCollision.SideLengths.Y)),
                        this.collisionProperties);
                }
            }

            base.Update(gameTime, actor);
        }
    }
}