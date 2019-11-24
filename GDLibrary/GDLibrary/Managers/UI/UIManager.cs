﻿/*
Function: 		Store, update, and draw all visible UI objects based on PausableDrawableGameComponent
Author: 		NMCG
Version:		1.0
Date Updated:	10/11/17
Bugs:			None
Fixes:			None
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class UIManager : PausableDrawableGameComponent
    {
        public UIManager(Game game, SpriteBatch spriteBatch, EventDispatcher eventDispatcher, int initialSize,
            StatusType statusType)
            : base(game, eventDispatcher, statusType)
        {
            this.spriteBatch = spriteBatch;

            drawList = new List<Actor2D>(initialSize);
            //create list to store objects to be removed at start of each update
            removeList = new List<Actor2D>(initialSize);
        }

        //See MenuManager::EventDispatcher_MenuChanged to see how it does the reverse i.e. they are mutually exclusive
        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
                //turn on update and draw i.e. hide the menu
                StatusType = StatusType.Update | StatusType.Drawn;
            //did the event come from the main menu and is it a start game event
            else if (eventData.EventType == EventActionType.OnPause)
                //turn off update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Off;
            else if (eventData.EventType == EventActionType.OnLose)
                //turn off update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Off;
            else if (eventData.EventType == EventActionType.OnWin) StatusType = StatusType.Off;
        }

        public void Add(Actor2D actor)
        {
            drawList.Add(actor);
        }

        //call when we want to remove a drawn object from the scene
        public void Remove(Actor2D actor)
        {
            removeList.Add(actor);
        }

        public int Remove(Predicate<Actor2D> predicate)
        {
            List<Actor2D> resultList = null;

            resultList = drawList.FindAll(predicate);
            if (resultList != null && resultList.Count != 0) //the actor(s) were found in the opaque list
                foreach (var actor in resultList)
                    removeList.Add(actor);

            return resultList != null ? resultList.Count : 0;
        }

        public Actor2D Find(Predicate<Actor2D> predicate)
        {
            return drawList.Find(predicate);
        }

        //to do as an exercise...FindAll(Predicate<Actor2D> predicate)

        //batch remove on all objects that were requested to be removed
        protected virtual void ApplyRemove()
        {
            foreach (var actor in removeList) drawList.Remove(actor);

            removeList.Clear();
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            //remove any outstanding objects since the last update
            ApplyRemove();

            foreach (var actor in drawList)
                if ((actor.StatusType & StatusType.Update) == StatusType.Update)
                    actor.Update(gameTime);
        }

        protected override void ApplyDraw(GameTime gameTime)
        {
            spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
            foreach (var actor in drawList)
                if ((actor.StatusType & StatusType.Drawn) == StatusType.Drawn)
                    actor.Draw(gameTime, spriteBatch);
            spriteBatch.End();
        }

        #region Variables

        private readonly List<Actor2D> drawList;
        private readonly List<Actor2D> removeList;
        private readonly SpriteBatch spriteBatch;

        #endregion

        #region Properties

        #endregion
    }
}