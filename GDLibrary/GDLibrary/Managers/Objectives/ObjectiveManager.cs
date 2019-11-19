using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GDLibrary
{
    public class ObjectiveManager : PausableDrawableGameComponent
    {
        #region Fields

        

        private enum Objectives
        {
            noObjective = 0,
            escape = 1,
            solveRiddle = 2,
            solveLogic = 3,
        }

        private Game game;
        private List<Actor2D> drawList, removeList;
        private SpriteBatch spriteBatch;
        private List<Objectives> completedObjectives = new List<Objectives> { };
        private ContentDictionary<Texture2D> textureDictionary;
        private UIManager uIManager;
        private int currentObjective = 1;


       



        #endregion

        public ObjectiveManager(Game game, EventDispatcher eventDispatcher, StatusType  statusType,int initialSize,SpriteBatch spriteBatch, ContentDictionary<Texture2D> textureDictionary,UIManager  uIManager)
            : base(game, eventDispatcher, statusType)
        {
            this.drawList  = new List<Actor2D>(initialSize);
            this.removeList = new List<Actor2D>(initialSize);
            this.spriteBatch = spriteBatch;
            this.textureDictionary = textureDictionary;
            this.uIManager = uIManager;
            this.InitializeObjectivesUI();

        }


        public int getCurrentObjective()
        {
            return this.currentObjective;
        }


        #region ManagerMethods
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
        #endregion

        #region register for events

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.ObjectiveChanged +=newObjective;
            eventDispatcher.ObjectiveChanged +=EventDispatcher_MenuChanged;
        }

        #endregion
        public Texture2D InitializeObjectivesUI()
        {
           Texture2D texture = textureDictionary["Objective"];


            if ((Objectives)this.currentObjective == Objectives.escape) { texture = textureDictionary["Escape"]; }
            if ((Objectives)this.currentObjective == Objectives.solveRiddle) { texture = textureDictionary["Riddle"]; }
            if ((Objectives)this.currentObjective == Objectives.solveLogic) { texture = textureDictionary["Logic"]; }



            return texture;
        }

        public void setObjectivesUI()
        {
            Predicate<Actor2D> pred = s => s.ActorType == ActorType.Objective;
            UITextureObject objective = this.uIManager.Find(pred) as UITextureObject;
            int newWidth;


            if ((Objectives)this.currentObjective == Objectives.escape)
            {
                objective.Texture = textureDictionary["Escape"];

                newWidth = (int)Math.Round(objective.SourceRectangle.Width /1.5);

                objective.Transform.Translation = new Vector2(objective.Transform.Translation.X + newWidth / 3.5f, objective.Transform.Translation.Y);

                objective.SourceRectangle = new Rectangle
                    (objective.SourceRectangle.X, objective.SourceRectangle.Y, newWidth, objective.SourceRectangle.Height);

            }
            if ((Objectives)this.currentObjective == Objectives.solveRiddle)
            {
                objective.Texture = textureDictionary["Riddle"];
          
                newWidth = (int)Math.Round(objective.SourceRectangle.Width * 1.5);

                objective.Transform.Translation = new Vector2(objective.Transform.Translation.X - newWidth/6, objective.Transform.Translation.Y);

                objective.SourceRectangle = new Rectangle
                    (objective.SourceRectangle.X, objective.SourceRectangle.Y, newWidth, objective.SourceRectangle.Height);
            }
            if ((Objectives)this.currentObjective == Objectives.solveLogic)
            {
                objective.Texture = textureDictionary["Logic"];
           

                objective.Transform.Translation = new Vector2(objective.Transform.Translation.X, objective.Transform.Translation.Y);

                objective.SourceRectangle = new Rectangle
                    (objective.SourceRectangle.X, objective.SourceRectangle.Y, objective.SourceRectangle.Width, objective.SourceRectangle.Height);

            }
 
        }

        protected void newObjective(EventData eventData)
        {
            System.Diagnostics.Debug.WriteLine("CurrentObj " + currentObjective);
                if (currentObjective >= 3)
                {
                    completedObjectives.Add((Objectives)currentObjective);
                    currentObjective = 1;
                    setObjectivesUI();
                }
                else
                {

                    completedObjectives.Add((Objectives)currentObjective);
                    currentObjective++;
                    setObjectivesUI();

                }

            System.Diagnostics.Debug.WriteLine("CurrentObj " + currentObjective);

        }

        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
            {
                this.StatusType = StatusType.Drawn;
            }
            //did the event come from the main menu and is it a start game event
            else if (eventData.EventType == EventActionType.OnPause)
            {
                this.StatusType = StatusType.Off;

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
 



    }
}
