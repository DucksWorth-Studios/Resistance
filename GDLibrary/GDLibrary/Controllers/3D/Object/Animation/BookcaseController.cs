/*
Function: 		Rotates the bookcase model based on animation triggers
Author: 		Cameron
*/

using JigLibX.Collision;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class BookcaseController : Controller
    {
        private bool opened = false;
        private bool opening = false;
        private CollidableObject parent;
        private Box bookcaseCollision;
        private MaterialProperties collisionProperties;

        /*
         * Authors: Cameron & Tomas
         */
        public BookcaseController(string id, ControllerType controllerType, EventDispatcher eventDispatcher, 
            Box bookcaseCollision, MaterialProperties collisionProperties) : base(id, controllerType)
        {
            RegisterForEventHandling(eventDispatcher);
            this.bookcaseCollision = bookcaseCollision;
            this.collisionProperties = collisionProperties;
        }

        #region Event Handeling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.animationTriggered += RotateBookcase;
            eventDispatcher.Reset += Reset;
            base.RegisterForEventHandling(eventDispatcher);
        }

        protected void RotateBookcase(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OpenBookcase)
                opening = true;
        }

        protected void Reset(EventData eventData)
        {
            if (this.parent != null && parent.Transform.Rotation.Y >= 90)
            {
                opened = false;
                
                this.parent.Transform.RotateAroundYBy(-90);

                parent.Collision.RemoveAllPrimitives();
                parent.Collision.AddPrimitive(bookcaseCollision, collisionProperties);
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
                if (parent.Transform.Rotation.Y < 90)
                {
                    parent.Transform.RotateAroundYBy(1);
                }
                else if (parent.Transform.Rotation.Y >= 90)
                {
                    opened = true;
                    opening = false;

                    //TODO - Ask Niall why neither of these work
                    /*
                    parent.Transform.TranslateTo(new Vector3(parent.Transform.Translation.X - 10,
                        parent.Transform.Translation.Y,
                        parent.Transform.Translation.Z - 10));
                    parent.Transform.TranslateBy(new Vector3(-10, 0, -10));
                    */

                    parent.Collision.RemoveAllPrimitives();
                    //NCMG
                    parent.Collision.AddPrimitive(new Box(this.bookcaseCollision.Position, Matrix.Identity,
                            new Vector3(this.bookcaseCollision.SideLengths.Y, 
                                this.bookcaseCollision.SideLengths.Z, 
                                this.bookcaseCollision.SideLengths.X)),
                        this.collisionProperties);
                    //y    z    x
                }
            }

            base.Update(gameTime, actor);
        }
    }
}