/**
 * Author Tomas
 * This manager handles events from puzzles in order to allow a cutscene to happen it recives information telling 
 * it to wait till a certain time has passed then switch camera
 * 
 * Bugs: None
 */

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

        /*
         * gets current time and adds the time to wait to it this is then stored till the time occurs
         */
        public void timeFunction(EventData eventData)
        {
            this.secondsToWait = (int) eventData.AdditionalParameters[0];
            
            this.cameraToChangeTo = eventData.AdditionalParameters[1] as string;

            this.timeToDeploy = currentTime + secondsToWait;
        }

        // Waits till the time is appropriate and fires off an event to change back to First Person Camera
        public override void Update(GameTime gameTime)
        {
            if(gameTime.TotalGameTime.Seconds == this.timeToDeploy)
            {
                
                EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive,EventCategoryType.Camera, new object[] {cameraToChangeTo}));
            }
            this.currentTime = gameTime.TotalGameTime.Seconds;
            base.Update(gameTime);
        }
    }
}
