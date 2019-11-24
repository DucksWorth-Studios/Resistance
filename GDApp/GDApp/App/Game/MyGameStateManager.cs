/*
Function: 		Use this class to say exactly how your game listens for events and responds with changes to the game.
Author: 		NMCG
Version:		1.0
Date Updated:	17/11/17
Bugs:			
Fixes:			None
*/

using GDLibrary;
using Microsoft.Xna.Framework;

namespace GDApp
{
    public class MyGameStateManager : GameStateManager
    {
        private bool logicPuzzleSolved;
        private bool riddleSolved;
        private bool winBroadcasted;

        public MyGameStateManager(Game game, EventDispatcher eventDispatcher, StatusType statusType)
            : base(game, eventDispatcher, statusType)
        {
            logicPuzzleSolved = false;
            riddleSolved = false;
            winBroadcasted = false;
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            base.ApplyUpdate(gameTime);
        }

        #region Event Handeling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.PlayerChanged += EventDispatcher_PlayerChanged;
            base.RegisterForEventHandling(eventDispatcher);
        }

        private void EventDispatcher_PlayerChanged(EventData eventData)
        {
            if (eventData.EventType == EventActionType.LogicPuzzleSolved)
                logicPuzzleSolved = true;
            else if (eventData.EventType == EventActionType.RiddleSolved)
                riddleSolved = true;

            if (logicPuzzleSolved && riddleSolved && !winBroadcasted)
            {
                EventDispatcher.Publish(new EventData(EventActionType.OnWin, EventCategoryType.Player));
                winBroadcasted = true;
            }
        }

        #endregion
    }
}