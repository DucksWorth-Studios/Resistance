namespace GDLibrary
{
    public class TimerUtility
    {
        #region Fields

        private int hours = 0;
        private int minutes = 0;
        private int seconds = 0;

        #endregion

        #region Constructors

        public TimerUtility(int minutes)
        {
            this.minutes = minutes;
        }

        public TimerUtility(int hours, int minutes)
        {
            this.hours = hours;
            this.minutes = minutes;
        }

        public TimerUtility(int hours, int minutes, int seconds)
        {
            this.hours = hours;
            this.minutes = minutes;
            this.seconds = seconds;
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

        #endregion
    }
}