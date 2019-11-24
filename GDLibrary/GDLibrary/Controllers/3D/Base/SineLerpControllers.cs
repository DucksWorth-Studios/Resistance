/*
Function: 		Base class for all "sine lerp-able" 3D controllers e.g. See ColorLerpController
Author: 		NMCG
Version:		1.0
Date Updated:	24/10/17
Bugs:			None
Fixes:			None
*/

namespace GDLibrary
{
    public class SineLerpController : Controller
    {
        #region Fields

        #endregion

        public SineLerpController(string id, ControllerType controllerType,
            TrigonometricParameters trigonometricParameters)
            : base(id, controllerType)
        {
            TrigonometricParameters = trigonometricParameters;
        }

        #region Properties

        public TrigonometricParameters TrigonometricParameters { get; set; }

        #endregion
    }
}