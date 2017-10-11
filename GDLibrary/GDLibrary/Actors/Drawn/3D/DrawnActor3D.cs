/*
Function: 		Represents the parent class for all updateable AND drawn 3D game objects. Notice that Effect has been added.
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework.Graphics;
using System;

namespace GDLibrary
{
    public class DrawnActor3D : Actor3D, ICloneable
    {
        #region Fields
        private Effect effect;
        private ColorParameters colorParameters;
        #endregion

        #region Properties
        public Effect Effect
        {
            get
            {
                return this.effect;
            }
            set
            {
                this.effect = value;
            }
        }
        public ColorParameters ColorParameters
        {
            get
            {
                return this.colorParameters;
            }
            set
            {
                this.colorParameters = value;
            }
        }
        #endregion

        //used when we don't want to specify color and alpha
        public DrawnActor3D(string id, ActorType actorType, Transform3D transform, Effect effect)
            : this(id, actorType, transform, effect, ColorParameters.WhiteOpaque, StatusType.Drawn | StatusType.Update) 
        {
        }

        public DrawnActor3D(string id, ActorType actorType, Transform3D transform, Effect effect, 
            ColorParameters colorParameters, StatusType statusType) : base(id, actorType, transform, statusType)
        {
            this.effect = effect;
            this.colorParameters = colorParameters;
        }

        public override float GetAlpha()
        {
            return this.colorParameters.Alpha;
        }

        public override bool Equals(object obj)
        {
            DrawnActor3D other = obj as DrawnActor3D;

            if (other == null)
                return false;
            else if (this == other)
                return true;

            return this.ColorParameters.Equals(other.ColorParameters) && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hash = 1;
            hash = hash * 31 + this.ColorParameters.GetHashCode();
            hash = hash * 17 + base.GetHashCode();
            return hash;
        }

        public new object Clone()
        {
            IActor actor = new DrawnActor3D("clone - " + ID, //deep
                this.ActorType, //deep
                (Transform3D)this.Transform.Clone(), //deep - calls the clone for Transform3D explicitly
                this.effect, //shallow - its ok if all objects refer to the same effect
                (ColorParameters)this.ColorParameters.Clone(), //deep 
                this.StatusType); //deep - a simple numeric type

            //clone each of the (behavioural) controllers
            foreach (IController controller in this.ControllerList)
                actor.AttachController((IController)controller.Clone());

            return actor;
        }

        public override bool Remove()
        {
            this.colorParameters = null;
            return base.Remove();
        }


    }
}
