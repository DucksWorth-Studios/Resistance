using JigLibX.Collision;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using System;

namespace GDLibrary
{
    public class PhysicsManager : GameComponent
    {
        #region Fields
        private PhysicsSystem physicSystem;
        private PhysicsController physCont;
        private float timeStep = 0;
        //pause/unpause based on menu event
        private bool bPaused = true;
        #endregion

        #region Properties
        public bool Paused
        {
            get
            {
                return bPaused;
            }
            set
            {
                bPaused = value;
            }
        }
        public PhysicsSystem PhysicsSystem
        {
            get
            {
                return physicSystem;
            }
        }
        public PhysicsController PhysicsController
        {
            get
            {
                return physCont;
            }
        }
        #endregion

        public PhysicsManager(Game game, EventDispatcher eventDispatcher)
            : base(game)
        {
            this.physicSystem = new PhysicsSystem();

            //add cd/cr system
            this.physicSystem.CollisionSystem = new CollisionSystemSAP();
            this.physicSystem.EnableFreezing = true;
            this.physicSystem.SolverType = PhysicsSystem.Solver.Normal;
            this.physicSystem.CollisionSystem.UseSweepTests = true;
            //affect accuracy and the overhead == time required
            this.physicSystem.NumCollisionIterations = 8; //8
            this.physicSystem.NumContactIterations = 8; //8
            this.physicSystem.NumPenetrationRelaxtionTimesteps = 12; //15

            #region SETTING_COLLISION_ACCURACY
            //affect accuracy of the collision detection
            this.physicSystem.AllowedPenetration = 0.000025f;
            this.physicSystem.CollisionTollerance = 0.00005f;
            #endregion

            this.physCont = new PhysicsController();
            this.physicSystem.AddController(physCont);

            #region Event Handling
            //pause/unpause events
            eventDispatcher.MenuChanged += EventDispatcher_MenuChanged;
            #endregion
        }

        private void EventDispatcher_MenuChanged(EventData eventData)
        {
            if (eventData.EventCategoryType == EventCategoryType.MainMenu)
            {
                if (eventData.EventType == EventActionType.OnPlay)
                    this.bPaused = false;
                else if (eventData.EventType == EventActionType.OnPause)
                    this.bPaused = true;
            }
        }

        public override void Initialize()
        {
            base.Initialize();
        }
        public override void Update(GameTime gameTime)
        {
            if (!bPaused)
            {
                timeStep = (float)gameTime.ElapsedGameTime.Ticks / TimeSpan.TicksPerSecond;
                //if the time between updates indicates a FPS of close to 60 fps or less then update CD/CR engine
                if (timeStep < 1.0f / 60.0f)
                    physicSystem.Integrate(timeStep);
                else
                    //else fix at 60 updates per second
                    physicSystem.Integrate(1.0f / 60.0f);
            }
            base.Update(gameTime);
        }

        //to do - dispose
    }
}
