/*
Function: 		Applies a color change to a UI actor based on a sine wave and user-specified min and max colours.
Author: 		NMCG
Version:		1.0
Date Updated:	6/10/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class UIColorSineLerpController : UIController
    {
        public UIColorSineLerpController(string id, ControllerType controllerType,
            TrigonometricParameters trigonometricParameters,
            Color colorMin, Color colorMax)
            : base(id, controllerType)
        {
            TrigonometricParameters = trigonometricParameters;
            this.colorMin = colorMin;
            this.colorMax = colorMax;
        }

        public override void SetActor(IActor actor)
        {
            var uiObject = actor as UIObject;
            uiObject.Color = uiObject.OriginalColor;
        }

        protected override void ApplyController(GameTime gameTime, UIObject uiObject, float totalElapsedTime)
        {
            //sine wave in the range 0 -> max amplitude
            var lerpFactor = MathUtility.SineLerpByElapsedTime(TrigonometricParameters, totalElapsedTime);
            //apply color change
            uiObject.Color = MathUtility.Lerp(colorMin, colorMax, lerpFactor);
        }

        public override bool Equals(object obj)
        {
            var other = obj as UIColorSineLerpController;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return colorMin.Equals(other.ColorMin)
                   && colorMax.Equals(other.ColorMax)
                   && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + colorMin.GetHashCode();
            hash = hash * 17 + colorMax.GetHashCode();
            hash = hash * 11 + base.GetHashCode();
            return hash;
        }

        public override object Clone()
        {
            return new UIColorSineLerpController("clone - " + ID, //deep
                ControllerType, //deep
                (TrigonometricParameters) TrigonometricParameters.Clone(), //deep
                colorMin, //deep
                colorMax); //deep
        }

        #region Fields

        private Color colorMin, colorMax;

        #endregion

        #region Properties

        public TrigonometricParameters TrigonometricParameters { get; set; }

        public Color ColorMin
        {
            get => colorMin;
            set => colorMin = value;
        }

        public Color ColorMax
        {
            get => colorMax;
            set => colorMax = value;
        }

        #endregion
    }
}