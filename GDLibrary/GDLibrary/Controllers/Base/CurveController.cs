/*
Function: 		Creates a controller which moves the attached object along a user defined 3D curve
Author: 		NMCG
Version:		1.0
Date Updated:	30/8/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
namespace GDLibrary
{
    //used to cause an object (or camera) to move along a predefined track
    public class CurveController : Controller
    {
        #region Variables
        private Transform3DCurve transform3DCurve;
        private PlayStatusType playStatusType;
        private float elapsedTimeInMs;
        #endregion

        #region Properties
        public Transform3DCurve Transform3DCurve
        {
            get
            {
                return this.transform3DCurve;
            }
            set
            {
                this.transform3DCurve = value;
            }
        }
        public PlayStatusType PlayStatusType
        {
            get
            {
                return this.playStatusType;
            }
            set
            {
                this.playStatusType = value;
            }
        }
        #endregion


        public CurveController(string id, 
            ControllerType controllerType,
            Transform3DCurve transform3DCurve, PlayStatusType playStatusType)
            : base(id, controllerType)
        {
            this.transform3DCurve = transform3DCurve;
            this.playStatusType = playStatusType;
            this.elapsedTimeInMs = 0;
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            Actor3D parentActor = actor as Actor3D;

            if (this.playStatusType == PlayStatusType.Play)
                UpdateTrack(gameTime, parentActor);
            else if ((this.playStatusType == PlayStatusType.Reset) || (this.playStatusType == PlayStatusType.Stop))
                this.elapsedTimeInMs = 0;
        }

        private void UpdateTrack(GameTime gameTime, Actor3D parentActor)
        {
            if (parentActor != null)
            {
                this.elapsedTimeInMs += gameTime.ElapsedGameTime.Milliseconds;

                Vector3 translation, look, up;
                this.transform3DCurve.Evalulate(elapsedTimeInMs, 2,
                    out translation, out look, out up);

                parentActor.Transform3D.Translation = translation;
                parentActor.Transform3D.Look = look;
                parentActor.Transform3D.Up = up;
            }
        }

        //add clone...
    }
}

