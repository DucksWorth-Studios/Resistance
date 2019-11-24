/*
Function: 		Encapsulates the transformation, bounding rectangle, and World matrix specific parameters for any 2D entity that can have a position on screen (e.g. UI text, a clickable button, game state information)
Author: 		NMCG
Version:		1.0
Date Updated:	11/9/17
Bugs:			None
Fixes:			None
*/

using System;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class Transform2D : ICloneable
    {
        //used by dynamic sprites i.e. which need a look and right vector for movement
        public Transform2D(Vector2 translation, float rotationInDegrees, Vector2 scale,
            Vector2 origin, Integer2 dimensions)
        {
            Initialize(translation, rotationInDegrees, scale, origin, dimensions);

            //store original values in case of reset
            OriginalTransform2D = new Transform2D();
            OriginalTransform2D.Initialize(translation, rotationInDegrees, scale, origin, dimensions);
        }

        //used by static background sprites that cover the entire screen OR more than the entire screen
        public Transform2D(Vector2 scale) : this(Vector2.Zero, 0, scale, Vector2.Zero, Integer2.Zero)
        {
        }

        //used internally when creating the originalTransform object
        private Transform2D()
        {
        }

        #region Statics

        public static Transform2D One => new Transform2D(Vector2.One);

        #endregion

        public object Clone()
        {
            //deep because all variables are either C# types (e.g. primitives, structs, or enums) or  XNA types
            return MemberwiseClone();
        }

        //called by constructor to setup the object
        protected void Initialize(Vector2 translation, float rotationInDegrees, Vector2 scale, Vector2 origin,
            Integer2 dimensions)
        {
            Translation = translation;
            Scale = scale;
            RotationInDegrees = rotationInDegrees;
            Origin = origin;

            //original bounding box based on the texture source rectangle dimensions
            OriginalBounds = new Rectangle(0, 0, dimensions.X, dimensions.Y);
            originalDimensions = dimensions;
        }

        //called if we ever wish to completely reset the object (e.g. after modifying a menu button with a controller we want to reset the button's position etc)
        public virtual void Reset()
        {
            Initialize(OriginalTransform2D.Translation, OriginalTransform2D.RotationInDegrees,
                OriginalTransform2D.Scale, OriginalTransform2D.Origin, OriginalTransform2D.originalDimensions);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Transform2D;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return translation.Equals(other.Translation)
                   && RotationInRadians.Equals(other.RotationInRadians)
                   && scale.Equals(other.Scale)
                   && origin.Equals(other.Origin);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + translation.GetHashCode();
            hash = hash * 17 + RotationInRadians.GetHashCode();
            hash = hash * 13 + scale.GetHashCode();
            hash = hash * 7 + origin.GetHashCode();
            return hash;
        }

        #region Fields

        private Vector2 translation, scale;
        private float rotationInDegrees;
        private Vector2 origin;

        private bool bBoundsDirty;
        private Matrix originMatrix, scaleMatrix;

        private Rectangle bounds;
        private Integer2 originalDimensions;

        //used to reset object

        #endregion

        #region Properties     

        protected Matrix RotationMatrix { get; private set; }

        protected Matrix PositionMatrix { get; private set; }

        public Vector2 Translation
        {
            get => translation;
            set
            {
                translation = value;
                PositionMatrix = Matrix.CreateTranslation(new Vector3(translation, 0));
                bBoundsDirty = true;
            }
        }

        public float RotationInDegrees
        {
            get => rotationInDegrees;
            set
            {
                rotationInDegrees = value;
                rotationInDegrees %= 360;
                RotationInRadians = MathHelper.ToRadians(rotationInDegrees);
                RotationMatrix = Matrix.CreateRotationZ(RotationInRadians);
                bBoundsDirty = true;
            }
        }

        public float RotationInRadians { get; private set; }

        public Vector2 Scale
        {
            get => scale;
            set
            {
                //do not allow scale to go to zero
                scale = value != Vector2.Zero ? value : Vector2.One;
                scaleMatrix = Matrix.CreateScale(new Vector3(scale, 1));
                bBoundsDirty = true;
            }
        }

        public Vector2 Origin
        {
            get => origin;
            set
            {
                origin = value;
                originMatrix = Matrix.CreateTranslation(new Vector3(-origin, 0));
                bBoundsDirty = true;
            }
        }

        public Matrix World => originMatrix * scaleMatrix * RotationMatrix * PositionMatrix;

        public Rectangle Bounds
        {
            get
            {
                if (bBoundsDirty)
                {
                    //calculate where the new bounding box is in screen space based on the ISRoT transformation from the World matrix
                    bounds = CollisionUtility.CalculateTransformedBoundingRectangle(OriginalBounds, World);
                    bBoundsDirty = false;
                }

                return bounds;
            }
        }

        public Rectangle OriginalBounds { get; private set; }

        public Transform2D OriginalTransform2D { get; }

        #endregion
    }
}