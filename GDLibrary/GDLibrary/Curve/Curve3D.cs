using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class Curve3D
    {
        private readonly Curve1D xCurve;
        private readonly Curve1D yCurve;
        private readonly Curve1D zCurve;

        public Curve3D(CurveLoopType curveLoopType)
        {
            CurveLookType = curveLoopType;

            xCurve = new Curve1D(curveLoopType);
            yCurve = new Curve1D(curveLoopType);
            zCurve = new Curve1D(curveLoopType);
        }

        public CurveLoopType CurveLookType { get; }

        public void Add(Vector3 value, float time)
        {
            xCurve.Add(value.X, time);
            yCurve.Add(value.Y, time);
            zCurve.Add(value.Z, time);
        }

        public void Clear()
        {
            xCurve.Clear();
            yCurve.Clear();
            zCurve.Clear();
        }

        public Vector3 Evaluate(float timeInSecs, int decimalPrecision)
        {
            return new Vector3(xCurve.Evaluate(timeInSecs, decimalPrecision),
                yCurve.Evaluate(timeInSecs, decimalPrecision),
                zCurve.Evaluate(timeInSecs, decimalPrecision));
        }

        //Add Equals, Clone, ToString, GetHashCode...
    }
}