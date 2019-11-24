/*
Function: 		Creates a class based on the GameComponent class that can be paused when the menu is shown.
Author: 		NMCG
Version:		1.0
Date Updated:	27/10/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class PausableGameComponent : GameComponent
    {
        public PausableGameComponent(Game game, EventDispatcher eventDispatcher, StatusType statusType)
            : base(game)
        {
            //store handle to event dispatcher for event registration and de-registration
            EventDispatcher = eventDispatcher;

            //allows us to start the game component with drawing and/or updating paused
            StatusType = statusType;

            //register with the event dispatcher for the events of interest
            RegisterForEventHandling(eventDispatcher);
        }

        public override void Update(GameTime gameTime)
        {
            if ((StatusType & StatusType.Update) != 0) //if update flag is set
            {
                ApplyUpdate(gameTime);
                base.Update(gameTime);
            }
        }


        protected virtual void ApplyUpdate(GameTime gameTime)
        {
        }

        protected virtual void HandleInput(GameTime gameTime)
        {
        }

        protected virtual void HandleMouse(GameTime gameTime)
        {
        }

        protected virtual void HandleKeyboard(GameTime gameTime)
        {
        }

        protected virtual void HandleGamePad(GameTime gameTime)
        {
        }

        #region Fields

        #endregion

        #region Properties 

        private EventDispatcher EventDispatcher { get; }

        public StatusType StatusType { get; set; }

        #endregion

        #region Event Handling

        protected virtual void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.MenuChanged += EventDispatcher_MenuChanged;
        }

        protected virtual void EventDispatcher_MenuChanged(EventData eventData)
        {
        }

        #endregion
    }
}