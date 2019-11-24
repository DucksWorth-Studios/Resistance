using System;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class Curve1D
    {
        //See CurveLoopType - https://msdn.microsoft.com/en-us/library/microsoft.xna.framework.curvelooptype.aspx
        public Curve1D(CurveLoopType curveLookType)
        {
            CurveLookType = curveLookType;

            Curve = new Curve();
            Curve.PreLoop = curveLookType;
            Curve.PostLoop = curveLookType;
        }

        public void Add(float value, float timeInSecs)
        {
            timeInSecs *= 1000; //convert to milliseconds
            Curve.Keys.Add(new CurveKey(timeInSecs, value));
            bSet = false;
            //Set();
        }

        private void Set()
        {
            SetTangents(Curve);
            bSet = true;
        }

        public void Clear()
        {
            Curve.Keys.Clear();
        }

        public float Evaluate(float timeInSecs, int decimalPrecision)
        {
            if (!bSet)
                Set();

            return (float) Math.Round(Curve.Evaluate(timeInSecs), decimalPrecision);
        }

        private void SetTangents(Curve curve)
        {
            CurveKey prev;
            CurveKey current;
            CurveKey next;
            int prevIndex;
            int nextIndex;
            for (var i = 0; i < curve.Keys.Count; i++)
            {
                prevIndex = i - 1;
                if (prevIndex < 0) prevIndex = i;

                nextIndex = i + 1;
                if (nextIndex == curve.Keys.Count) nextIndex = i;

                prev = curve.Keys[prevIndex];
                next = curve.Keys[nextIndex];
                current = curve.Keys[i];
                SetCurveKeyTangent(ref prev, ref current, ref next);
                curve.Keys[i] = current;
            }
        }

        private static void SetCurveKeyTangent(ref CurveKey prev, ref CurveKey cur, ref CurveKey next)
        {
            var dt = next.Position - prev.Position;
            var dv = next.Value - prev.Value;
            if (Math.Abs(dv) < float.Epsilon)
            {
                cur.TangentIn = 0;
                cur.TangentOut = 0;
            }
            else
            {
                // The in and out tangents should be equal to the 
                // slope between the adjacent keys.
                cur.TangentIn = dv * (cur.Position - prev.Position) / dt;
                cur.TangentOut = dv * (next.Position - cur.Position) / dt;
            }
        }

        #region Fields

        private bool bSet;

        #endregion

        #region Properties

        public CurveLoopType CurveLookType { get; }

        public Curve Curve { get; }

        #endregion

        //Add Equals, Clone, ToString, GetHashCode...
    }
}