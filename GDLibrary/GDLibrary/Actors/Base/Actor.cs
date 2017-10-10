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
        #region Statics
        public static Game game;
        #endregion

        #region Fields
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
        public virtual StatusType GetStatusType()
        {
            return this.statusType;
        }

        public override bool Equals(object obj)
        {
            Actor other = obj as Actor;

            if (other == null)
                return false;
            else if (this == other)
                return true;

            return this.ID.Equals(other.ID)
                && this.actorType == other.ActorType
                    && this.statusType.Equals(other.StatusType);
        }

        public override int GetHashCode()
        {
            int hash = 1;
            hash = hash * 31 + this.ID.GetHashCode();
            hash = hash * 17 + this.actorType.GetHashCode();
            hash = hash * 13 + this.statusType.GetHashCode();
            return hash;
        }

        public object Clone()
        {
            //deep because all variables are C# types (e.g. primitives, structs, or enums)
            return this.MemberwiseClone();
        }

        public virtual bool Remove()
        {
            return true; //see implementation in child classes e.g. ModelObject
        }

        public virtual void AttachController(IController controller)
        {
            //does nothing see derived classes e.g. Actor2D, Actor3D
        }

        public virtual bool DetachController(string id)
        {
            //does nothing see derived classes e.g. Actor2D, Actor3D
            return true;
        }
    }
}
