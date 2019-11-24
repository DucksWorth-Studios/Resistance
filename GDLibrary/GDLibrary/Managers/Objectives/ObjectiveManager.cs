using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class ObjectiveManager : PausableDrawableGameComponent
    {
        public ObjectiveManager(Game game, EventDispatcher eventDispatcher, StatusType statusType, int initialSize,
            SpriteBatch spriteBatch, ContentDictionary<Texture2D> textureDictionary, UIManager uIManager)
            : base(game, eventDispatcher, statusType)
        {
            drawList = new List<Actor2D>(initialSize);
            removeList = new List<Actor2D>(initialSize);
            this.spriteBatch = spriteBatch;
            this.textureDictionary = textureDictionary;
            this.uIManager = uIManager;
            InitializeObjectivesUI();
        }


        public int getCurrentObjective()
        {
            return currentObjective;
        }

        #region register for events

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.ObjectiveChanged += newObjective;
            eventDispatcher.ObjectiveChanged += EventDispatcher_MenuChanged;
        }

        #endregion

        public Texture2D InitializeObjectivesUI()
        {
            var texture = textureDictionary["Objective"];


            if ((Objectives) currentObjective == Objectives.escape) texture = textureDictionary["Escape"];
            if ((Objectives) currentObjective == Objectives.solveRiddle) texture = textureDictionary["Riddle"];
            if ((Objectives) currentObjective == Objectives.solveLogic) texture = textureDictionary["Logic"];


            return texture;
        }

        public void setObjectivesUI()
        {
            Predicate<Actor2D> pred = s => s.ActorType == ActorType.Objective;
            var objective = uIManager.Find(pred) as UITextureObject;
            int newWidth;


            if ((Objectives) currentObjective == Objectives.escape)
            {
                objective.Texture = textureDictionary["Escape"];

                newWidth = (int) Math.Round(objective.SourceRectangle.Width / 1.5);

                objective.Transform.Translation = new Vector2(objective.Transform.Translation.X + newWidth / 3.5f,
                    objective.Transform.Translation.Y);

                objective.SourceRectangle = new Rectangle
                (objective.SourceRectangle.X, objective.SourceRectangle.Y, newWidth,
                    objective.SourceRectangle.Height);
            }

            if ((Objectives) currentObjective == Objectives.solveRiddle)
            {
                objective.Texture = textureDictionary["Riddle"];

                newWidth = (int) Math.Round(objective.SourceRectangle.Width * 1.5);

                objective.Transform.Translation = new Vector2(objective.Transform.Translation.X - newWidth / 6,
                    objective.Transform.Translation.Y);

                objective.SourceRectangle = new Rectangle
                (objective.SourceRectangle.X, objective.SourceRectangle.Y, newWidth,
                    objective.SourceRectangle.Height);
            }

            if ((Objectives) currentObjective == Objectives.solveLogic)
            {
                objective.Texture = textureDictionary["Logic"];


                objective.Transform.Translation =
                    new Vector2(objective.Transform.Translation.X, objective.Transform.Translation.Y);

                objective.SourceRectangle = new Rectangle
                (objective.SourceRectangle.X, objective.SourceRectangle.Y, objective.SourceRectangle.Width,
                    objective.SourceRectangle.Height);
            }
        }

        protected void newObjective(EventData eventData)
        {
            Debug.WriteLine("CurrentObj " + currentObjective);
            if (currentObjective >= 3)
            {
                completedObjectives.Add((Objectives) currentObjective);
                currentObjective = 1;
                setObjectivesUI();
            }
            else
            {
                completedObjectives.Add((Objectives) currentObjective);
                currentObjective++;
                setObjectivesUI();
            }

            Debug.WriteLine("CurrentObj " + currentObjective);
        }

        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
                StatusType = StatusType.Drawn;
            //did the event come from the main menu and is it a start game event
            else if (eventData.EventType == EventActionType.OnPause) StatusType = StatusType.Off;
        }


        protected override void ApplyUpdate(GameTime gameTime)
        {
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

        #region Fields

        private enum Objectives
        {
            noObjective = 0,
            escape = 1,
            solveRiddle = 2,
            solveLogic = 3
        }

        private Game game;
        private readonly List<Actor2D> drawList;
        private readonly List<Actor2D> removeList;
        private readonly SpriteBatch spriteBatch;
        private readonly List<Objectives> completedObjectives = new List<Objectives>();
        private readonly ContentDictionary<Texture2D> textureDictionary;
        private readonly UIManager uIManager;
        private int currentObjective = 1;

        #endregion


        #region ManagerMethods

        public void Add(Actor2D actor)
        {
            drawList.Add(actor);
        }

        public void Remove(Actor2D actor)
        {
            removeList.Add(actor);
        }

        public int Remove(Predicate<Actor2D> predicate)
        {
            List<Actor2D> resultList = null;

            resultList = drawList.FindAll(predicate);
            if (resultList != null && resultList.Count != 0)
                foreach (var actor in resultList)
                    removeList.Add(actor);

            return resultList != null ? resultList.Count : 0;
        }

        public Actor2D Find(Predicate<Actor2D> predicate)
        {
            return drawList.Find(predicate);
        }

        protected virtual void ApplyRemove()
        {
            foreach (var actor in removeList) drawList.Remove(actor);

            removeList.Clear();
        }

        #endregion
    }
}