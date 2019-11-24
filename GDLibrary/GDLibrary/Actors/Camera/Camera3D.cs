/*
Function: 		Represents a simple static camera in our 3D world to which we will later attach controllers. 
Author: 		NMCG
Version:		1.1
Date Updated:	24/8/17
Bugs:			None
Fixes:			None
*/


using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    //Represents the base camera class to which controllers can be attached (to do...)
    public class Camera3D : Actor3D
    {
        //constructor with default drawDepth set to 0
        public Camera3D(string id, ActorType actorType,
            Transform3D transform, ProjectionParameters projectionParameters,
            Viewport viewPort, StatusType statusType)
            : this(id, actorType, transform, projectionParameters, viewPort, 0, statusType)
        {
        }

        public Camera3D(string id, ActorType actorType,
            Transform3D transform, ProjectionParameters projectionParameters,
            Viewport viewPort, float drawDepth, StatusType statusType)
            : base(id, actorType, transform, statusType)
        {
            ProjectionParameters = projectionParameters;
            Viewport = viewPort;
            DrawDepth = drawDepth;
        }

        //creates a default camera3D - we can use this for a fixed camera archetype i.e. one we will clone - see MainApp::InitialiseCameras()
        public Camera3D(string id, ActorType actorType, Viewport viewPort)
            : this(id, actorType, Transform3D.Zero,
                ProjectionParameters.StandardMediumFourThree, viewPort, 0, StatusType.Update)
        {
        }

        public override bool Equals(object obj)
        {
            var other = obj as Camera3D;

            return Equals(Transform.Translation, other.Transform.Translation)
                   && Equals(Transform.Look, other.Transform.Look)
                   && Equals(Transform.Up, other.Transform.Up)
                   && ProjectionParameters.Equals(other.ProjectionParameters);
        }

        public override int GetHashCode() //a simple hash code method 
        {
            var hash = 1;
            hash = hash * 31 + Transform.Translation.GetHashCode();
            hash = hash * 17 + Transform.Look.GetHashCode();
            hash = hash * 13 + Transform.Up.GetHashCode();
            hash = hash * 53 + ProjectionParameters.GetHashCode();
            return hash;
        }

        public new object Clone()
        {
            return new Camera3D("clone - " + ID,
                ActorType, (Transform3D) Transform.Clone(),
                (ProjectionParameters) ProjectionParameters.Clone(), Viewport, 0, StatusType.Update);
        }

        public override string ToString()
        {
            return ID
                   + ", Translation: " + MathUtility.Round(Transform.Translation, 0)
                   + ", Look: " + MathUtility.Round(Transform.Look, 0)
                   + ", Up: " + MathUtility.Round(Transform.Up, 0)
                   + ", Depth: " + DrawDepth;
        }

        #region Fields

        private Viewport viewPort;

        //centre for each cameras viewport - important when deciding how much to turn the camera when a particular camera view, in a multi-screen layout, is in focus

        //used to sort cameras by depth on screen where 0 = top-most, 1 = bottom-most (i.e. 0 for rear-view mirror and > 0 for main game screen)

        #endregion

        #region Properties

        public Matrix View =>
            Matrix.CreateLookAt(Transform.Translation,
                Transform.Translation + Transform.Look,
                Transform.Up);

        public Matrix Projection => ProjectionParameters.Projection;

        public ProjectionParameters ProjectionParameters { get; set; }

        public Viewport Viewport
        {
            get => viewPort;
            set
            {
                viewPort = value;
                ViewportCentre = new Vector2(viewPort.Width / 2.0f, viewPort.Height / 2.0f);
            }
        }

        public Vector2 ViewportCentre { get; private set; }

        public float DrawDepth { get; set; }

        public BoundingFrustum BoundingFrustum => new BoundingFrustum(View * ProjectionParameters.Projection);

        #endregion
    }
}