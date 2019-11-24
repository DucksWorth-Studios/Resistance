using System;

namespace GDLibrary
{
    public class Integer3 : ICloneable
    {
        #region Fields

        #endregion

        public Integer3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Integer3(float x, float y, float z)
            : this((int) x, (int) y, (int) z)
        {
        }

        public Integer3(double x, double y, double z)
            : this((float) x, (float) y, (float) z)
        {
        }

        //to do - add /, + - operator methods

        public object Clone()
        {
            return MemberwiseClone();
        }


        public override string ToString()
        {
            return "(x: " + X + ", " + "y: " + Y + ", " + "z: " + Z + ")";
        }

        public static Integer3 operator *(Integer3 value, int multiplier)
        {
            return new Integer3(value.X * multiplier, value.Y * multiplier, value.Z * multiplier);
        }

        public static Integer3 operator *(int multiplier, Integer3 value)
        {
            return value * multiplier;
        }

        #region Statics

        public static Integer3 Zero => new Integer3(0, 0, 0);

        public static Integer3 One => new Integer3(1, 1, 1);

        public static Integer3 UnitX => new Integer3(1, 0, 0);

        public static Integer3 UnitY => new Integer3(0, 1, 0);

        public static Integer3 UnitZ => new Integer3(0, 0, 1);

        #endregion

        #region Properties

        public int X { get; set; }

        public int Y { get; set; }

        public int Z { get; set; }

        #endregion
    }
}