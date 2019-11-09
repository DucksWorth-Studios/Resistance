namespace GDLibrary
{
    public class TimerUtility
    {
        #region Fields

        private int hours = 0;
        private int minutes = 0;
        private int seconds = 0;
        private StatusType statusType;

        #endregion

        #region Constructors

        public TimerUtility(int minutes, StatusType statusType)
        {
            this.minutes = minutes;
            this.statusType = statusType;
        }

        public TimerUtility(int hours, int minutes, StatusType statusType)
        {
            this.hours = hours;
            this.minutes = minutes;
            this.statusType = statusType;
        }

        public TimerUtility(int hours, int minutes, int seconds, StatusType statusType)
        {
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
            this.statusType = statusType;
        }

        #endregion

        #region Properties

        public int Hours
        {
            get => hours;
            set => hours = value;
        }

        public int Minutes
        {
            get => minutes;
            set => minutes = value;
        }

        public int Seconds
        {
            get => seconds;
            set => seconds = value;
        }

        public StatusType StatusType
        {
            get => statusType;
            set => statusType = value;
        }

        #endregion

        public override string ToString()
        {
            return Hours + ":" + Minutes + ":" + Seconds;
        }
    }
}