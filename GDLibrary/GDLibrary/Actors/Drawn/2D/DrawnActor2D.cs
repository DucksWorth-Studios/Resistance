/*
Function: 		Represents the parent class for all updateable AND drawn 2D menu and UI objects. 
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
    public class DrawnActor2D : Actor2D
    {
        public DrawnActor2D(string id, ActorType actorType, Transform2D transform, StatusType statusType,
            Color color, SpriteEffects spriteEffects, float layerDepth)
            : base(id, actorType, transform, statusType)
        {
            Color = color;
            OriginalColor = color;
            SpriteEffects = spriteEffects;
            LayerDepth = layerDepth;
            OriginalLayerDepth = LayerDepth;
            SpriteEffects = spriteEffects;
        }

        public override bool Equals(object obj)
        {
            var other = obj as DrawnActor2D;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return Color.Equals(other.Color) && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + Color.GetHashCode();
            hash = hash * 17 + base.GetHashCode();
            return hash;
        }

        public new object Clone()
        {
            IActor actor = new DrawnActor2D("clone - " + ID, //deep
                ActorType, //deep
                (Transform2D) Transform.Clone(), //deep - calls the clone for Transform3D explicitly
                StatusType, //deep - enum type
                Color, //deep 
                SpriteEffects, //deep - enum type
                LayerDepth); //deep - a simple numeric type

            //clone each of the (behavioural) controllers
            foreach (var controller in ControllerList)
                actor.AttachController((IController) controller.Clone());

            return actor;
        }

        #region Fields

        private float layerDepth;

        #endregion

        #region Properties

        public Color Color { get; set; }

        public Color OriginalColor { get; set; }

        public float LayerDepth
        {
            get => layerDepth;
            set =>
                layerDepth = value >= 0 && value <= 1
                    ? value
                    : 0;
        }

        public float OriginalLayerDepth { get; }

        public SpriteEffects SpriteEffects { get; set; }

        public SpriteEffects OriginalSpriteEffects { get; private set; }

        #endregion
    }
}