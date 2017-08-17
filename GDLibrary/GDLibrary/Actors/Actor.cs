/*
Function: 		Represents the parent class for all updateable 3D AND 2D game objects (e.g. camera,pickup, player, menu text). Notice that this class doesn't 
                have any positional information (i.e. a Transform3D or Transform2D). This will allow us to use Actor as the parent for both 3D and 2D game objects (e.g. a player or a string of menu text).
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

using System;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class Actor : IActor, ICloneable
    {
        #region Variables
        public static Main game;

        private string id;
        private ActorType actorType;
        private StatusType statusType;
        #endregion

        #region Properties
        public ActorType ActorType
        {
            get
            {
                return this.actorType;
            }
            set
            {
                this.actorType = value;
            }
        }
        public string ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }
        public StatusType StatusType
        {
            get
            {
                return this.statusType;
            }
            set
            {
                this.statusType = value;
            }
        }
        #endregion
        
        public Actor(string id, ActorType actorType, StatusType statusType)
        {
            this.id = id;
            this.actorType = actorType;
            this.statusType = statusType;
        }
        public virtual void Update(GameTime gameTime)
        {           
        }
        public virtual void Draw(GameTime gameTime)
        {       
        }

        public virtual Matrix GetWorldMatrix()
        {
            return Matrix.Identity; //does nothing - see derived classes especially CollidableObject
        }
        public virtual ActorType GetActorType()
        {
            return this.actorType;
        }
        public virtual string GetID()
        {
            return this.id;
        }
        public virtual float GetAlpha()
        {
            return 1;
        }
        public object Clone()
        {
            return this.MemberwiseClone(); //deep because all variables are either C# types, structs, or enums
        }

        public virtual bool Remove()
        {
            return false; //see implementation in child classes e.g. ModelObject
        }
    }
}
