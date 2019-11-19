using System.Security.Policy;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class BookcaseController : Controller
    {
        private bool opened = false;
        private bool opening = true;
        
        public BookcaseController(string id, ControllerType controllerType) : base(id, controllerType)
        {
        }
        
        #region Event Handeling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.animationTriggered += RotateBookcase;
            base.RegisterForEventHandling(eventDispatcher);
        }

        protected void RotateBookcase(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OpenBookcase)
                opening = true;
        }

        #endregion

        public override void Update(GameTime gameTime, IActor actor)
        {
            Actor3D parent = actor as Actor3D;

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
                }
            }

            base.Update(gameTime, actor);
        }
    }
}