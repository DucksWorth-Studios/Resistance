/*
Function: 		Encapsulates the color and alpha fields for any drawn 2D or 3D object
Author: 		NMCG
Version:		1.0
Date Updated:	6/10/17
Bugs:			None
Fixes:			None
*/
using Microsoft.Xna.Framework;
using System;

namespace GDLibrary
{
    public class ColorParameters : ICloneable
    {
        #region Statics
        public static ColorParameters WhiteOpaque = new ColorParameters(Color.White, 1);
        //use this setting when we are rendering a model that has a texture with transparent pixels - see Main::InitializeNonCollidableFoliage()
        public static ColorParameters WhiteAlmostOpaque = new ColorParameters(Color.White, 0.99f);
        #endregion

        #region Fields
        private Color color;
        private float alpha;
        private ColorParameters originalColorParameters;
       
        #endregion

        #region Properties
        public Color Color
        {
            get
            {
                return this.color;
            }
            set
            {
                this.color = value;
            }
        }
        public float Alpha
        {
            get
            {
                return this.alpha;
            }
            set
            {
                if (value < 0)
                    this.alpha = 0;
                else if (value > 1)
                    this.alpha = 1;
                else
                    this.alpha = (float)Math.Round(value, 3);
            }
        }
        public ColorParameters OriginalColorParameters
        {
            get
            {
                return this.originalColorParameters;
            }
        }
        #endregion

        public ColorParameters(Color color, float alpha)
        {
            Initialize(color, alpha);

            //store original values in case of reset
            this.originalColorParameters = new ColorParameters();
            this.originalColorParameters.Initialize(color, alpha);
        }

        public ColorParameters()
        {

        }

        protected void Initialize(Color color, float alpha)
        {
            this.color = color;
            this.Alpha = alpha;
        }

        public void Reset()
        {
            Initialize(this.originalColorParameters.Color, this.originalColorParameters.Alpha);
        }

        public override bool Equals(object obj)
        {
            ColorParameters other = obj as ColorParameters;
            return this.color == other.Color && this.alpha == other.Alpha;
        }

        public override int GetHashCode()
        {
            int hash = 1;
            hash = hash * 31 + this.color.GetHashCode();
            hash = (int)(hash * 17 + this.alpha);
            return hash;
        }

        public object Clone()
        {
            //deep because all variables are either C# types (e.g. primitives, structs, or enums) or  XNA types
            return this.MemberwiseClone();
        }
    }
}
