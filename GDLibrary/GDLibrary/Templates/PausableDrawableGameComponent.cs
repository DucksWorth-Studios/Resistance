using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class PausableDrawableGameComponent : DrawableGameComponent
    {
        #region Fields
        private StatusType statusType;
        #endregion

        #region Properties 
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
        public PausableDrawableGameComponent(Game game, StatusType statusType)
            : base(game)
        {
            //allows us to start the game component with drawing and/or updating paused
            this.statusType = statusType;
        }

        public override void Update(GameTime gameTime)
        {
            if ((this.statusType & StatusType.Update) != 0) //if update flag is set
            {
                ApplyUpdate(gameTime);
                base.Update(gameTime);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if ((this.statusType & StatusType.Drawn) != 0) //if draw flag is set
            {
                ApplyDraw(gameTime);
                base.Draw(gameTime);
            }
        }

        protected virtual void ApplyUpdate(GameTime gameTime)
        {

        }

        protected virtual void ApplyDraw(GameTime gameTime)
        {

        }
    }
}