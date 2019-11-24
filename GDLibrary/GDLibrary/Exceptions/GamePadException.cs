/*
Function: 		Exception generated if a method is called on an invalid gamepad controller player index (i.e. one which is not connected)
Author: 		NMCG
Version:		1.0
Date Updated:	23/11/17
Bugs:			None
Fixes:			None
*/

using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    [Serializable]
    internal class GamePadException : Exception
    {
        public GamePadException()
        {
        }

        //we could use a method like this to silently log what method created an exception and at what time
        public GamePadException(string currentMethod, PlayerIndex playerIndex, string message)
            : this(" Error[" + currentMethod + "," + playerIndex + "]:" + message)
        {
        }

        public GamePadException(string message) : base(message)
        {
            ShowExceptionMessage(message);
        }

        public GamePadException(string message, Exception innerException) : base(message, innerException)
        {
            ShowExceptionMessage(message);
        }

        protected GamePadException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            //here we could add serialization for error logging...
        }

        private void ShowExceptionMessage(string message)
        {
            var timeNow = DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
            Debug.WriteLine(timeNow + ": " + message);
        }
    }
}