/*
Function: 		Lerps target actor's color between two user specified colors. Can be used to decorate static object and give it a visually interesting behaviour.
Author: 		NMCG
Version:		1.0
Date Updated:	24/10/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class ColorSineLerpController : SineLerpController
    {
        public ColorSineLerpController(string id,
            ControllerType controllerType,
            Color startColor, Color endColor, TrigonometricParameters trigonometricParameters)
            : base(id, controllerType, trigonometricParameters)
        {
            this.startColor = startColor;
            this.endColor = endColor;
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            var parentActor = actor as DrawnActor3D;

            if (parentActor != null)
            {
                //accumulate elapsed time - note we are not formally resetting this time if the controller becomes inactive - we should mirror the approach used for the UI sine controllers.
                totalElapsedTime += gameTime.ElapsedGameTime.Milliseconds;

                //sine wave in the range 0 -> max amplitude
                var lerpFactor = MathUtility.SineLerpByElapsedTime(TrigonometricParameters, totalElapsedTime);
                parentActor.EffectParameters.DiffuseColor = MathUtility.Lerp(startColor, endColor, lerpFactor);
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as ColorSineLerpController;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return startColor.Equals(other.StartColor)
                   && endColor.Equals(other.EndColor)
                   && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + startColor.GetHashCode();
            hash = hash * 17 + endColor.GetHashCode();
            hash = hash * 11 + base.GetHashCode();
            return hash;
        }

        public override object Clone()
        {
            return new ColorSineLerpController("clone - " + ID, //deep
                ControllerType, //deep
                startColor, //deep
                endColor, //deep
                (TrigonometricParameters) TrigonometricParameters.Clone()); //deep
        }

        #region Fields

        private Color startColor;
        private Color endColor;
        private int totalElapsedTime;

        #endregion

        #region Properties

        public Color StartColor
        {
            get => startColor;
            set => startColor = value;
        }

        public Color EndColor
        {
            get => endColor;
            set => endColor = value;
        }

        #endregion
    }
}