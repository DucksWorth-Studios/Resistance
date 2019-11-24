/*
Function: 		Rotates the barrier models based on animation triggers
Author: 		Cameron
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class BarrierController : Controller
    {
        private bool rotateClockwise;
        private bool rotateTopBarrier = false;
        private bool rotateBottomBarrier = false;

        /*
         * Authors: Cameron & Tomas
         */
        public BarrierController(bool rotateClockwise, string id, ControllerType controllerType,
            EventDispatcher eventDispatcher) : base(id, controllerType)
        {
            this.rotateClockwise = rotateClockwise;
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
            if (eventData.EventType == EventActionType.RotateBottomBarrier)
                rotateBottomBarrier = true;
            else if (eventData.EventType == EventActionType.RotateTopBarrier)
                rotateTopBarrier = true;
        }

        #endregion

        public override void Update(GameTime gameTime, IActor actor)
        {
            CollidableObject parent = actor as CollidableObject;

            if (rotateClockwise && rotateTopBarrier)
            {
                if (parent.Transform.Rotation.Z < 270)
                    parent.Transform.RotateAroundZBy(1);
                else if (parent.Transform.Rotation.Z == 270)
                {
                    rotateTopBarrier = false;
                    parent.Collision.RemoveAllPrimitives();
                }
            }
            else if (!rotateClockwise && rotateBottomBarrier)
            {
                if (parent.Transform.Rotation.Z > -90)
                    parent.Transform.RotateAroundZBy(-1);
                else if (parent.Transform.Rotation.Z == -90)
                {
                    rotateBottomBarrier = false;
                    parent.Collision.RemoveAllPrimitives();
                }
            }

            base.Update(gameTime, actor);
        }
    }
}