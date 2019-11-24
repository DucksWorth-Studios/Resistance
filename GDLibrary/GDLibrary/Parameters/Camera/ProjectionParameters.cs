/*
Function: 		Encapsulates the projection matrix specific parameters for the camera class
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
    public class ProjectionParameters : ICloneable
    {
        public ProjectionParameters(Rectangle rectangle,
            float nearClipPlane, float farClipPlane)
        {
            Rectangle = rectangle;
            NearClipPlane = nearClipPlane;
            FarClipPlane = farClipPlane;
            IsPerspectiveProjection = false;
            originalProjectionParameters = (ProjectionParameters) Clone();
        }

        public ProjectionParameters(float fieldOfView, float aspectRatio,
            float nearClipPlane, float farClipPlane)
        {
            FOV = fieldOfView;
            AspectRatio = aspectRatio;
            NearClipPlane = nearClipPlane;
            FarClipPlane = farClipPlane;
            IsPerspectiveProjection = true;
            originalProjectionParameters = (ProjectionParameters) Clone();
        }

        public object Clone() //deep copy
        {
            //remember we can use a simple this.MemberwiseClone() because all fields are primitive C# types
            return MemberwiseClone();
        }

        public void Reset()
        {
            FOV = originalProjectionParameters.FOV;
            AspectRatio = originalProjectionParameters.AspectRatio;
            NearClipPlane = originalProjectionParameters.NearClipPlane;
            FarClipPlane = originalProjectionParameters.FarClipPlane;
            Rectangle = originalProjectionParameters.Rectangle;
            IsPerspectiveProjection = originalProjectionParameters.IsPerspectiveProjection;
        }

        public override bool Equals(object obj)
        {
            var other = obj as ProjectionParameters;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return Equals(FOV, other.FOV)
                   && Equals(AspectRatio, other.AspectRatio)
                   && Equals(NearClipPlane, other.NearClipPlane)
                   && Equals(FarClipPlane, other.FarClipPlane)
                   && Rectangle.Equals(other.Rectangle);
        }

        public override int GetHashCode() //a simple hash code method 
        {
            var hash = 1;
            hash = hash * 31 + FOV.GetHashCode();
            hash = hash * 17 + AspectRatio.GetHashCode();
            hash = hash * 13 + NearClipPlane.GetHashCode();
            hash = hash * 59 + FarClipPlane.GetHashCode();
            hash = hash * 53 + Rectangle.GetHashCode();
            return hash;
        }

        #region Statics

        //Deep relates to the distance between the near and far clipping planes i.e. 1 to 2500
        public static ProjectionParameters StandardDeepFiveThree =>
            new ProjectionParameters(MathHelper.PiOver2, 5.0f / 3, 0.1f, 2500);

        public static ProjectionParameters StandardDeepFourThree =>
            new ProjectionParameters(MathHelper.PiOver2, 4.0f / 3, 0.1f, 2500);


        public static ProjectionParameters StandardDeepSixteenTen =>
            new ProjectionParameters(MathHelper.PiOver2, 16.0f / 10, 0.1f, 2500);

        public static ProjectionParameters StandardDeepSixteenNine =>
            new ProjectionParameters(MathHelper.PiOver4, 16.0f / 9, 1, 2500);

        //Medium relates to the distance between the near and far clipping planes i.e. 1 to 1000
        public static ProjectionParameters StandardMediumFiveThree =>
            new ProjectionParameters(MathHelper.PiOver2, 5.0f / 3, 0.1f, 1000);

        public static ProjectionParameters StandardMediumFourThree =>
            new ProjectionParameters(MathHelper.PiOver2, 4.0f / 3, 0.1f, 1000);

        public static ProjectionParameters StandardMediumSixteenTen =>
            new ProjectionParameters(MathHelper.PiOver2, 16.0f / 10, 0.1f, 1000);

        public static ProjectionParameters StandardMediumSixteenNine =>
            new ProjectionParameters(MathHelper.PiOver4, 16.0f / 9, 0.1f, 1000);

        //Shallow relates to the distance between the near and far clipping planes i.e. 1 to 500
        public static ProjectionParameters StandardShallowFiveThree =>
            new ProjectionParameters(MathHelper.PiOver2, 5.0f / 3, 0.1f, 500);

        public static ProjectionParameters StandardShallowFourThree =>
            new ProjectionParameters(MathHelper.PiOver2, 4.0f / 3, 0.1f, 500);

        public static ProjectionParameters StandardShallowSixteenTen =>
            new ProjectionParameters(MathHelper.PiOver2, 16.0f / 10, 0.1f, 500);

        public static ProjectionParameters StandardShallowSixteenNine =>
            new ProjectionParameters(MathHelper.PiOver2, 16.0f / 9, 0.1f, 500);

        #endregion

        #region Fields

        //used by perspective projections
        private float fieldOfView, aspectRatio, nearClipPlane, farClipPlane;

        //used by orthographic projections
        private Rectangle rectangle;
        private bool isPerspectiveProjection;

        //used by both
        private Matrix projection;
        private readonly ProjectionParameters originalProjectionParameters;
        private bool isDirty;

        #endregion

        #region Properties

        #region Orthographic Specific Properties

        public Rectangle Rectangle
        {
            get => rectangle;
            set
            {
                rectangle = value;
                isDirty = true;
            }
        }

        public bool IsPerspectiveProjection
        {
            get => isPerspectiveProjection;
            set
            {
                isPerspectiveProjection = value;
                isDirty = true;
            }
        }

        #endregion

        #region Perspective Specific Properties

        public float FOV
        {
            get => fieldOfView;
            set
            {
                fieldOfView = value;
                isDirty = true;
            }
        }

        public float AspectRatio
        {
            get => aspectRatio;
            set
            {
                aspectRatio = value;
                isDirty = true;
            }
        }

        #endregion

        public float NearClipPlane
        {
            get => nearClipPlane;
            set
            {
                nearClipPlane = value;
                isDirty = true;
            }
        }

        public float FarClipPlane
        {
            get => farClipPlane;
            set
            {
                farClipPlane = value;
                isDirty = true;
            }
        }

        public Matrix Projection
        {
            get
            {
                if (isDirty)
                {
                    if (isPerspectiveProjection)
                        projection = Matrix.CreatePerspectiveFieldOfView(
                            fieldOfView, aspectRatio,
                            nearClipPlane, farClipPlane);
                    else
                        projection = Matrix.CreateOrthographicOffCenter(
                            rectangle.X, rectangle.Y, rectangle.X, rectangle.Y,
                            nearClipPlane, farClipPlane);
                    isDirty = false;
                }

                return projection;
            }
        }

        #endregion
    }
}