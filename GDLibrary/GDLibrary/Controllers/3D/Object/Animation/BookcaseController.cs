/*
Function: 		Rotates the bookcase model based on animation triggeres
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
        
        /*
         * Author: Tomas
         */
        public BookcaseController(string id, ControllerType controllerType, EventDispatcher eventDispatcher) : base(id, controllerType)
        {
            RegisterForEventHandling(eventDispatcher);
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
            if (this.parent != null)
            {
                opened = false;
                this.parent.Transform.RotateAroundYBy(-90);
                
                //TODO - Find a better way of updating collision
                parent.Collision.RemoveAllPrimitives();
                parent.Collision.AddPrimitive(new Box(new Vector3(-64, 0, -102), Matrix.Identity, 
                        new Vector3(8f, 30.0f, 35.0f)),
                    new MaterialProperties(0.2f, 0.8f, 0.7f));
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
                    parent.Transform.TranslateTo(new Vector3(parent.Transform.Translation.X -10, 
                        parent.Transform.Translation.Y, 
                        parent.Transform.Translation.Z - 10));
                    parent.Transform.TranslateBy(new Vector3(-10, 0, -10));

                    parent.Collision.RemoveAllPrimitives();
                    //NCMG
                    parent.Collision.AddPrimitive(new Box(new Vector3(-64, 0, -102), Matrix.Identity, 
                            new Vector3(15.0f, 17.0f, 2.0f)),
                        new MaterialProperties(0.2f, 0.8f, 0.7f));
                }
            }

            base.Update(gameTime, actor);
        }
    }
}