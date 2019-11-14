using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GDLibrary
{
    public class ObjectiveManager : PausableDrawableGameComponent
    {
        #region Fields
        private Game game;
        private List<Actor2D> drawList, removeList;
        private SpriteBatch spriteBatch;

        public ObjectiveManager(Game game, EventDispatcher eventDispatcher, StatusType  statusType,int initialSize,SpriteBatch spriteBatch)
            : base(game, eventDispatcher, statusType)
        {
            this.drawList  = new List<Actor2D>(initialSize);
            this.removeList = new List<Actor2D>(initialSize);
            this.spriteBatch = spriteBatch;
        }







        #endregion



    }
}
