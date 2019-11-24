/*
Function: 		Creates a parent target controller which causes the parent actor (to which the controller is attached) to follow a target e.g. ThirdPersonController or RailController
Author: 		NMCG
Version:		1.0
Date Updated:	30/8/17
Bugs:			None
Fixes:			None
*/

namespace GDLibrary
{
    public class TargetController : Controller
    {
        #region Fields

        #endregion

        public TargetController(string id, ControllerType controllerType, IActor targetActor)
            : base(id, controllerType)
        {
            TargetActor = targetActor;
        }

        #region Properties

        public IActor TargetActor { get; set; }

        #endregion

        //Add Equals, Clone, ToString, GetHashCode...
    }
}