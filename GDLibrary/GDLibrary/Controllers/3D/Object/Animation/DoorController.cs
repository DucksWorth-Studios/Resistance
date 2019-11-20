using System.Security.Policy;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class DoorController : Controller
    {
        private bool opened = false;
        private bool opening = false;
        
        public DoorController(string id, ControllerType controllerType, EventDispatcher eventDispatcher) : base(id, controllerType)
        {
            RegisterForEventHandling(eventDispatcher);
        }
        
        #region Event Handeling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.animationTriggered += RotateBarrier;
            base.RegisterForEventHandling(eventDispatcher);
        }

        protected void RotateBarrier(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OpenDoor)
                opening = true;
        }

        #endregion

        public override void Update(GameTime gameTime, IActor actor)
        {
            Actor3D parent = actor as Actor3D;

            if (opening && !opened)
            {
                if (parent.Transform.Rotation.Y > 90)
                    parent.Transform.RotateAroundYBy(-1);
                else if (parent.Transform.Rotation.Y <= 90)
                {
                    opened = true;
                    opening = false;
                }
            }

            base.Update(gameTime, actor);
        }
    }
}