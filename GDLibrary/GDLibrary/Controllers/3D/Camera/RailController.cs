/*
Function: 		Rail controller constrains an actor to movement along a rail and causes the actor to focus on a target.
Author: 		NMCG
Version:		1.0
Date Updated:	30/8/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class RailController : TargetController
    {
        public RailController(string id, ControllerType controllerType, IActor targetActor,
            RailParameters railParameters)
            : base(id, controllerType, targetActor)
        {
            RailParameters = railParameters;
        }

        #region Properties

        public RailParameters RailParameters { get; set; }

        #endregion

        public override void Update(GameTime gameTime, IActor actor)
        {
            var parentActor = actor as Actor3D;
            var targetDrawnActor = TargetActor as DrawnActor3D;

            if (targetDrawnActor != null)
            {
                if (bFirstUpdate)
                {
                    //set the initial position of the camera
                    parentActor.Transform.Translation = RailParameters.MidPoint;
                    bFirstUpdate = false;
                }

                //get look vector to target
                var cameraToTarget =
                    MathUtility.GetNormalizedObjectToTargetVector(parentActor.Transform, targetDrawnActor.Transform);
                cameraToTarget =
                    MathUtility.Round(cameraToTarget,
                        3); //round to prevent floating-point precision errors across updates

                //new position for camera if it is positioned between start and the end points of the rail
                var projectedCameraPosition = parentActor.Transform.Translation +
                                              Vector3.Dot(cameraToTarget, RailParameters.Look) *
                                              RailParameters
                                                  .Look; // gameTime.ElapsedGameTime.Milliseconds; //removed gameTime multiplier - was causing camera judder when object close to camera
                projectedCameraPosition =
                    MathUtility.Round(projectedCameraPosition,
                        3); //round to prevent floating-point precision errors across updates

                //do not allow the camera to move outside the rail
                if (RailParameters.InsideRail(projectedCameraPosition))
                    parentActor.Transform.Translation = projectedCameraPosition;

                //set the camera to look at the object
                parentActor.Transform.Look = cameraToTarget;
            }
        }

        #region Fields

        private bool bFirstUpdate = true;

        #endregion

        //Add Equals, Clone, ToString, GetHashCode...
    }
}