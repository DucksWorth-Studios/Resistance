using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class UIObject : DrawnActor2D
    {
        #region Fields

        #endregion

        public UIObject(string id, ActorType actorType, StatusType statusType, Transform2D transform,
            Color color, SpriteEffects spriteEffects, float layerDepth)
            : base(id, actorType, transform, statusType, color, spriteEffects, layerDepth)
        {
            MouseOverState = new StatefulBool(2);
        }

        #region Properties

        public StatefulBool MouseOverState { get; private set; }

        #endregion

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 31 + MouseOverState.GetHashCode();
            hash = hash * 17 + base.GetHashCode();
            return hash;
        }

        public override bool Remove()
        {
            MouseOverState = null;
            return base.Remove();
        }
    }
}