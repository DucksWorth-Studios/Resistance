/*
Function: 		Enables CDCR through JibLibX by integrating forces applied to each collidable object within the scene
Author: 		NMCG
Version:		1.0
Date Updated:	27/10/17
Bugs:			
Fixes:			None
*/

using System;
using System.Collections.Generic;
using JigLibX.Collision;
using JigLibX.Physics;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class PhysicsManager : PausableGameComponent
    {
        //gravity pre-defined
        public PhysicsManager(Game game, EventDispatcher eventDispatcher, StatusType statusType)
            : this(game, eventDispatcher, statusType, -10 * Vector3.UnitY)
        {
        }

        //user-defined gravity
        public PhysicsManager(Game game, EventDispatcher eventDispatcher, StatusType statusType, Vector3 gravity)
            : base(game, eventDispatcher, statusType)
        {
            PhysicsSystem = new PhysicsSystem();

            //add cd/cr system
            PhysicsSystem.CollisionSystem = new CollisionSystemSAP();

            //allows us to define the direction and magnitude of gravity - default is (0, -9.8f, 0)
            PhysicsSystem.Gravity = gravity;

            //25/11/17 - prevents bug where objects would show correct CDCR response when velocity == Vector3.Zero
            PhysicsSystem.EnableFreezing = false;

            PhysicsSystem.SolverType = PhysicsSystem.Solver.Normal;
            PhysicsSystem.CollisionSystem.UseSweepTests = true;

            //affect accuracy and the overhead == time required
            PhysicsSystem.NumCollisionIterations = 8; //8
            PhysicsSystem.NumContactIterations = 8; //8
            PhysicsSystem.NumPenetrationRelaxtionTimesteps = 12; //15          

            #region SETTING_COLLISION_ACCURACY

            //affect accuracy of the collision detection
            PhysicsSystem.AllowedPenetration = 0.000025f;
            PhysicsSystem.CollisionTollerance = 0.00005f;

            #endregion

            PhysicsController = new PhysicsController();
            PhysicsSystem.AddController(PhysicsController);

            //batch removal - as in ObjectManager
            removeList = new List<CollidableObject>();
        }

        //call when we want to remove a drawn object from the scene
        public void Remove(CollidableObject collidableObject)
        {
            removeList.Add(collidableObject);
        }

        //batch remove on all objects that were requested to be removed
        protected virtual void ApplyRemove()
        {
            foreach (var collidableObject in removeList)
                //what would happen if we did not remove the physics body? would the CD/CR skin remain?
                PhysicsSystem.RemoveBody(collidableObject.Body);

            removeList.Clear();
        }

        protected override void ApplyUpdate(GameTime gameTime)
        {
            ApplyRemove();

            timeStep = (float) gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
            //if the time between updates indicates a FPS of close to 60 fps or less then update CD/CR engine
            if (timeStep < 1.0f / 60.0f)
                PhysicsSystem.Integrate(timeStep);
            else
                //else fix at 60 updates per second
                PhysicsSystem.Integrate(1.0f / 60.0f);
        }

        #region Fields

        private float timeStep;
        private readonly List<CollidableObject> removeList;

        #endregion

        #region Properties

        public PhysicsSystem PhysicsSystem { get; }

        public PhysicsController PhysicsController { get; }

        #endregion

        #region Event Handling

        protected override void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.RemoveActorChanged += EventDispatcher_RemoveActorChanged;
            base.RegisterForEventHandling(eventDispatcher);
        }

        private void EventDispatcher_RemoveActorChanged(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnRemoveActor)
                //using the "sender" property of the event to pass reference to object to be removed - use "as" to access Body since sender is defined as a raw object.
                Remove(eventData.Sender as CollidableObject);
        }

        //See MenuManager::EventDispatcher_MenuChanged to see how it does the reverse i.e. they are mutually exclusive
        protected override void EventDispatcher_MenuChanged(EventData eventData)
        {
            //did the event come from the main menu and is it a start game event
            if (eventData.EventType == EventActionType.OnStart)
                //turn on update and draw i.e. hide the menu
                StatusType = StatusType.Update | StatusType.Drawn;
            //did the event come from the main menu and is it a pause game event
            else if (eventData.EventType == EventActionType.OnPause)
                //turn off update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Off;
            else if (eventData.EventType == EventActionType.OnLose)
                //turn off update and draw i.e. show the menu since the game is paused
                StatusType = StatusType.Off;
        }

        #endregion
    }
}