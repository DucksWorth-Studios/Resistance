using System;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    /*
     * Allow the user to pass in offsets for a 3D curve so that platforms can use the same curve but
     * operate "out of sync" by the offsets specified. 
     */
    public class Transform3DCurveOffsets : ICloneable
    {
        public static Transform3DCurveOffsets Zero = new Transform3DCurveOffsets(Vector3.Zero, Vector3.One, 0, 0);

        public Transform3DCurveOffsets(Vector3 position, Vector3 scale, float rotation, float timeInSecs)
        {
            Position = position;
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

        public Vector3 Position { get; set; }

        public Vector3 Scale { get; set; }

        public float Rotation { get; set; }

        public float TimeInSecs { get; set; }

        public float TimeInMs => TimeInSecs * 1000;

        #endregion
    }

    //Represents a 3D point on a camera curve (i.e. position, look, and up) at a specified time in seconds
    public class Transform3DCurve
    {
        #region Fields

        private readonly Curve3D translationCurve;
        private readonly Curve3D lookCurve;
        private readonly Curve3D upCurve;

        #endregion

        public Transform3DCurve(CurveLoopType curveLoopType)
        {
            translationCurve = new Curve3D(curveLoopType);
            lookCurve = new Curve3D(curveLoopType);
            upCurve = new Curve3D(curveLoopType);
        }

        public void Add(Vector3 translation, Vector3 look, Vector3 up, float timeInSecs)
        {
            translationCurve.Add(translation, timeInSecs);
            lookCurve.Add(look, timeInSecs);
            upCurve.Add(up, timeInSecs);
        }

        public void Clear()
        {
            translationCurve.Clear();
            lookCurve.Clear();
            upCurve.Clear();
        }

        //See https://msdn.microsoft.com/en-us/library/t3c3bfhx.aspx for information on using the out keyword
        public void Evalulate(float timeInSecs, int precision,
            out Vector3 translation, out Vector3 look, out Vector3 up)
        {
            translation = translationCurve.Evaluate(timeInSecs, precision);
            look = lookCurve.Evaluate(timeInSecs, precision);
            up = upCurve.Evaluate(timeInSecs, precision);
        }

        //Add Equals, Clone, ToString, GetHashCode...
    }
}