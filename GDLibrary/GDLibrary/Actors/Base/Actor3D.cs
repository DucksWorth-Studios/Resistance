/*
Function: 		Represents the parent class for all updateable 3D game objects. Notice that Transform3D and List<IController> has been added.
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
    public class Actor3D : Actor, ICloneable
    {
        #region Fields

        #endregion

        public Actor3D(string id, ActorType actorType,
            Transform3D transform, StatusType statusType)
            : base(id, actorType, statusType)
        {
            Transform = transform;
        }

        #region Properties

        public Transform3D Transform { get; set; }

        #endregion

        public new object Clone()
        {
            IActor actor = new Actor3D("clone - " + ID, //deep
                ActorType, //deep
                (Transform3D) Transform.Clone(), //deep
                StatusType); //shallow

            //clone each of the (behavioural) controllers
            foreach (var controller in ControllerList)
                actor.AttachController((IController) controller.Clone());

            return actor;
        }

        //returns the compound matrix transformation that will scale, rotate and place the actor in the 3D world of the game
        public override Matrix GetWorldMatrix()
        {
            return Transform.World;
        }

        public virtual void Draw(GameTime gameTime)
        {
        }

        public override bool Equals(object obj)
        {
            var other = obj as Actor3D;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return Transform.Equals(other.Transform) && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + Transform.GetHashCode();
            hash = hash * 17 + base.GetHashCode();
            return hash;
        }

        public override bool Remove()
        {
            //tag for garbage collection
            Transform = null;
            return base.Remove();
        }
    }
}