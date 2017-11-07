/*
Function: 		Encapsulates the event data sent by the sender to the dispatcher. This data will be interpreted and acted upon by the registered recipients.
Author: 		NMCG
Version:		1.1
Date Updated:	7/11/17
Bugs:			None
Fixes:			None
*/

namespace GDLibrary
{
    public class EventData
    {
        #region Fields
        private EventActionType eventType;
        private EventCategoryType eventCategoryType;
        private object sender;
        private string id;
        //an optional array to pass multiple parameters within an event (used for camera, sound, or video based events)
        private object[] eventParameters;
        #endregion

        #region Properties
        public string ID
        {
            get
            {
                return this.id;
            }
            set
            {
                this.id = value;
            }
        }
        public object Sender
        {
            get
            {
                return this.sender;
            }
            set
            {
                this.sender = value;
            }
        }
        public object[] EventParameters
        {
            get
            {
                return this.eventParameters;
            }
            set
            {
                this.eventParameters = value;
            }
        }
        public int ParameterCount
        {
            get
            {
                return this.eventParameters.Length;
            }
        }

        public EventActionType EventType
        {
            get
            {
                return this.eventType;
            }
            set
            {
                this.eventType = value;
            }
        }
        public EventCategoryType EventCategoryType
        {
            get
            {
                return this.eventCategoryType;
            }
            set
            {
                this.eventCategoryType = value;
            }
        }
        #endregion

        //pre-object[] compatability constructor
        public EventData(string id, object sender, EventActionType eventType, EventCategoryType eventCategoryType)
            : this(id, sender, null, eventType, eventCategoryType)
        {

        }

        //supports passing of multiple parameter objects within an event
        public EventData(string id, object sender, object[] eventParameters, EventActionType eventType, EventCategoryType eventCategoryType)
        {
            this.id = id;                           //id of sender
            this.sender = sender;                   //object reference of sender
            this.eventParameters = eventParameters;
            this.eventType = eventType;             //is it play, mute, volume, zone?   
            this.eventCategoryType = eventCategoryType; //where did it originate? ui, menu, video
        }

        public object Clone() //deep copy
        {
            return this.MemberwiseClone(); //all primitive types or structs so use MemberwiseClone();
        }


        public override bool Equals(object obj)
        {
            EventData other = obj as EventData;
            return this.id.Equals(other) 
                && this.sender == other.Sender 
                && ((this.eventParameters != null && this.eventParameters.Length != 0) ? this.eventParameters.Equals(other.EventParameters) : true)
                && this.eventType == other.EventType 
                && this.eventCategoryType == other.EventCategoryType;
        }

        public override int GetHashCode()
        {
            int hash = 1;
            hash = hash * 7 + this.id.GetHashCode();
            hash = hash * 11 + this.sender.GetHashCode();
            if(this.eventParameters != null && this.eventParameters.Length != 0)
                hash = hash * 31 + this.eventParameters.GetHashCode();
            hash = hash * 47 + this.eventType.GetHashCode();
            hash = hash * 79 + this.eventCategoryType.GetHashCode();
            return hash;
        }
    }
}
