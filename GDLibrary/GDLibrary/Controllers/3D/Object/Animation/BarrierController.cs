using JigLibX.Math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary
{
    public class BarrierController : Controller
    {
        private bool rotateClockwise;

        public BarrierController(bool rotateClockwise, string id, ControllerType controllerType) : base(id, controllerType)
        {
            this.rotateClockwise = rotateClockwise;
        }

        #region Event Handeling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            base.RegisterForEventHandling(eventDispatcher);
        }

        #endregion

        public override void Update(GameTime gameTime, IActor actor)
        {
            Actor3D parent = actor as Actor3D;

            if (rotateClockwise)
                parent.Transform.RotateAroundZBy(1);
            else
                parent.Transform.RotateAroundZBy(-1);

            base.Update(gameTime, actor);
        }
    }
}