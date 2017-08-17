/*
Function: 		Represents the parent class for all updateable AND drawn 3D game objects. Notice that Effect has been added.
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GDLibrary
{
    public class DrawnActor3D : Actor3D, ICloneable
    {
        #region Variables
        private Effect effect;
        private Color color;
        private float alpha;
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
        public Color Color
        {
            get
            {
                return this.color;
            }
            set
            {
                this.color = value;
            }
        }
        public float Alpha
        {
            get
            {
                return this.alpha;
            }
            set
            {
                value = (value >= 0 && value <= 1) ? value : 1; //bounds check on value

                //assign the new alpha
                this.alpha = value;
            }
        }
        #endregion

        //used when we don't want to specify color and alpha
        public DrawnActor3D(string id, ActorType actorType,
         Transform3D transform, Effect effect)
            : this(id, actorType, transform, effect, 
            Color.White, 1, 
            StatusType.Drawn | StatusType.Update) // when we use a bitwise OR we are saying "drawn AND updated"
        {
        }

        public DrawnActor3D(string id, ActorType actorType,
            Transform3D transform, Effect effect, Color color, 
            float alpha, StatusType statusType)
            : base(id, actorType, transform, statusType)
        {
            this.effect = effect;
            this.color = color;
            this.alpha = alpha;
        }

        public override float GetAlpha()
        {
            return this.alpha;
        }
        public new object Clone()
        {
            return new DrawnActor3D("clone - " + ID, //deep
                this.ActorType, //deep
                (Transform3D)this.Transform3D.Clone(), //deep - calls the clone for Transform3D explicitly
                this.effect, //shallow - its ok if objects refer to the same effect
                this.color, //deep  - a simple numeric type
                this.alpha, //deep  - a simple numeric type
                StatusType.Drawn | StatusType.Update); //deep - a simple numeric type
        }       
    }
}
