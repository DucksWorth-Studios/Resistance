using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class UIProgressController : Controller
    {
        public UIProgressController(string id, ControllerType controllerType, int startValue, int maxValue,
            EventDispatcher eventDispatcher)
            : base(id, controllerType)
        {
            StartValue = startValue;
            MaxValue = maxValue;
            CurrentValue = startValue;

            //register with the event dispatcher for the events of interest
            RegisterForEventHandling(eventDispatcher);
        }

        public override void Update(GameTime gameTime, IActor actor)
        {
            //has the value changed?
            if (bDirty)
            {
                parentUITextureActor = actor as UITextureObject;
                //set the source rectangle according to whatever start value the user supplies
                UpdateSourceRectangle();
                bDirty = false;

                HandleWinLose();
            }

            base.Update(gameTime, actor);
        }

        protected virtual void HandleWinLose()
        {
            //if we lose/win all health then generate an event here that will be handled by SoundManager (play win/lose sound) and other game components.

            if (currentValue == maxValue)
            {
            }
            else if (currentValue == 0)
            {
            }
        }

        protected virtual void UpdateSourceRectangle()
        {
            //how much of a percentage of the width of the image does the current value represent?
            var widthMultiplier = (float) currentValue / maxValue;

            //now set the amount of visible rectangle using the current value
            parentUITextureActor.SourceRectangleWidth
                = (int) (widthMultiplier * parentUITextureActor.OriginalSourceRectangle.Width);
        }

        #region Fields

        private int maxValue, startValue, currentValue;
        private UITextureObject parentUITextureActor;
        private bool bDirty;

        #endregion

        #region Properties

        public int CurrentValue
        {
            get => currentValue;
            set
            {
                currentValue = value >= 0 && value <= maxValue ? value : 0;
                bDirty = true;
            }
        }

        public int MaxValue
        {
            get => maxValue;
            set => maxValue = value >= 0 ? value : 0;
        }

        public int StartValue
        {
            get => startValue;
            set => startValue = value >= 0 ? value : 0;
        }

        #endregion

        #region Event Handling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.PlayerChanged += EventDispatcher_PlayerChanged;
        }

        protected virtual void EventDispatcher_PlayerChanged(EventData eventData)
        {
            //the second value in additionalParameters holds target ID for the event
            var targetID = eventData.AdditionalParameters[0] as string;

            //was this event targeted at me?
            if (targetID.Equals(ID))
                /*
                     * Let's use a switch since its more efficient than "if...else if" and we rarely use switches. Why is it more efficient?
                     * 
                     * Search for "switch vs if else c# jump table"
                     * 
                     */
                switch (eventData.EventType)
                {
                    //delta to health
                    case EventActionType.OnHealthDelta:
                    {
                        //the second value in additionalParameters holds the gain/lose health value
                        CurrentValue = currentValue + (int) eventData.AdditionalParameters[1];
                    }
                        break;

                    //set health
                    case EventActionType.OnHealthSet: //game start events
                    {
                        //the second value in additionalParameters holds the gain/lose health value
                        CurrentValue = (int) eventData.AdditionalParameters[1];
                    }
                        break;
                }
        }

        #endregion
    }
}