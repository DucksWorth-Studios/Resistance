/*
Function: 		Represents the parent class for all updateable 2D menu and UI objects. Notice that Transform2D has been added.
Author: 		NMCG
Version:		1.0
Date Updated:	27/9/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class Actor2D : Actor
    {
        #region Fields

        #endregion

        public Actor2D(string id, ActorType actorType, Transform2D transform, StatusType statusType)
            : base(id, actorType, statusType)
        {
            Transform = transform;
        }

        public override Matrix GetWorldMatrix()
        {
            return Transform.World;
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
        }

        public override bool Equals(object obj)
        {
            var other = obj as Actor2D;

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

        public new object Clone()
        {
            IActor actor = new Actor2D("clone - " + ID, //deep
                ActorType, //deep
                (Transform2D) Transform.Clone(), //deep
                StatusType); //deep

            //clone each of the (behavioural) controllers
            foreach (var controller in ControllerList)
                actor.AttachController((IController) controller.Clone());

            return actor;
        }

        public override bool Remove()
        {
            //tag for garbage collection
            Transform = null;
            return base.Remove();
        }

        #region Properties

        public Transform2D Transform { get; set; }

        public Matrix World => Transform.World;

        #endregion
    }
}