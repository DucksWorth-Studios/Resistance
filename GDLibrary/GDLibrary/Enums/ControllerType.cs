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
        FirstPerson,
        ThirdPerson,
        Rail,
        Track,
        Security,

        Rotation,
        Translation,
        Scale,
        Color,
        Texture,

        LerpRotation,
        LerpTranslation,
        LerpScale,
        LerpColor,
        LerpTexture,
    }
}
