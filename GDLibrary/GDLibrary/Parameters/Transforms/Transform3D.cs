/*
Function: 		Encapsulates the transformation and World matrix specific parameters for any 3D entity that can have a position (e.g. a player, a prop, a camera)
Author: 		NMCG
Version:		1.0
Date Updated:	1/10/17
Bugs:			None
Fixes:			None
*/

using System;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class Transform3D : ICloneable
    {
        //used by drawn objects
        public Transform3D(Vector3 translation, Vector3 rotation, Vector3 scale, Vector3 look, Vector3 up)
        {
            Initialize(translation, rotation, scale, look, up);

            //store original values in case of reset
            OriginalTransform3D = new Transform3D();
            OriginalTransform3D.Initialize(translation, rotation, scale, look, up);
        }

        //used by the camera
        public Transform3D(Vector3 translation, Vector3 look, Vector3 up)
            : this(translation, Vector3.Zero, Vector3.One, look, up)
        {
        }

        //used by zone objects
        public Transform3D(Vector3 translation, Vector3 scale)
            : this(translation, Vector3.Zero, scale, Vector3.UnitX, Vector3.UnitY)
        {
        }

        //used internally when creating the originalTransform object
        private Transform3D()
        {
        }

        #region Statics

        //30/11/17 - fix for Transform3D.Zero - thanks to JL
        public static Transform3D Zero =>
            new Transform3D(Vector3.Zero, Vector3.Zero, Vector3.One, -Vector3.UnitZ, Vector3.UnitY);

        #endregion

        public object Clone()
        {
            //deep because all variables are either C# types (e.g. primitives, structs, or enums) or  XNA types
            return MemberwiseClone();
        }

        protected void Initialize(Vector3 translation, Vector3 rotation, Vector3 scale, Vector3 look, Vector3 up)
        {
            Translation = translation;
            Rotation = rotation;
            Scale = scale;

            Look = Vector3.Normalize(look);
            Up = Vector3.Normalize(up);
        }

        public void Reset()
        {
            translation = OriginalTransform3D.Translation;
            rotation = OriginalTransform3D.Rotation;
            Scale = OriginalTransform3D.Scale;
            look = OriginalTransform3D.Look;
            up = OriginalTransform3D.Up;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Transform3D;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return Equals(translation, other.Translation)
                   && Equals(rotation, other.Rotation)
                   && Equals(Scale, other.Scale)
                   && Equals(look, other.Look)
                   && Equals(up, other.Up);
        }

        public override int GetHashCode() //a simple hash code method 
        {
            var hash = 1;
            hash = hash * 31 + translation.GetHashCode();
            hash = hash * 17 + look.GetHashCode();
            hash = hash * 13 + up.GetHashCode();
            return hash;
        }

        public void RotateBy(Vector3 rotateBy) //in degrees
        {
            rotation = OriginalTransform3D.Rotation + rotateBy;

            //update the look and up - RADIANS!!!!
            var rot = Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(rotation.X),
                MathHelper.ToRadians(rotation.Y), MathHelper.ToRadians(rotation.Z));

            look = Vector3.Transform(OriginalTransform3D.Look, rot);
            up = Vector3.Transform(OriginalTransform3D.Up, rot);

            isDirty = true;
        }

        public void RotateAroundYBy(float magnitude) //in degrees
        {
            rotation.Y += magnitude;
            look = Vector3.Normalize(Vector3.Transform(OriginalTransform3D.Look,
                Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y))));
            isDirty = true;
        }

        /*
         * Author: Cameron
         * This will apply rotation to an object around the X axis
         */
        public void RotateAroundXBy(float magnitude) //in degrees
        {
            rotation.X += magnitude;
            look = Vector3.Normalize(Vector3.Transform(OriginalTransform3D.Look,
                Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X))));
            isDirty = true;
        }

        /*
         * Author: Cameron
         * This will apply rotation to an object around the Z axis
         */
        public void RotateAroundZBy(float magnitude) //in degrees
        {
            rotation.Z += magnitude;
            look = Vector3.Normalize(Vector3.Transform(OriginalTransform3D.Look,
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z))));
            isDirty = true;
        }

        /*
         * Author: Cameron
         * This will apply rotation to an object around the Z axis
         */
        public void RotateAroundZBy(Vector3 lookVector, float magnitude) //in degrees
        {
            rotation.Z += magnitude;
            look = Vector3.Normalize(Vector3.Transform(lookVector,
                Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z))));
            isDirty = true;
        }

        public void TranslateTo(Vector3 translate)
        {
            translation = translate;
            isDirty = true;
        }

        public void TranslateBy(Vector3 translateBy)
        {
            translation += translateBy;
            isDirty = true;
        }

        public void ScaleTo(Vector3 scale)
        {
            Scale = scale;
            isDirty = true;
        }

        public void ScaleBy(Vector3 scaleBy)
        {
            Scale *= scaleBy;
            isDirty = true;
        }

        #region Fields

        private Vector3 translation, rotation;
        private Vector3 look, up;
        private Matrix world;
        private bool isDirty;

        #endregion

        #region Properties

        public Matrix Orientation =>
            Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X))
            * Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y))
            * Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z));

        public Matrix World
        {
            set => world = value;
            get
            {
                if (isDirty)
                {
                    world = Matrix.Identity * Matrix.CreateScale(Scale)
                                            * Matrix.CreateRotationX(MathHelper.ToRadians(rotation.X))
                                            * Matrix.CreateRotationY(MathHelper.ToRadians(rotation.Y))
                                            * Matrix.CreateRotationZ(MathHelper.ToRadians(rotation.Z))
                                            * Matrix.CreateTranslation(translation);
                    isDirty = false;
                }

                return world;
            }
        }

        public Vector3 Translation
        {
            get => translation;
            set
            {
                translation = value;
                isDirty = true;
            }
        }

        public Vector3 Rotation
        {
            get => rotation;
            set
            {
                rotation = value;
                isDirty = true;
            }
        }

        public Vector3 Scale { get; set; }

        public Vector3 Target => translation + look;

        public Vector3 Up
        {
            get => up;
            set => up = value;
        }

        public Vector3 Look
        {
            get => look;
            set => look = value;
        }

        public Vector3 Right => Vector3.Normalize(Vector3.Cross(look, up));

        public Transform3D OriginalTransform3D { get; }

        public double DistanceToCamera { get; set; }

        public Vector3 TranslateIncrement { get; internal set; }
        public int RotateIncrement { get; internal set; }

        #endregion
    }
}