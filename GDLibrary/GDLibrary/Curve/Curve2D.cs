using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class Curve2D
    {
        private readonly Curve1D xCurve;
        private readonly Curve1D yCurve;

        public Curve2D(CurveLoopType curveLoopType)
        {
            CurveLookType = curveLoopType;

            xCurve = new Curve1D(curveLoopType);
            yCurve = new Curve1D(curveLoopType);
        }

        public CurveLoopType CurveLookType { get; }

        public void Add(Vector2 value, float time)
        {
            xCurve.Add(value.X, time);
            yCurve.Add(value.Y, time);
        }

        public void Clear()
        {
            xCurve.Clear();
            yCurve.Clear();
        }

        public Vector2 Evaluate(float timeInSecs, int decimalPrecision)
        {
            return new Vector2(xCurve.Evaluate(timeInSecs, decimalPrecision),
                yCurve.Evaluate(timeInSecs, decimalPrecision));
        }

        //Add Equals, Clone, ToString, GetHashCode...
    }
}