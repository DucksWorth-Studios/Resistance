/*
Function: 		Enables support for split screen and overlapping (e.g. rear-view mirror) camera viewports 
Author: 		NMCG
Version:		1.0
Date Updated:	23/8/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class ScreenManager : DrawableGameComponent
    {

        #region Variables
        #endregion

        #region Properties   
        #endregion

        public ScreenManager(Game game)
            : base(game)
        {
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }
}
