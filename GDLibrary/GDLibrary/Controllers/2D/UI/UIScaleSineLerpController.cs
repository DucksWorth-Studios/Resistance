/*
Function: 		Applies a scale change to a UI actor based on a sine wave.
Author: 		NMCG
Version:		1.0
Date Updated:	6/10/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class UIScaleSineLerpController : UIController
    {
        #region Fields

        #endregion

        public UIScaleSineLerpController(string id, ControllerType controllerType,
            TrigonometricParameters trigonometricParameters)
            : base(id, controllerType)
        {
            TrigonometricParameters = trigonometricParameters;
        }

        #region Properties

        public TrigonometricParameters TrigonometricParameters { get; set; }

        #endregion

        public override void SetActor(IActor actor)
        {
            var uiObject = actor as UIObject;
            uiObject.Transform.Scale = uiObject.Transform.OriginalTransform2D.Scale;
        }

        protected override void ApplyController(GameTime gameTime, UIObject uiObject, float totalElapsedTime)
        {
            //sine wave in the range 0 -> max amplitude
            var lerpFactor = MathUtility.SineLerpByElapsedTime(TrigonometricParameters, totalElapsedTime);
            //apply scale change
            uiObject.Transform.Scale = uiObject.Transform.OriginalTransform2D.Scale + Vector2.One * lerpFactor;
        }

        public override bool Equals(object obj)
        {
            var other = obj as UIScaleSineLerpController;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return TrigonometricParameters.Equals(other.TrigonometricParameters)
                   && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + ID.GetHashCode();
            hash = hash * 17 + TrigonometricParameters.GetHashCode();
            return hash;
        }

        public override object Clone()
        {
            return new UIScaleSineLerpController("clone - " + ID, //deep
                ControllerType, //deep
                (TrigonometricParameters) TrigonometricParameters.Clone() //deep
            );
        }
    }
}