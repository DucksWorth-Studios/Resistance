/*
Function: 		Provide GamePad input functions
Author: 		NMCG
Version:		1.0
Date Updated:	24/11/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary
{
    /// <summary>
    ///     Provides methods to determine the state of gamepad buttons and sticks.
    /// </summary>
    public class GamePadManager : GameComponent
    {
        public GamePadManager(Game game, int numberOfConnectedPlayers)
            : base(game)
        {
            NumberOfConnectedPlayers = numberOfConnectedPlayers;
        }

        #region Properties

        public int NumberOfConnectedPlayers
        {
            get => numberOfConnectedPlayers;
            set
            {
                //max number of 4 connected players with an XBox controller
                numberOfConnectedPlayers = value > 0 && value <= 4 ? value : 1;
                //a new and old state for each of the 1-4 controllers
                newState = new GamePadState[numberOfConnectedPlayers];
                oldState = new GamePadState[numberOfConnectedPlayers];
            }
        }

        #endregion

        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }

        public override void Update(GameTime gameTime)
        {
            //store the old states
            for (var i = 0; i < numberOfConnectedPlayers; i++) oldState[i] = newState[i];

            //update the new states
            for (var i = 0; i < numberOfConnectedPlayers; i++) newState[i] = GamePad.GetState(playerIndices[i]);

            base.Update(gameTime);
        }

        //is a specific button pressed on the gamepad for a specific connected player?
        public bool IsButtonPressed(PlayerIndex playerIndex, Buttons button)
        {
            if (IsPlayerConnected(playerIndex))
                return newState[(int) playerIndex].IsButtonDown(button);
            return false;
        }

        //is a specific button pressed now that was not pressed in the last update for a specific connected player?
        public bool IsFirstButtonPress(PlayerIndex playerIndex, Buttons button)
        {
            if (IsPlayerConnected(playerIndex))
                return newState[(int) playerIndex].IsButtonDown(button) &&
                       oldState[(int) playerIndex].IsButtonUp(button);
            return false;
        }

        //has the gamepad state changed since the last update for a specific connected player?
        public bool IsStateChanged(PlayerIndex playerIndex)
        {
            if (IsPlayerConnected(playerIndex))
                return !newState[(int) playerIndex]
                    .Equals(oldState[(int) playerIndex]); //false if no change, otherwise true
            return false;
        }

        //returns the position of the thumbsticks for a specific connected player
        public GamePadThumbSticks GetThumbSticks(PlayerIndex playerIndex)
        {
            if (IsPlayerConnected(playerIndex))
                return newState[(int) playerIndex].ThumbSticks;
            return default(GamePadThumbSticks);
        }

        //returns the state of the triggers (i.e. front of controller) for a specific connected player
        public GamePadTriggers GetTriggers(PlayerIndex playerIndex)
        {
            if (IsPlayerConnected(playerIndex))
                return newState[(int) playerIndex].Triggers;
            return default(GamePadTriggers);
        }

        //returns the state of the DPad (i.e. front of controller) for a specific connected player
        public GamePadDPad GetDPad(PlayerIndex playerIndex)
        {
            if (IsPlayerConnected(playerIndex))
                return newState[(int) playerIndex].DPad;
            return default(GamePadDPad);
        }

        //returns the state of the buttons for a specific connected player
        public GamePadButtons GetButtons(PlayerIndex playerIndex)
        {
            if (IsPlayerConnected(playerIndex))
                return newState[(int) playerIndex].Buttons;
            return default(GamePadButtons);
        }

        //is player index for a controller within 1-4 range and connected?
        public bool IsPlayerConnected(PlayerIndex playerIndex)
        {
            if (newState[(int) playerIndex].IsConnected)
                return true;
            return false;
            //or more aggressively we can throw an exception
            //throw new GamePadException(DebugUtility.GetCurrentMethod(), playerIndex, "not connected");
        }

        #region Fields 

        //xna uses special PlayerIndex variable to refer to controller number not simple 1-4
        private static readonly PlayerIndex[] playerIndices =
            {PlayerIndex.One, PlayerIndex.Two, PlayerIndex.Three, PlayerIndex.Four};

        //similar to keyboard and mouse except we can have as many as 4 states (i.e. 4 connected controllers)
        protected GamePadState[] newState, oldState;

        //how many players
        private int numberOfConnectedPlayers;

        #endregion
    }
}