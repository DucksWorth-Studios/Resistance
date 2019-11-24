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
        //when we don't have any pertinent string data in ID AND sender
        public EventData(EventActionType eventType, EventCategoryType eventCategoryType)
            : this(null, null, eventType, eventCategoryType, null)
        {
        }

        //when we don't have any pertinent string data in ID AND sender but have additional event parameters
        public EventData(EventActionType eventType, EventCategoryType eventCategoryType, object[] additionalParameters)
            : this(null, null, eventType, eventCategoryType, additionalParameters)
        {
        }

        //when we don't have any pertinent string data in ID
        public EventData(object sender, EventActionType eventType, EventCategoryType eventCategoryType)
            : this(null, sender, eventType, eventCategoryType, null)
        {
        }

        //pre-object[] compatability constructor
        public EventData(string id, object sender, EventActionType eventType, EventCategoryType eventCategoryType)
            : this(id, sender, eventType, eventCategoryType, null)
        {
        }

        //supports passing of multiple parameter objects within an event
        public EventData(string id, object sender, EventActionType eventType, EventCategoryType eventCategoryType,
            object[] additionalParameters)
        {
            ID = id; //id of sender
            Sender = sender; //object reference of sender
            EventType = eventType; //is it play, mute, volume, zone?   
            EventCategoryType = eventCategoryType; //where did it originate? ui, menu, video

            //used to pass extra information between sender and registered recipient(s)
            AdditionalParameters = additionalParameters;
        }

        public object Clone() //deep copy
        {
            return MemberwiseClone(); //all primitive types or structs so use MemberwiseClone();
        }


        public override bool Equals(object obj)
        {
            var other = obj as EventData;
            var bEquals = true;

            //sometimes we don't specify ID or sender so run a test
            if (ID != null)
                bEquals = bEquals && ID.Equals(other.ID);

            if (Sender != null)
                bEquals = bEquals && Sender.Equals(other.Sender);

            return bEquals && (AdditionalParameters != null && AdditionalParameters.Length != 0
                               ? AdditionalParameters.Equals(other.AdditionalParameters)
                               : true)
                           && EventType == other.EventType
                           && EventCategoryType == other.EventCategoryType;
        }

        public override int GetHashCode()
        {
            var hash = 1;
            if (ID != null)
                hash = hash * 7 + ID.GetHashCode();
            if (Sender != null)
                hash = hash * 11 + Sender.GetHashCode();

            if (AdditionalParameters != null && AdditionalParameters.Length != 0)
                hash = hash * 31 + AdditionalParameters.GetHashCode();

            hash = hash * 47 + EventType.GetHashCode();
            hash = hash * 79 + EventCategoryType.GetHashCode();
            return hash;
        }

        #region Fields

        //an optional array to pass multiple parameters within an event (used for camera, sound, or video based events)

        #endregion

        #region Properties

        public string ID { get; set; }

        public object Sender { get; set; }

        public object[] AdditionalParameters { get; set; }

        public EventActionType EventType { get; set; }

        public EventCategoryType EventCategoryType { get; set; }

        #endregion
    }
}