/*
Function: 		Used by Controller to define what action the controller applies to its attached Actor. For example, a FirstPersonCamera has
                an attached controller of type FirstPerson.
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

namespace GDLibrary
{
    public enum ControllerType : sbyte
    {
        Drive, //applied to model
        FirstPerson, //applied to camera
        ThirdPerson, //applied to camera
        Rail,  //applied to camera or model
        Track, //applied to camera or model
        Security, //applied to camera

        //applied to any actor (camera or model)
        Rotation,
        Translation,
        Scale,
        Color,
        LerpRotation,
        LerpTranslation,
        LerpScale,
        LerpColor,
        LerpTexture
    }
}
