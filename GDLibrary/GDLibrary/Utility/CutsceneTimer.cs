using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GDLibrary
{
    public class CutsceneTimer : GameComponent
    {
        private string id;
        private string cameraToChangeTo;
        private int currentTime;
        private int secondsToWait;
        private int timeToDeploy;
        

        public CutsceneTimer(string id,EventDispatcher eventDispatcher ,Game game) : base(game)
        {
            this.id = id;
            this.timeToDeploy = -1;
            

            RegesterForEvent(eventDispatcher);
        }

        private void RegesterForEvent(EventDispatcher eventDispatcher)
        {
            eventDispatcher.cutsceneChanged += timeFunction;
        }

        public void timeFunction(EventData eventData)
        {
            Console.WriteLine("Event Date " + eventData.ToString());
            this.secondsToWait = (int) eventData.AdditionalParameters[0];
            
            this.cameraToChangeTo = eventData.AdditionalParameters[1] as string;

            this.timeToDeploy = currentTime + secondsToWait;
            Console.WriteLine("Time to Wait To " + this.timeToDeploy);
        }

        public override void Update(GameTime gameTime)
        {
            if(gameTime.TotalGameTime.Seconds == this.timeToDeploy)
            {
                Console.WriteLine("IN" + gameTime.TotalGameTime.Seconds);
                EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive,EventCategoryType.Camera, new object[] {cameraToChangeTo}));
            }
            this.currentTime = gameTime.TotalGameTime.Seconds;
            base.Update(gameTime);
        }
    }
}
