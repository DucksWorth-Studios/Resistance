/*
Function: 		Class to hold values for a timer
Author: 		Cameron
*/

namespace GDLibrary
{
    public class TimerUtility
    {
        public override string ToString()
        {
            return Hours + ":" + Minutes + ":" + Seconds;
        }

        #region Fields

        #endregion

        #region Constructors

        public TimerUtility(string id, int minutes, StatusType statusType)
        {
            ID = id;
            Minutes = minutes;
            StatusType = statusType;
        }

        public TimerUtility(string id, int hours, int minutes, StatusType statusType)
        {
            ID = id;
            Hours = hours;
            Minutes = minutes;
            StatusType = statusType;
        }

        public TimerUtility(string id, int hours, int minutes, int seconds, StatusType statusType)
        {
            ID = id;
            Hours = hours;
            Minutes = minutes;
            Seconds = seconds;
            StatusType = statusType;
        }

        #endregion

        #region Properties

        public string ID { get; set; }

        public int Hours { get; set; }

        public int Minutes { get; set; }

        public int Seconds { get; set; }

        public StatusType StatusType { get; set; }

        #endregion
    }
}