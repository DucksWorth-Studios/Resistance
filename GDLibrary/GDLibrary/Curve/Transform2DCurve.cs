using System;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    /*
     * Allow the user to pass in offsets for a 2D curve so that platforms can use the same curve but
     * operate "out of sync" by the offsets specified. 
     */
    public class Transform2DCurveOffsets : ICloneable
    {
        public static Transform2DCurveOffsets Zero = new Transform2DCurveOffsets(Vector2.Zero, Vector2.One, 0, 0);

        public Transform2DCurveOffsets(Vector2 translation, Vector2 scale, float rotation, float timeInSecs)
        {
            Translation = translation;
            Scale = scale;
            Rotation = rotation;
            TimeInSecs = timeInSecs;
        }

        public object Clone()
        {
            return MemberwiseClone(); //simple C# or XNA types so use MemberwiseClone()
        }

        #region Fields

        #endregion

        #region Properties

        public Vector2 Translation { get; set; }

        public Vector2 Scale { get; set; }

        public float Rotation { get; set; }

        public float TimeInSecs { get; set; }

        public float TimeInMs => TimeInSecs * 1000;

        #endregion
    }

    //Represents a 2D point on a curve (i.e. position, rotation, and scale) at a specified time in seconds
    public class Transform2DCurve
    {
        public Transform2DCurve(CurveLoopType curveLoopType)
        {
            translationCurve = new Curve2D(curveLoopType);
            scaleCurve = new Curve2D(curveLoopType);
            rotationCurve = new Curve1D(curveLoopType);
        }

        public void Add(Vector2 translation, Vector2 scale, float rotation, float timeInSecs)
        {
            translationCurve.Add(translation, timeInSecs);
            scaleCurve.Add(scale, timeInSecs);
            rotationCurve.Add(rotation, timeInSecs);
        }

        public void Clear()
        {
            translationCurve.Clear();
            scaleCurve.Clear();
            rotationCurve.Clear();
        }

        //See https://msdn.microsoft.com/en-us/library/t3c3bfhx.aspx for information on using the out keyword
        public void Evalulate(float timeInSecs, int precision, out Vector2 translation, out Vector2 scale,
            out float rotation)
        {
            translation = translationCurve.Evaluate(timeInSecs, precision);
            scale = scaleCurve.Evaluate(timeInSecs, precision);
            rotation = rotationCurve.Evaluate(timeInSecs, precision);
        }

        #region Fields

        private readonly Curve1D rotationCurve;
        private readonly Curve2D translationCurve;
        private readonly Curve2D scaleCurve;

        #endregion

        //Add Equals, Clone, ToString, GetHashCode...
    }
}