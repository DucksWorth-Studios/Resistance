/*
Function: 		Provide GamePad input functions
Author: 		NMCG
Version:		1.0
Date Updated:	24/11/17
Bugs:			None
Fixes:			None
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GDLibrary
{
    /// <summary>
    /// Provides methods to determine the state of gamepad buttons and sticks.
    /// </summary>
    public class GamePadManager : GameComponent
    {
        #region Fields 
        //xna uses special PlayerIndex variable to refer to controller number not simple 1-4
        private static readonly PlayerIndex[] playerIndices = { PlayerIndex.One, PlayerIndex.Two, PlayerIndex.Three, PlayerIndex.Four };
        //similar to keyboard and mouse except we can have as many as 4 states (i.e. 4 connected controllers)
        protected GamePadState[] newState, oldState;
        //how many players
        private int numberOfConnectedPlayers;
        #endregion

        #region Properties
        public int NumberOfConnectedPlayers
        {
            get
            {
                return this.numberOfConnectedPlayers;
            }
            set
            {
                //max number of 4 connected players with an XBox controller
                this.numberOfConnectedPlayers = (value > 0 && value <= 4) ? value : 1;
            }
        }
        #endregion

        public GamePadManager(Game game, int numberOfConnectedPlayers)
            : base(game)
        {
            this.NumberOfConnectedPlayers = numberOfConnectedPlayers;
            //a new and old state for each of the 1-4 controllers
            this.newState = new GamePadState[this.NumberOfConnectedPlayers];
            this.oldState = new GamePadState[this.NumberOfConnectedPlayers];
        }
  
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
        }
        
        public override void Update(GameTime gameTime)
        {
            //store the old states
            for (int i = 0; i < this.numberOfConnectedPlayers; i++)
            {
                this.oldState[i] = this.newState[i];
            }

            //update the new states
            for (int i = 0; i < this.numberOfConnectedPlayers; i++)
            {
                this.newState[i] = GamePad.GetState(playerIndices[i]);
            }

            base.Update(gameTime);
        }

        //is a specific button pressed on the gamepad for a specific connected player?
        public bool IsButtonPressed(int playerIndexAsInt, Buttons button)
        {
            if (IsPlayerIndexValidAndConnected(playerIndexAsInt))
                return this.newState[playerIndexAsInt].IsButtonDown(button);
            else
                return false;
        }

        //is a specific button pressed now that was not pressed in the last update for a specific connected player?
        public bool IsFirstButtonPress(int playerIndexAsInt, Buttons button)
        {
            if (IsPlayerIndexValidAndConnected(playerIndexAsInt))
                return this.newState[playerIndexAsInt].IsButtonDown(button) && this.oldState[playerIndexAsInt].IsButtonUp(button);
            else
                return false;
        }

        //has the gamepad state changed since the last update for a specific connected player?
        public bool IsStateChanged(int playerIndexAsInt)
        {
            return !this.newState[playerIndexAsInt].Equals(oldState[playerIndexAsInt]); //false if no change, otherwise true
        }

        //returns the position of the thumbsticks for a specific connected player
        public GamePadThumbSticks GetThumbSticks(int playerIndexAsInt)
        {
            GamePadThumbSticks gamePadThumbSticks = new GamePadThumbSticks();

            if (IsPlayerIndexValidAndConnected(playerIndexAsInt))
                gamePadThumbSticks = this.newState[playerIndexAsInt].ThumbSticks;
            
            return gamePadThumbSticks;
        }

        //returns the state of the triggers (i.e. front of controller) for a specific connected player
        public GamePadTriggers GetTriggers(int playerIndexAsInt)
        {
            GamePadTriggers gamePadTriggers = new GamePadTriggers();

            if (IsPlayerIndexValidAndConnected(playerIndexAsInt))
                gamePadTriggers = this.newState[playerIndexAsInt].Triggers;

            return gamePadTriggers;
        }

        //returns the state of the DPad (i.e. front of controller) for a specific connected player
        public GamePadDPad GetDPad(int playerIndexAsInt)
        {
            GamePadDPad gamePadDPad = new GamePadDPad();

            if (IsPlayerIndexValidAndConnected(playerIndexAsInt))
                gamePadDPad = this.newState[playerIndexAsInt].DPad;

            return gamePadDPad;
        }

        //is player index for a controller within 1-4 range and connected?
        private bool IsPlayerIndexValidAndConnected(int playerIndexAsInt)
        {
            if (playerIndexAsInt > 0 && playerIndexAsInt < this.numberOfConnectedPlayers && this.newState[playerIndexAsInt].IsConnected) 
                return true;
            else
                throw new GamePadException("Player index " + playerIndexAsInt + " is not connected");
        }
    }
}
