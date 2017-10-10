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
using System.Collections.Generic;

namespace GDLibrary
{
    public class Actor2D : Actor
    {
        #region Fields
        private Transform2D transform;
        private List<IController> controllerList;
        #endregion

        #region Properties
        public List<IController> ControllerList
        {
            get
            {
                return this.controllerList;
            }
        }
        public Transform2D Transform
        {
            get
            {
                return this.transform;
            }
            set
            {
                this.transform = value;
            }
        }
        public Matrix World
        {
            get
            {
                return this.transform.World;
            }
        }
        #endregion

        public Actor2D(string id, ActorType actorType, Transform2D transform, StatusType statusType)
            : base(id, actorType, statusType)
        {
            this.transform = transform;
        }

        public override Matrix GetWorldMatrix()
        {
            return this.transform.World;
        }

        public override void AttachController(IController controller)
        {
            if (this.controllerList == null)
                this.controllerList = new List<IController>();

            this.controllerList.Add(controller); //do we allow duplicates?
        }
        public override bool DetachController(string id)
        {
            return false; //to do...
        }
        public bool DetachController(IController controller)
        {
            return false; //to do...
        }

        public override void Update(GameTime gameTime)
        {
            if (this.controllerList != null)
            {
                foreach (IController controller in this.controllerList)
                    controller.Update(gameTime, this); //you control me, update!
            }
            base.Update(gameTime);
        }

        public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {

        }


        public override bool Equals(object obj)
        {
            Actor2D other = obj as Actor2D;

            if (other == null)
                return false;
            else if (this == other)
                return true;

            return this.Transform.Equals(other.Transform) && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hash = 1;
            hash = hash * 31 + this.Transform.GetHashCode();
            hash = hash * 17 + base.GetHashCode();
            return hash;
        }

        public new object Clone()
        {
            IActor actor = new Actor2D("clone - " + ID, //deep
               this.ActorType, //deep
               (Transform2D)this.transform.Clone(), //deep
               this.StatusType); //deep

            //clone each of the (behavioural) controllers
            foreach (IController controller in this.controllerList)
                actor.AttachController((IController)controller.Clone());

            return actor;
        }

        public override bool Remove()
        {
            //tag for garbage collection
            this.transform = null;
            if (this.controllerList != null)
            {
                this.controllerList.Clear();
                this.controllerList = null;
            }

            return true;

            //don't bother returning the base since it doesnt do anything
            //return base.Remove();
        }
    }
}
