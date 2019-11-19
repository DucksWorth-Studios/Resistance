using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GDLibrary
{
    public class TimerManager : PausableGameComponent, IEnumerable<TimerUtility>
    {
        #region Fields

        private List<TimerUtility> timerList;
        private int lastGameSecond = 0;
        private bool loseEventFired = false;

        #endregion

        #region Constructors

        public TimerManager(TimerUtility timer, Game game, EventDispatcher eventDispatcher, StatusType statusType) : 
            base(game, eventDispatcher, statusType)
        {
            this.timerList = new List<TimerUtility>(0);
            this.timerList.Add(timer);

            //register with the event dispatcher for the events of interest
            RegisterForEventHandling(eventDispatcher);
        }

        public TimerManager(string id, int minutes, Game game, EventDispatcher eventDispatcher, StatusType statusType) :
            base(game, eventDispatcher, statusType)
        {
            this.timerList = new List<TimerUtility>(0);
            this.timerList.Add(new TimerUtility(id, minutes, statusType));

            //register with the event dispatcher for the events of interest
            RegisterForEventHandling(eventDispatcher);
        }

        public TimerManager(string id, int hours, int minutes, Game game, EventDispatcher eventDispatcher, StatusType statusType) :
            base(game, eventDispatcher, statusType)
        {
            this.timerList = new List<TimerUtility>(0);
            this.timerList.Add(new TimerUtility(id, hours, minutes, statusType));

            //register with the event dispatcher for the events of interest
            RegisterForEventHandling(eventDispatcher);
        }

        public TimerManager(string id, int hours, int minutes, int seconds, Game game, EventDispatcher eventDispatcher, StatusType statusType) :
            base(game, eventDispatcher, statusType)
        {
            this.timerList = new List<TimerUtility>(0);
            this.timerList.Add(new TimerUtility(id, hours, minutes, seconds, statusType));

            //register with the event dispatcher for the events of interest
            RegisterForEventHandling(eventDispatcher);
        }

        #endregion

        #region Properties

        public List<TimerUtility> TimerList
        {
            get => timerList;
            set => timerList = value;
        }

        #endregion

        #region EnumeratorProperties

        #region Add

        public void Add(TimerUtility timer)
        {
            timerList.Add(timer);
        }

        public void Add(string id, int minutes, StatusType statusType)
        {
            timerList.Add(new TimerUtility(id, minutes, statusType));
        }

        public void Add(string id, int hours, int minutes, StatusType statusType)
        {
            timerList.Add(new TimerUtility(id, hours, minutes, statusType));
        }

        public void Add(string id, int hours, int minutes, int seconds, StatusType statusType)
        {
            timerList.Add(new TimerUtility(id, hours, minutes, seconds, statusType));
        }

        #endregion

        public bool Remove(Predicate<TimerUtility> predicate)
        {
            TimerUtility foundTimer = this.timerList.Find(predicate);
            if (foundTimer != null)
                return this.timerList.Remove(foundTimer);

            return false;
        }

        public int RemoveAll(Predicate<TimerUtility> predicate)
        {
            return this.timerList.RemoveAll(predicate);

        }

        #region GetEnumerator

        public IEnumerator<TimerUtility> GetEnumerator()
        {
            return this.timerList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #endregion

        #region Event Handeling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.ScreenChanged += EventDispatcher_MenuChanged;
            base.RegisterForEventHandling(eventDispatcher);
        }

        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
            {
                //turn on update for all timers
                foreach (TimerUtility timer in timerList)
                {
                    timer.StatusType = StatusType.Update;
                }
            }
            //did the event come from the main menu and is it a start game event
            else if (eventData.EventType == EventActionType.OnPause)
            {
                //turn off update for all timers
                foreach (TimerUtility timer in timerList)
                {
                    timer.StatusType = StatusType.Off;
                }
            }
        }
        #endregion

        public override void Update(GameTime gameTime)
        {
            if (lastGameSecond < (int) gameTime.TotalGameTime.TotalSeconds)
            {
                lastGameSecond = (int) gameTime.TotalGameTime.TotalSeconds;

                foreach (TimerUtility timer in TimerList)
                {
                    if ((timer.StatusType & StatusType.Update) != 0)
                    {
                        int tempHrs = timer.Hours;
                        int tempMins = timer.Minutes;
                        int tempSecs = timer.Seconds;

                        if (timer.Seconds > 0)
                            timer.Seconds -= 1;
                        else if (timer.Seconds == 0)
                        {
                            if (timer.Minutes > 0)
                            {
                                timer.Minutes -= 1;
                                timer.Seconds = 59;
                            }
                            else if (timer.Minutes == 0)
                            {
                                if (timer.Hours > 0)
                                {
                                    timer.Hours -= 1;
                                    timer.Minutes = 59;
                                    timer.Seconds = 59;
                                }
                                else if (timer.Hours == 0)
                                {
                                    if (timer.ID.Equals("Lose Timer") && !loseEventFired)
                                    {
                                        EventDispatcher.Publish(new EventData(EventActionType.OnLose, EventCategoryType.Player));
                                        loseEventFired = true;
                                    }
                                    else
                                        System.Diagnostics.Debug.WriteLine("Event doesn't exist for this timer " + timer.ID);
                                }
                                else
                                    throw new Exception("Hour check has gone wrong");
                            }
                            else
                                throw new Exception("Minute check has gone wrong");
                        }
                        else
                            throw new Exception("Second check has gone wrong");
                        /*
                        System.Diagnostics.Debug.WriteLine("Old - " + tempHrs + ":" + tempMins + ":" + tempSecs +
                                                           "\tNew - " + timer);
                        */
                    }
                }
            }
        }

    }
}