/*
Function: 		Creates a controller which moves the attached object along a user defined 3D curve
Author: 		NMCG
Version:		1.1
Date Updated:	8/10/17
Bugs:			Judder in path
Fixes:			Updated evaluation precision on UpdateTrack() to a default of 4 point decimal precision
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    //used to cause an object (or camera) to move along a predefined track
    public class CurveController : Controller
    {
        private static readonly int DefaultCurveEvaluationPrecision = 4;

        //pre-curveEvaluationPrecision compatability constructor
        public CurveController(string id, ControllerType controllerType,
            Transform3DCurve transform3DCurve, PlayStatusType playStatusType)
            : this(id, controllerType, transform3DCurve, playStatusType, DefaultCurveEvaluationPrecision)
        {
        }

        public CurveController(string id, ControllerType controllerType,
            Transform3DCurve transform3DCurve, PlayStatusType playStatusType,
            int curveEvaluationPrecision)
            : base(id, controllerType)
        {
            Transform3DCurve = transform3DCurve;
            PlayStatusType = playStatusType;
            elapsedTimeInMs = 0;
            CurveEvaluationPrecision = curveEvaluationPrecision;
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            var parentActor = actor as Actor3D;

            if (PlayStatusType == PlayStatusType.Play)
                UpdateTrack(gameTime, parentActor);
            else if (PlayStatusType == PlayStatusType.Reset || PlayStatusType == PlayStatusType.Stop)
                elapsedTimeInMs = 0;
        }

        private void UpdateTrack(GameTime gameTime, Actor3D parentActor)
        {
            if (parentActor != null)
            {
                elapsedTimeInMs += gameTime.ElapsedGameTime.Milliseconds;

                Vector3 translation, look, up;
                Transform3DCurve.Evalulate(elapsedTimeInMs, CurveEvaluationPrecision,
                    out translation, out look, out up);

                parentActor.Transform.Translation = translation;
                parentActor.Transform.Look = look;
                parentActor.Transform.Up = up;
            }
        }

        #region Fields

        private float elapsedTimeInMs;

        #endregion

        #region Properties

        public Transform3DCurve Transform3DCurve { get; set; }

        public PlayStatusType PlayStatusType { get; set; }

        public int CurveEvaluationPrecision { get; set; }

        #endregion

        //add clone...
    }
}