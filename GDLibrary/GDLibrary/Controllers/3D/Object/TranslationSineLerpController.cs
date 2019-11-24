/*
Function: 		Lerps target actor's along a direction specified by the user. Can be used to decorate add simple linear movement to gates, barriers, doors.
                To add more exotic movement (i.e. along a curve) then attach a RailController to object.
Author: 		NMCG
Version:		1.0
Date Updated:	24/10/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class TranslationSineLerpController : SineLerpController
    {
        public TranslationSineLerpController(string id, ControllerType controllerType,
            Vector3 lerpDirection,
            TrigonometricParameters trigonometricParameters)
            : base(id, controllerType, trigonometricParameters)
        {
            LerpDirection = lerpDirection;
        }

        #region Properties

        public Vector3 LerpDirection
        {
            get => lerpDirection;
            set
            {
                lerpDirection = value;
                //direction only
                lerpDirection.Normalize();
            }
        }

        #endregion

        public override void Update(GameTime gameTime, IActor actor)
        {
            var parentActor = actor as Actor3D;

            if (parentActor != null)
            {
                //accumulate elapsed time - note we are not formally resetting this time if the controller becomes inactive - we should mirror the approach used for the UI sine controllers.
                totalElapsedTime += gameTime.ElapsedGameTime.Milliseconds;

                //sine wave in the range 0 -> max amplitude
                var lerpFactor = MathUtility.SineLerpByElapsedTime(TrigonometricParameters, totalElapsedTime);

                //calculate the new translation by adding to the original translation
                parentActor.Transform.Translation =
                    parentActor.Transform.OriginalTransform3D.Translation
                    + lerpFactor * lerpDirection;
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as TranslationSineLerpController;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return lerpDirection.Equals(other.LerpDirection)
                   && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + lerpDirection.GetHashCode();
            hash = hash * 11 + base.GetHashCode();
            return hash;
        }

        public override object Clone()
        {
            return new TranslationSineLerpController("clone - " + ID, //deep
                ControllerType, //deep
                lerpDirection, //deep
                (TrigonometricParameters) TrigonometricParameters.Clone()); //deep
        }

        #region Fields

        private int totalElapsedTime;
        private Vector3 lerpDirection;

        #endregion
    }
}