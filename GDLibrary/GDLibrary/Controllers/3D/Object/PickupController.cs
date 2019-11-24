using Microsoft.Xna.Framework;

namespace GDLibrary
{
    /// <summary>
    ///     Applied to an object when it is picked up to "animate" removal - it floats upwards and fades away
    /// </summary>
    public class PickupController : Controller
    {
        private readonly float alphaDecayRate;
        private readonly float alphaDecayThreshold;
        private readonly float rotationRate;
        private readonly Vector3 translationRate;
        private readonly Vector3 scaleRate;

        public PickupController(string id, ControllerType controllerType, float rotationRate, Vector3 translationRate,
            Vector3 scaleRate,
            float alphaDecayRate, float alphaDecayThreshold)
            : base(id, controllerType)
        {
            this.rotationRate = rotationRate;
            this.translationRate = translationRate;
            this.scaleRate = scaleRate;
            this.alphaDecayRate = alphaDecayRate;
            this.alphaDecayThreshold = alphaDecayThreshold;
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            var parentActor = actor as DrawnActor3D;

            //makes the object spin upwards and fade away after its alpha is lower than a threshold value
            parentActor.Transform.RotateAroundYBy(rotationRate);
            parentActor.Transform.TranslateBy(translationRate * gameTime.ElapsedGameTime.Milliseconds);
            parentActor.Transform.ScaleBy(scaleRate);
            parentActor.Alpha += alphaDecayRate;

            //if alpha less than some threshold value then remove
            if (parentActor.Alpha < alphaDecayThreshold)
                EventDispatcher.Publish(new EventData(parentActor, EventActionType.OnRemoveActor,
                    EventCategoryType.SystemRemove));
        }
    }
}