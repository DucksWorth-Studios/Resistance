/*
Function: 		Represents the parent interface for all game objects. This interface should really only have an Update() method since a Camera doesn't need a Draw() - this is just a concession to simplify the hierarchy.
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

//base class from which all drawn, collidable, 
//non-collidable, trigger volumes, and camera inherit
namespace GDLibrary
{
    public interface IActor
    {
        void Update(GameTime gameTime);
        void AttachController(IController controller);
        bool DetachController(string id);
        ActorType GetActorType();
        StatusType GetStatusType();
        string GetID();
        bool Remove();
        float GetAlpha();
    }
}
