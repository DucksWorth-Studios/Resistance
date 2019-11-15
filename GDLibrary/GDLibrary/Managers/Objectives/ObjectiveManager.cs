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

        public void Add(Actor2D actor)
        {
            this.drawList.Add(actor);
        }

        public void Remove(Actor2D actor)
        {
            this.removeList.Add(actor);
        }

        public int Remove(Predicate<Actor2D> predicate)
        {
            List<Actor2D> resultList = null;

            resultList = this.drawList.FindAll(predicate);
            if ((resultList != null) && (resultList.Count != 0)) 
            {
                foreach (Actor2D actor in resultList)
                    this.removeList.Add(actor);
            }

            return resultList != null ? resultList.Count : 0;
        }
        public Actor2D Find(Predicate<Actor2D> predicate)
        {
            return drawList.Find(predicate);
        }

        protected virtual void ApplyRemove()
        {
            foreach (Actor2D actor in this.removeList)
            {
                this.drawList.Remove(actor);
            }

            this.removeList.Clear();
        }

        #region register for events

        protected void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.ObjectiveChanged +=updateObjective;
        }

        #endregion


        protected void updateObjective(EventData eventData)
        {

        }

        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
            {
               
            }
            //did the event come from the main menu and is it a start game event
            else if (eventData.EventType == EventActionType.OnPause)
            {
              
            }
        }


        protected override void ApplyUpdate(GameTime gameTime)
        {
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
        #endregion



    }
}
