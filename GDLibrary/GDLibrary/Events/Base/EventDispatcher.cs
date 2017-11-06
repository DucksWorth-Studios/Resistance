/*
Function: 		Represent a message broker for events received and routed through the game engine. 
                Allows the receiver to receive event messages with no reference to the publisher - decouples the sender and receiver.
Author: 		NMCG
Version:		1.0
Date Updated:	11/10/17
Bugs:			None
Fixes:			None
Comments:       Should consider making this class a Singleton because of the static message Stack - See https://msdn.microsoft.com/en-us/library/ff650316.aspx
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class EventDispatcher : GameComponent
    {
        //See Queue doc - https://docs.microsoft.com/en-us/dotnet/api/system.collections.generic.queue-1?view=netframework-4.7.1
        private static Queue<EventData> queue; //stores events in arrival sequence
        private static HashSet<EventData> uniqueSet; //prevents the same event from existing in the stack for a single update cycle (e.g. when playing a sound based on keyboard press)

        //a delegate is basically a list - the list contains a pointer to a function - this function pointer comes from the object wishing to be notified when the event occurs.
        public delegate void CameraEventHandler(EventData eventData);
        public delegate void MenuEventHandler(EventData eventData);
        public delegate void MouseEventHandler(EventData eventData);


        //an event is either null (not yet happened) or non-null - when the event occurs the delegate reads through its list and calls all the listening functions
        public event CameraEventHandler CameraChanged;
        public event MenuEventHandler MenuChanged;
        public event MouseEventHandler MouseChanged;

        public EventDispatcher(Game game, int initialSize)
            : base(game)
        {
            queue = new Queue<EventData>(initialSize);
            uniqueSet = new HashSet<EventData>(new EventDataEqualityComparer());
        }
        public static void Publish(EventData eventData)
        {
            //this prevents the same event being added multiple times within a single update e.g. 10x bell ring sounds
            if (!uniqueSet.Contains(eventData))
            {
                queue.Enqueue(eventData);
                uniqueSet.Add(eventData);
            }
        }
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < queue.Count; i++)
                Process(queue.Dequeue());

            queue.Clear();
            uniqueSet.Clear();

            base.Update(gameTime);
        }

        private void Process(EventData eventData)
        {
            //Switch - See https://msdn.microsoft.com/en-us/library/06tc147t.aspx
            //one case for each category type
            switch (eventData.EventCategoryType)
            {
                case EventCategoryType.Camera:
                    OnCamera(eventData);
                    break;

                case EventCategoryType.MainMenu:
                    OnMenu(eventData);
                    break;

                case EventCategoryType.Mouse:
                    OnMouse(eventData);
                    break;

                //add a case to handle the On...() method for each type

                default:
                    break;
            }
        }

        //called when a mouse centre is requested
        protected virtual void OnMouse(EventData eventData)
        {
            //non-null if an object has subscribed to this event
            MouseChanged?.Invoke(eventData);

            /*
             //Old form:
              if (MouseChanged != null)
                MouseChanged(eventData);
             */
        }

        //called when a menu change is requested
        protected virtual void OnMenu(EventData eventData)
        {
            //non-null if an object has subscribed to this event
            MenuChanged?.Invoke(eventData);
        }

        //called when a camera event needs to be generated
        protected virtual void OnCamera(EventData eventData)
        {
            CameraChanged?.Invoke(eventData);
        }
    }
}
