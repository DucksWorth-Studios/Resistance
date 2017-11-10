/*
Function: 		Store, update, and draw all visible UI objects based on PausableDrawableGameComponent
Author: 		NMCG
Version:		1.0
Date Updated:	10/11/17
Bugs:			None
Fixes:			None
*/
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GDLibrary
{
    public class UIManager : PausableDrawableGameComponent
    {
        #region Variables
        private bool bMouseVisible;
        private List<Actor2D> drawList, removeList;
        private SpriteBatch spriteBatch;
        #endregion

        #region Properties
        public bool MouseVisible
        {
            get
            {
                return this.Game.IsMouseVisible;
            }
            set
            {
                this.Game.IsMouseVisible = value;
            }
        }
        #endregion

        public UIManager(Game game, SpriteBatch spriteBatch, EventDispatcher eventDispatcher, int initialSize, StatusType statusType)
          : base(game, eventDispatcher, statusType)
        {
            game.IsMouseVisible = bMouseVisible;
            this.spriteBatch = spriteBatch;

            this.drawList = new List<Actor2D>(initialSize);
            //create list to store objects to be removed at start of each update
            this.removeList = new List<Actor2D>(initialSize);
        }

        public void Add(Actor2D actor)
        {
            this.drawList.Add(actor);
        }

        //call when we want to remove a drawn object from the scene
        public void Remove(Actor2D actor)
        {
            this.removeList.Add(actor);
        }

        public int Remove(Predicate<Actor2D> predicate)
        {
            List<Actor2D> resultList = null;

            resultList = this.drawList.FindAll(predicate);
            if ((resultList != null) && (resultList.Count != 0)) //the actor(s) were found in the opaque list
            {
                foreach (Actor2D actor in resultList)
                    this.removeList.Add(actor);
            }

            return resultList != null ? resultList.Count : 0;
        }

        //batch remove on all objects that were requested to be removed
        protected virtual void ApplyRemove()
        {
            foreach (Actor2D actor in this.removeList)
            {
                this.drawList.Remove(actor);
            }

            this.removeList.Clear();
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            //remove any outstanding objects since the last update
            ApplyRemove();

            foreach (Actor2D actor in this.drawList)
            {
                if ((actor.StatusType & StatusType.Update) == StatusType.Update)
                {
                    actor.Update(gameTime);
                }
            }
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            this.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            foreach (Actor2D actor in this.drawList)
            {
                if ((actor.StatusType & StatusType.Drawn) == StatusType.Drawn)
                {
                    actor.Draw(gameTime, spriteBatch);
                }
            }
            this.spriteBatch.End();
        }
    }
}
