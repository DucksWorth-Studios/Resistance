/*
Function: 		Used by Actor to help us distunguish one type of actor from another when we perform CD/CR or when we want to enable/disable certain game entities
                e.g. hide all the pickups.
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

namespace GDLibrary
{
    public enum ActorType : sbyte
    {
        Prop, //i.e. an interactable prop related to game narrative or game state e.g. ammo
        Player,
        Decorator, //i.e.  architecture
        Billboard, //i.e. an imposter for a 3D object e.g. distant tree or facade of a building

        Camera,
        Zone, //i.e. invisible and triggers events e.g. walk through a bounding volume and trigger game end or camera change
        Helper, //i.e.. a wireframe visualisation for an entitiy e.g. camera, camera path, bounding box of a pickip

        UITexture, //i.e. a menu texture
        UIText     //i.e. menu text representing game state or menu choice e.g. "Pause"
    }
}
