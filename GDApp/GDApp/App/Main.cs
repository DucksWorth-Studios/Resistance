#define DEMO

using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JigLibX.Geometry;
using JigLibX.Collision;
using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
/*
override Clone()
Reticule color not resetting
No statustype on controllers
mouse object text interleaving with progress controller
progress controller not receiving events to increase progress
Z-fighting on ground plane in 3rd person mode
Elevation angle on 3rd person view
ScreenManager - enum - this.ScreenType = (ScreenUtilityScreenType)eventData.AdditionalEventParameters[0];
check clone on new eventdata
PiP
menu - click sound
menu transparency
*/

namespace GDApp
{
    public class Main : Game
    {

        #region Fields
#if DEBUG
        //used to visualize debug info (e.g. FPS) and also to draw collision skins
        private DebugDrawer debugDrawer;
        private PhysicsDebugDrawer physicsDebugDrawer;
#endif

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        //added property setters in short-hand form for speed
        public ObjectManager objectManager { get; private set; }
        public CameraManager cameraManager { get; private set; }
        public MouseManager mouseManager { get; private set; }
        public KeyboardManager keyboardManager { get; private set; }
        public ScreenManager screenManager { get; private set; }
        public MyAppMenuManager menuManager { get; private set; }
        public PhysicsManager physicsManager { get; private set; }
        public UIManager uiManager { get; private set; }
        public GamePadManager gamePadManager { get; private set; }
        public SoundManager soundManager { get; private set; }
        public PickingManager pickingManager { get; private set; }

        private CutsceneTimer cutsceneTimer;

        public TimerManager timerManager { get; private set; }
        public LogicTemplate logicPuzzle;
        public ObjectiveManager objectiveManager;
        //receives, handles and routes events
        public EventDispatcher eventDispatcher { get; private set; }
        
        //stores loaded game resources
        private ContentDictionary<Model> modelDictionary;
        private ContentDictionary<Texture2D> textureDictionary;
        private ContentDictionary<SpriteFont> fontDictionary;

        //stores curves and rails used by cameras, viewport, effect parameters
        private Dictionary<string, Transform3DCurve> curveDictionary;
        private Dictionary<string, RailParameters> railDictionary;
        private Dictionary<string, Viewport> viewPortDictionary;
        private Dictionary<string, EffectParameters> effectDictionary;
        private ContentDictionary<Video> videoDictionary;
        private Dictionary<string, IVertexData> vertexDataDictionary;

        private ManagerParameters managerParameters;

        //random number for riddle
        private int riddleId;

        public int logicID;

        #endregion

        #region Properties
        #endregion


        #region Constructor
        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }
        #endregion

        #region Initialization
        protected override void Initialize()
        {
            //moved instanciation here to allow menu and ui managers to be moved to InitializeManagers()
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            int gameLevel = 1;
            bool isMouseVisible = true;
            Integer2 screenResolution = ScreenUtility.HD720;
            ScreenUtility.ScreenType screenType = ScreenUtility.ScreenType.SingleScreen;
            int numberOfGamePadPlayers = 1;
            this.riddleId = GenerateRandomNum(1, 4); // gets a random number between 1 and 4

            //set the title
            Window.Title = "Resistance";

            //EventDispatcher
            InitializeEventDispatcher();

            //Dictionaries, Media Assets and Non-media Assets
            LoadDictionaries();
            LoadAssets();
            LoadViewports(screenResolution);

            //to draw primitives and billboards
            LoadVertexData();

            //Effects
            InitializeEffects();

            //Managers
            InitializeManagers(screenResolution, screenType, isMouseVisible, numberOfGamePadPlayers);

            //menu and UI elements
            
            AddMenuElements();
            AddUIElements();
            AddGameOverMenu();
            AddWinMenu();
            addAudioMenu();
#if DEBUG
          //  InitializeDebugTextInfo();
#endif

            //load game happens before cameras are loaded because we may add a third person camera that needs a reference to a loaded Actor
            LoadGame(gameLevel);

            InitializeCollidableFirstPersonDemo(screenResolution);
            InitializeCutsceneCameras();
            //Publish Start Event(s)
            StartGame();

#if DEBUG
         //   InitializeDebugCollisionSkinInfo();
#endif

            InitializeEvents();
            initialiseTestObject();
            InitializeSwitches();
            InitialiseBaseLogicPuzzleLights();
            InitialiseSimpleLogicPuzzleLights();
            InitialiseHardLogicPuzzleLights();
            InitialisePopUP();
            InitialiseObjectiveHUD();
            loadCurrentObjective();

            InitialiseLogicPuzzle();

            base.Initialize();
        }

        /*
         * Authors: Andrew
         * Generates a random number between two values
         * will be used to pick the riddle that will be used in the game
         */
        private int GenerateRandomNum(int min, int max)
        {
            Random rand = new Random();            
            return rand.Next(min, max);
        }

  
        private void InitialiseLogicPuzzle()
        {
            Random rnd = new Random();
            int num = rnd.Next(1, 4);
            
            this.logicID = num;

            switch(num)
            {
                case 1:
                    BasePuzzle basePuzzle = new BasePuzzle(this, this.eventDispatcher);
                    this.logicPuzzle = basePuzzle as LogicTemplate;
                    this.Components.Add(this.logicPuzzle);
                    logicModelStatusChanger(num,StatusType.Drawn);
                    break;
                case 2:
                    SimplePuzzle simplePuzzle = new SimplePuzzle(this, this.eventDispatcher);
                    this.logicPuzzle = simplePuzzle as LogicTemplate;
                    this.Components.Add(this.logicPuzzle);
                    logicModelStatusChanger(num, StatusType.Drawn);
                    break;
                case 3:
                    HardPuzzle hardPuzzle = new HardPuzzle(this, this.eventDispatcher);
                    this.logicPuzzle = hardPuzzle as LogicTemplate;
                    this.Components.Add(this.logicPuzzle);
                    logicModelStatusChanger(num, StatusType.Drawn);
                    break;
                default:
                    BasePuzzle defaultPuzzle = new BasePuzzle(this, this.eventDispatcher);
                    this.logicPuzzle = defaultPuzzle as LogicTemplate;
                    this.Components.Add(this.logicPuzzle);
                    logicModelStatusChanger(1, StatusType.Drawn);
                    break;
            }
        }


        private void logicModelStatusChanger(int ID,StatusType s)
        {
            switch(ID)
            {
                case 1:
                    changeBaseStatus(s);
                    break;
                case 2:
                    changeSimpleStatus(s);
                    break;
                case 3:
                    changeHardStatus(s);
                    break;
                default:
                    changeBaseStatus(s);
                    break;
            }
        }

        #region ChangeLogicModelStatus
        private void changeBaseStatus(StatusType s)
        {
            Predicate<Actor3D> findModel = x => x.GetID() == "Base Logic Puzzle";
            ModelObject logicPuzzle =(ModelObject) this.objectManager.Find(findModel);
            logicPuzzle.StatusType = s;

            for (int i = 1; i < 6; i++)
            {
                Predicate<Actor3D> pred = y => y.ID == "base-gate-" + i;
                CollidableObject logicGate = (CollidableObject)this.objectManager.Find(pred);
                logicGate.StatusType = s;
            }
        }

        private void changeSimpleStatus(StatusType s)
        {
            Predicate<Actor3D> findModel = x => x.GetID() == "Simple Logic Puzzle";
            ModelObject logicPuzzle = (ModelObject)this.objectManager.Find(findModel);
            logicPuzzle.StatusType = s;

            for (int i = 1; i < 6; i++)
            {
                Predicate<Actor3D> pred = y => y.ID == "simple-gate-" + i;
                CollidableObject logicGate = (CollidableObject)this.objectManager.Find(pred);
                logicGate.StatusType = s;
            }
        }

        private void changeHardStatus(StatusType s)
        {
            Predicate<Actor3D> findModel = x => x.GetID() == "Hard Logic Puzzle";
            ModelObject logicPuzzle = (ModelObject)this.objectManager.Find(findModel);
            logicPuzzle.StatusType = s;

            for (int i = 1; i < 7; i++)
            {
                Predicate<Actor3D> pred = y => y.ID == "hard-gate-" + i;
                CollidableObject logicGate = (CollidableObject)this.objectManager.Find(pred);
                logicGate.StatusType = s;
            }
        }
        #endregion
        /**
         * Authors : Tomas & Aaron
         * This initialises the popup and scales it accrding to screen size
         */
        private void InitialisePopUP()
        {
            Texture2D texture = this.textureDictionary["popup"];
   
            int w,x,y,z,tw,th;
            int temp = graphics.PreferredBackBufferWidth / 4;
             x = graphics.PreferredBackBufferWidth / 6;
             y = graphics.PreferredBackBufferHeight / 6;
             w = graphics.PreferredBackBufferWidth - (x*2);
             z = graphics.PreferredBackBufferHeight - (y * 2);
             tw = texture.Width;
             th = texture.Height;


            Vector2 scale = new Vector2(
                (float)x/700,
                (float)y/390);


            Vector2 translation = new Vector2(
                (float)x,
                (float)y);

            Transform2D transform = new Transform2D(translation,0,scale, new Vector2(0,0), new Integer2(0,0));


            // Transform2D transform = new Transform2D(new Vector2(x,y), 0, new Vector2(1f, 1f),new Vector2(1,1),new Integer2(w,z));
             Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(0,0, tw, th);


            UITextureObject picture = new UITextureObject("PopUp",ActorType.PopUP,StatusType.Off,transform,Color.White,
                SpriteEffects.None,0,texture,rect, new Vector2(0,0));

            this.uiManager.Add(picture);
        }


        private void InitialiseObjectiveHUD()
        {
            Texture2D texture = this.textureDictionary["Objective"];

            int x,y,tw, th;
            tw = texture.Width;
            th = texture.Height;
            x = graphics.PreferredBackBufferWidth;
            y = graphics.PreferredBackBufferHeight;


            Vector2 scale = new Vector2(
                (float)(x/y),
                (float)(x/y));


            Vector2 translation = new Vector2(
                (float)(graphics.PreferredBackBufferWidth / 2) - (((x / y) * tw)) / 2,
                (float)1);

            Transform2D transform = new Transform2D(translation, 0, scale, new Vector2(0, 0), new Integer2(0, 0));


            // Transform2D transform = new Transform2D(new Vector2(x,y), 0, new Vector2(1f, 1f),new Vector2(1,1),new Integer2(w,z));
            Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(0, 0, tw, th);


            UITextureObject picture = new UITextureObject("Objective", ActorType.UIDynamicText, StatusType.Drawn, transform, Color.White,
                SpriteEffects.None, 0, texture, rect, new Vector2(0, 0));
            this.uiManager.Add(picture);
        }


        private void loadCurrentObjective()
        {
            Texture2D texture = objectiveManager.InitializeObjectivesUI();

            int x, y, tw, th;
            tw = texture.Width;
            th = texture.Height;
            x = graphics.PreferredBackBufferWidth;
            y = graphics.PreferredBackBufferHeight;


            Vector2 scale = new Vector2(
                (float)(x / y),
                (float)(x / y));


            Vector2 translation = new Vector2(
                (float)(graphics.PreferredBackBufferWidth / 2) - (((x / y) * tw)) / 2,
                (float)y/18);

            Transform2D transform = new Transform2D(translation, 0, scale, new Vector2(0, 0), new Integer2(0, 0));


            // Transform2D transform = new Transform2D(new Vector2(x,y), 0, new Vector2(1f, 1f),new Vector2(1,1),new Integer2(w,z));
            Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(0, 0, tw, th);


            UITextureObject picture = new UITextureObject("currentObjective", ActorType.Objective, StatusType.Drawn, transform, Color.White,
                SpriteEffects.None, 0, texture, rect, new Vector2(0, 0));

            this.uiManager.Add(picture);
        }

        /**
         * Author: Tomas
         * Initialises switch objects for logic puzzle
         */
        private void InitializeSwitches()
        {
            CollidableObject collidableObject, archetypeCollidableObject = null;

            Model model = this.modelDictionary["box2"];
            BasicEffectParameters effectParameters = (this.effectDictionary[AppData.LitModelsEffectID] as BasicEffectParameters).Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["gray"];

            archetypeCollidableObject = new CollidableObject("switch-", ActorType.Interactable, Transform3D.Zero, effectParameters, model);

            int count = 0;
            for (int i = 1; i < 5; ++i)
            {
                ++count;
                collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
                collidableObject.ID = "switch-" + count;


                collidableObject.Transform = new Transform3D(new Vector3(-46, 5.5f * i, -125), new Vector3(0, 0, 0), new Vector3(1, 1, 1), Vector3.UnitX, Vector3.UnitY);
                collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, 2.54f * collidableObject.Transform.Scale), new MaterialProperties(0.2f, 0.8f, 0.7f));

                //increase the mass of the boxes in the demo to see how collidable first person camera interacts vs. spheres (at mass = 1)
                collidableObject.Enable(true, 1);
                this.objectManager.Add(collidableObject);

            }
        }

        /**
         * Author Tomas
         * Initialise the lights for logic puzzle gates
         */
        private void InitialiseBaseLogicPuzzleLights()
        {
            CollidableObject collidableObject, archetypeCollidableObject = null;
            Model model = this.modelDictionary["sphere"];

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["gray"];

            //make once then clone
            archetypeCollidableObject = new CollidableObject("sphere", ActorType.Light, Transform3D.Zero, effectParameters, model);
            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();

            #region Gate-1
            collidableObject.ID = "base-gate-1";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-36.5f, 12.75f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
            #endregion


            #region Gate-2
            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "base-gate-2";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-33, 19.75f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
            #endregion


            #region Gate-3

            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "base-gate-3";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-25.75f, 9, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);

            #endregion

            #region Gate-4

            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "base-gate-4";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-13.5f, 13.75f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);

            #endregion

            #region End Light

            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "base-gate-5";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-11f, 13.75f, -125.25f), new Vector3(0, 0, 0),
                0.025f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.025f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);

            #endregion
        }

        private void InitialiseSimpleLogicPuzzleLights()
        {
            CollidableObject collidableObject, archetypeCollidableObject = null;
            Model model = this.modelDictionary["sphere"];

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["gray"];

            //make once then clone
            archetypeCollidableObject = new CollidableObject("sphere", ActorType.Light, Transform3D.Zero, effectParameters, model);
            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();

            #region Gate-1
            collidableObject.ID = "simple-gate-1";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-35f, 14.5f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
            #endregion


            #region Gate-2
            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "simple-gate-2";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-29.75f, 19, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
            #endregion


            #region Gate-3

            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "simple-gate-3";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-23f, 12.25f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);

            #endregion

            #region Gate-4

            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "simple-gate-4";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-15.5f, 13.75f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);

            #endregion

            #region End Light

            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "simple-gate-5";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-11f, 13.75f, -125.25f), new Vector3(0, 0, 0),
                0.025f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.025f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);

            #endregion
        }

        private void InitialiseHardLogicPuzzleLights()
        {
            CollidableObject collidableObject, archetypeCollidableObject = null;
            Model model = this.modelDictionary["sphere"];

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["gray"];

            //make once then clone
            archetypeCollidableObject = new CollidableObject("sphere", ActorType.Light, Transform3D.Zero, effectParameters, model);
            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();

            #region Gate-1
            collidableObject.ID = "hard-gate-1";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-37.5f, 13.75f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
            #endregion


            #region Gate-2
            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "hard-gate-2";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-36, 20.25f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
            #endregion

            #region Gate-3
            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "hard-gate-3";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-32, 9f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
            #endregion

            #region Gate-4

            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "hard-gate-4";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-26f, 11.1f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);

            #endregion

            #region Gate-5

            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "hard-gate-5";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-16f, 13.75f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);

            #endregion

            #region End Light

            collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
            collidableObject.ID = "hard-gate-6";
            collidableObject.StatusType = StatusType.Off;
            collidableObject.Transform = new Transform3D(new Vector3(-11f, 13.75f, -125.25f), new Vector3(0, 0, 0),
                0.025f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.025f), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);

            #endregion
        }

        #region TestObjects
        /**
         * Author Tomas
         * This test object is a simple interactive rectangle used to test 
         * various functions e.g interact function, pop ups etc etc
         */
        private void initialiseTestObject()
        {
            Model model = this.modelDictionary["box2"];
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["riddletexture"];
            Transform3D transform = new Transform3D(new Vector3(-90, 6.9f, -120), new Vector3(-90, 0, 0), new Vector3(1, 1, 0.0001f), Vector3.UnitX, Vector3.UnitY);
            CollidableObject collidableObject = new CollidableObject("Riddle Pickup", ActorType.PopUP, transform, effectParameters, model);
            collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, 2.54f * collidableObject.Transform.Scale), new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        #endregion


        private void InitializeManagers(Integer2 screenResolution,
            ScreenUtility.ScreenType screenType, bool isMouseVisible, int numberOfGamePadPlayers) //1 - 4
        {
            //add sound manager
            this.soundManager = new SoundManager(this, this.eventDispatcher, StatusType.Update, "Content/Assets/Audio/", "Music.xgs", "Music Wave Bank.xwb", "Music Sound Bank.xsb");
            Components.Add(this.soundManager);

            this.cameraManager = new CameraManager(this, 1, this.eventDispatcher);
            Components.Add(this.cameraManager);

            //create the object manager - notice that its not a drawablegamecomponent. See ScreeManager::Draw()
            this.objectManager = new ObjectManager(this, this.cameraManager, this.eventDispatcher, 10);

            //add keyboard manager
            this.keyboardManager = new KeyboardManager(this);
            Components.Add(this.keyboardManager);

            //create the manager which supports multiple camera viewports
            this.screenManager = new ScreenManager(this, graphics, screenResolution, screenType,
                this.objectManager, this.cameraManager, this.keyboardManager,
                AppData.KeyPauseShowMenu, this.eventDispatcher, StatusType.Off);
            Components.Add(this.screenManager);

            //CD-CR using JigLibX and add debug drawer to visualise collision skins
            this.physicsManager = new PhysicsManager(this, this.eventDispatcher, StatusType.Off, AppData.BigGravity);
            Components.Add(this.physicsManager);

            //add mouse manager
            this.mouseManager = new MouseManager(this, isMouseVisible, this.physicsManager);
            Components.Add(this.mouseManager);

            //add gamepad manager
            if (numberOfGamePadPlayers > 0)
            {
                this.gamePadManager = new GamePadManager(this, numberOfGamePadPlayers);
                Components.Add(this.gamePadManager);
            }

            //menu manager
            this.menuManager = new MyAppMenuManager(this, this.mouseManager, this.keyboardManager, this.cameraManager, spriteBatch, this.eventDispatcher, StatusType.Off);
            //set the main menu to be the active menu scene
            this.menuManager.SetActiveList("main menu");
            //this.menuManager.SetActiveList("lose-screen");
            Components.Add(this.menuManager);

            //ui (e.g. reticule, inventory, progress)
            this.uiManager = new UIManager(this, this.spriteBatch, this.eventDispatcher, 10, StatusType.Off);
            Components.Add(this.uiManager);


            //this object packages together all managers to give the mouse object the ability to listen for all forms of input from the user, as well as know where camera is etc.
            this.managerParameters = new ManagerParameters(this.objectManager,
                this.cameraManager, this.mouseManager, this.keyboardManager, this.gamePadManager, this.screenManager, this.soundManager);

            

            #region Pick Manager
            //call this function anytime we want to decide if a mouse over object is interesting to the PickingManager
            //See https://www.codeproject.com/Articles/114931/Understanding-Predicate-Delegates-in-C
            Predicate<CollidableObject> collisionPredicate = new Predicate<CollidableObject>(CollisionUtility.IsCollidableObjectOfInterest);
            //create the projectile archetype that the manager can fire

            //listens for picking with the mouse on valid (based on specified predicate) collidable objects and pushes notification events to listeners
            this.pickingManager = new PickingManager(this, this.eventDispatcher, StatusType.Off,
                this.managerParameters,
                PickingBehaviourType.InteractWithObject, AppData.PickStartDistance, AppData.PickEndDistance, collisionPredicate);
            Components.Add(this.pickingManager);
            #endregion
            
            this.cutsceneTimer = new CutsceneTimer("CutsceneTimer", this.eventDispatcher,this);
            Components.Add(cutsceneTimer);


            this.timerManager = new TimerManager(AppData.LoseTimerID, AppData.LoseTimerHours, AppData.LoseTimerMinutes, 
                AppData.LoseTimerSeconds, this, eventDispatcher, StatusType.Off);
            Components.Add(timerManager);


            this.objectiveManager = new ObjectiveManager(this, this.eventDispatcher, StatusType.Off, 0, this.spriteBatch,this.textureDictionary,this.uiManager);
            Components.Add(this.objectiveManager);

        }

        private void LoadDictionaries()
        {
            //models
            this.modelDictionary = new ContentDictionary<Model>("model dictionary", this.Content);

            //textures
            this.textureDictionary = new ContentDictionary<Texture2D>("texture dictionary", this.Content);

            //fonts
            this.fontDictionary = new ContentDictionary<SpriteFont>("font dictionary", this.Content);

            //curves - notice we use a basic Dictionary and not a ContentDictionary since curves and rails are NOT media content
            this.curveDictionary = new Dictionary<string, Transform3DCurve>();

            //rails
            this.railDictionary = new Dictionary<string, RailParameters>();

            //viewports - used to store different viewports to be applied to multi-screen layouts
            this.viewPortDictionary = new Dictionary<string, Viewport>();

            //stores default effect parameters
            this.effectDictionary = new Dictionary<string, EffectParameters>();

            //notice we go back to using a content dictionary type since we want to pass strings and have dictionary load content
            this.videoDictionary = new ContentDictionary<Video>("video dictionary", this.Content);

            //used to store IVertexData (i.e. when we want to draw primitive objects, as in I-CA)
            this.vertexDataDictionary = new Dictionary<string, IVertexData>();

        }

        private void LoadAssets()
        {
            #region Models
            //geometric samples
            this.modelDictionary.Load("Assets/Models/plane1", "plane1");
            //this.modelDictionary.Load("Assets/Models/plane", "plane");
            this.modelDictionary.Load("Assets/Models/box2", "box2");
            this.modelDictionary.Load("Assets/Models/sphere", "sphere");
            //architecture
           
            this.modelDictionary.Load("Assets/Models/Architecture/Doors/Barrier_Mapped_01", "barrier");
            this.modelDictionary.Load("Assets/Models/Architecture/Doors/BunkerDoorSmooth", "bunker_door");
            this.modelDictionary.Load("Assets/Models/Architecture/Doors/BunkerDoor_Mapped_01", "ExitDoor");
            //props
            this.modelDictionary.Load("Assets/Models/Props/lamp");
            this.modelDictionary.Load("Assets/Models/Props/ammo-box");
            this.modelDictionary.Load("Assets/Models/Props/field-cot");
            this.modelDictionary.Load("Assets/Models/Props/field-desk");
            this.modelDictionary.Load("Assets/Models/Props/war-table");
            this.modelDictionary.Load("Assets/Models/Props/FilingCabinet");
            this.modelDictionary.Load("Assets/Models/Props/Bookshelf_01");
            this.modelDictionary.Load("Assets/Models/Props/Phonograph");
            this.modelDictionary.Load("Assets/Models/Props/computer");
            this.modelDictionary.Load("Assets/Models/Props/LogicPuzzle","BaseLogicPuzzle");
            this.modelDictionary.Load("Assets/Models/Props/SimplePuzzle","SimpleLogicPuzzle");
            this.modelDictionary.Load("Assets/Models/Props/HardPuzzle", "HardLogicPuzzle");
            this.modelDictionary.Load("Assets/Models/Props/wine-bottle");
            this.modelDictionary.Load("Assets/Models/Props/globe");
            this.modelDictionary.Load("Assets/Models/Props/Stielhandgranate", "grenade");
            this.modelDictionary.Load("Assets/Models/Props/GermanHelmet", "helmet");
            this.modelDictionary.Load("Assets/Models/Props/hat2", "hat");
            //this.modelDictionary.Load("Assets/Models/Props/phone3", "phone");
            this.modelDictionary.Load("Assets/Models/Props/shelf1", "shelf");

            //riddle object
            if(this.riddleId == 1)
            {
                this.modelDictionary.Load("Assets/Models/Props/riddleObjects/riddleObj-1", "riddleAnswerObj");
                this.modelDictionary.Load("Assets/Models/Props/riddleObjects/riddleObj-2", "clock");
                this.modelDictionary.Load("Assets/Models/Props/riddleObjects/riddleObj-3", "phone");
            }
            else if(this.riddleId == 2)
            {
                this.modelDictionary.Load("Assets/Models/Props/riddleObjects/riddleObj-2", "riddleAnswerObj");
                this.modelDictionary.Load("Assets/Models/Props/riddleObjects/riddleObj-1", "gun");
                this.modelDictionary.Load("Assets/Models/Props/riddleObjects/riddleObj-3", "phone");
            }
            else
            {
                this.modelDictionary.Load("Assets/Models/Props/riddleObjects/riddleObj-3", "riddleAnswerObj");
                this.modelDictionary.Load("Assets/Models/Props/riddleObjects/riddleObj-1", "gun");
                this.modelDictionary.Load("Assets/Models/Props/riddleObjects/riddleObj-2", "clock");
            }
            #endregion

            #region Textures
            //environment
            this.textureDictionary.Load("Assets/Textures/Architecture/concrete2", "wall");
            this.textureDictionary.Load("Assets/Textures/Architecture/concrete", "concreteFloor");
            this.textureDictionary.Load("Assets/Textures/Architecture/concrete2");

            //menu - buttons
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/genericbtn");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/quit");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/start");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/restart-Button");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/Resume");

            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/back-Button","back");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/audio-button","Audio");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/control-button","controls");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/mainMenu-Button");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/volUp");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/volDown");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/audio-bar");


            //menu - backgrounds
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/Title-screen");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/game-over");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/PauseMenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/control-screen", "ControlMenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/win-screen");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/Audio-Menu" ,"audioMenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/mission-brief");

            //ui (or hud) elements
            this.textureDictionary.Load("Assets/Textures/UI/HUD/reticuleDefault");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/progress_gradient");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/Objective");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/Escape");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/Riddle");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/Logic");

            //architecture

            this.textureDictionary.Load("Assets/Textures/Architecture/Doors/Concrete", "concrete");
            this.textureDictionary.Load("Assets/Textures/Architecture/Doors/BrushedAluminum", "aluminum");
            this.textureDictionary.Load("Assets/Textures/Props/Crates/crate1");     
            

            //Load Colors
            this.textureDictionary.Load("Assets/Colours/gray");
            this.textureDictionary.Load("Assets/Colours/green");
            this.textureDictionary.Load("Assets/Colours/black");

            //props
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/ammo-box");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/ComputerTexture");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/FieldCotTexture");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/FieldDeskTexture");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/WarTableTexture");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/LightTexture");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/FilingCabinet");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/bookcase");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/phonograph");
            //this.textureDictionary.Load("Assets/Textures/Props/Interactable/GunTexture");
            this.textureDictionary.Load("Assets/Textures/Props/Globe/mp");
            this.textureDictionary.Load("Assets/Textures/Props/Globe/mtlscr");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/grenadetexture", "grenade");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/German Helmet", "helmet");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/hat");
            //this.textureDictionary.Load("Assets/Textures/Props/Resistance/phonetex", "phone");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/wood");
            this.textureDictionary.Load("Assets/Textures/Props/Resistance/map");
            this.textureDictionary.Load("Assets/Textures/Props/Crates/crate1");

            //propaganda
            this.textureDictionary.Load("Assets/Textures/Props/Propaganda/ww2-propaganda_waffenss", "poster-1");
            this.textureDictionary.Load("Assets/Textures/Props/Propaganda/poster2", "poster-2");
            this.textureDictionary.Load("Assets/Textures/Props/Propaganda/cuft", "poster-3");
            this.textureDictionary.Load("Assets/Textures/Props/Propaganda/unsere-luftwaffe", "poster-4");

            //interactable
            //riddle object
            if (this.riddleId == 1)
            {
                this.textureDictionary.Load("Assets/Textures/Props/Interactable/riddleObjTexture-1", "riddleObjTexture");
                this.textureDictionary.Load("Assets/Textures/Props/Interactable/riddleObjTexture-2", "clock");
                this.textureDictionary.Load("Assets/Textures/Props/Interactable/riddleObjTexture-3", "phone");
            }
            else if (this.riddleId == 2)
            {
                this.textureDictionary.Load("Assets/Textures/Props/Interactable/riddleObjTexture-2", "riddleObjTexture");
                this.textureDictionary.Load("Assets/Textures/Props/Interactable/riddleObjTexture-1", "gun");
                this.textureDictionary.Load("Assets/Textures/Props/Interactable/riddleObjTexture-3", "phone");
            }
            else
            {
                this.textureDictionary.Load("Assets/Textures/Props/Interactable/riddleObjTexture-3", "riddleObjTexture");
                this.textureDictionary.Load("Assets/Textures/Props/Interactable/riddleObjTexture-1", "gun");
                this.textureDictionary.Load("Assets/Textures/Props/Interactable/riddleObjTexture-2", "clock");
            }

            this.textureDictionary.Load("Assets/Textures/Props/Interactable/riddletexture");

            //load riddle pop up
            this.textureDictionary.Load("Assets/Textures/UI/HUD/Popup/riddlePopup-" + this.riddleId, "popup");

#if DEBUG
            //demo
            this.textureDictionary.Load("Assets/GDDebug/Textures/ml");
            this.textureDictionary.Load("Assets/GDDebug/Textures/checkerboard");
#endif


            #endregion

            #region Fonts
#if DEBUG

            this.fontDictionary.Load("Assets/GDDebug/Fonts/debug");

#endif
            this.fontDictionary.Load("Assets/Fonts/menu");
            this.fontDictionary.Load("Assets/Fonts/mouse");
            this.fontDictionary.Load("Assets/Fonts/timerFont");
            #endregion


        }

        private void LoadVertexData()
        {
            Microsoft.Xna.Framework.Graphics.PrimitiveType primitiveType;
            int primitiveCount;
            IVertexData vertexData = null;

            #region Textured Quad
            //get vertices for textured quad
            VertexPositionColorTexture[] vertices = VertexFactory.GetTextureQuadVertices(out primitiveType, out primitiveCount);

            //make a vertex data object to store and draw the vertices
            vertexData = new BufferedVertexData<VertexPositionColorTexture>(this.graphics.GraphicsDevice, vertices, primitiveType, primitiveCount);

            //add to the dictionary for use by things like billboards - see InitializeBillboards()
            this.vertexDataDictionary.Add(AppData.TexturedQuadID, vertexData);
            #endregion

            #region Billboard Quad - we must use this type when creating billboards
            // get vertices for textured billboard
            VertexBillboard[] verticesBillboard = VertexFactory.GetVertexBillboard(1, out primitiveType, out primitiveCount);

            //make a vertex data object to store and draw the vertices
            vertexData = new BufferedVertexData<VertexBillboard>(this.graphics.GraphicsDevice, verticesBillboard, primitiveType, primitiveCount);

            //add to the dictionary for use by things like billboards - see InitializeBillboards()
            this.vertexDataDictionary.Add(AppData.TexturedBillboardQuadID, vertexData);
            #endregion

        }

        private void LoadViewports(Integer2 screenResolution)
        {

            //the full screen viewport with optional padding
            int leftPadding = 0, topPadding = 0, rightPadding = 0, bottomPadding = 0;
            Viewport paddedFullViewPort = ScreenUtility.Pad(new Viewport(0, 0, screenResolution.X, (int)(screenResolution.Y)), leftPadding, topPadding, rightPadding, bottomPadding);
            this.viewPortDictionary.Add("full viewport", paddedFullViewPort);

            //work out the dimensions of the small camera views along the left hand side of the screen
            int smallViewPortHeight = 144; //6 small cameras along the left hand side of the main camera view i.e. total height / 5 = 720 / 5 = 144
            int smallViewPortWidth = 5 * smallViewPortHeight / 3; //we should try to maintain same ProjectionParameters aspect ratio for small cameras as the large     
            //the five side viewports in multi-screen mode
            this.viewPortDictionary.Add("column0 row0", new Viewport(0, 0, smallViewPortWidth, smallViewPortHeight));
            this.viewPortDictionary.Add("column0 row1", new Viewport(0, 1 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            this.viewPortDictionary.Add("column0 row2", new Viewport(0, 2 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            this.viewPortDictionary.Add("column0 row3", new Viewport(0, 3 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            this.viewPortDictionary.Add("column0 row4", new Viewport(0, 4 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            //the larger view to the right in column 1
            this.viewPortDictionary.Add("column1 row0", new Viewport(smallViewPortWidth, 0, screenResolution.X - smallViewPortWidth, screenResolution.Y));

            //picture-in-picture viewport
            Integer2 viewPortDimensions = new Integer2(240, 150); //set to 16:10 ratio as with screen dimensions
            int verticalOffset = 20;
            int rightHorizontalOffset = 20;
            this.viewPortDictionary.Add("PIP viewport", new Viewport((screenResolution.X - viewPortDimensions.X - rightHorizontalOffset),
                verticalOffset, viewPortDimensions.X, viewPortDimensions.Y));
        }

#if DEBUG
        private void InitializeDebugTextInfo()
        {
            //add debug info in top left hand corner of the screen
            this.debugDrawer = new DebugDrawer(this, this.managerParameters, spriteBatch,
                this.fontDictionary["debug"], Color.Black, new Vector2(5, 5), this.eventDispatcher, StatusType.Off);
            Components.Add(this.debugDrawer);

        }

        private void InitializeDebugCollisionSkinInfo()
        {
            //show the collision skins
            this.physicsDebugDrawer = new PhysicsDebugDrawer(this, this.cameraManager, this.objectManager,
                this.screenManager, this.eventDispatcher, StatusType.Off);
            Components.Add(this.physicsDebugDrawer);
        }
#endif
        #endregion

        #region Load Game Content
        //load the contents for the level specified
        private void LoadGame(int level)
        {
            int worldScale = 100;
            // inital game volume
            this.soundManager.ChangeVolume(1f, "Default");

            //collidable


            InitializeCollidableWalls(worldScale);
            InitialiseStairs(worldScale);
            InitializeCollidableGround(worldScale);
            InitializeNonCollidableCeiling(worldScale);
            InitialiseExitHallDoors();
            //add level elements
            //InitializeBuildings();
            InitializeExitDoor();
            //InitializeDoorBarriers();

            //init props
            InitializeWarTable();
            //InitializeCeilingLights();
            InitializeAmmoBoxes();
            InitializeFieldCot();
            InitializeFieldDesk();
            InitializeFilingCabinet();
            InitializeBookCaseDoor();
            //InitializeBookCaseProp();
            InitializePhonoGraph();
            InitializeComputer();
            InitializeLogicPuzzleModel();
            InitializeRiddleAnswerObject();
            InitializeWinVolumeBox();
            InitialiseWineBottles();
            InitialiseGlobe();
            InitialiseGrenade();
            InitialiseHelmet();
            InitialiseHat();
            //InitialisePhone();
            InitialiseShelf();
            InitializePosters();
            InitializeMap();
            InitializeCrates();
            if (this.riddleId == 1)
            {
                InitializeNonRiddleModels(AppData.ClockTransform, "clock");
                InitializeNonRiddleModels(AppData.PhoneTransform, "phone");
            }
            else if(this.riddleId == 2)
            {
                InitializeNonRiddleModels(AppData.gunTransform, "gun");
                InitializeNonRiddleModels(AppData.PhoneTransform, "phone");
            }
            else
            {
                InitializeNonRiddleModels(AppData.gunTransform, "gun");
                InitializeNonRiddleModels(AppData.ClockTransform, "clock");
            }
        }

        private void InitializeNonRiddleModels(Transform3D transform3D, string objName)
        {
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;
            
            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary[objName];

            collidableObject = new TriangleMeshObject(objName, ActorType.CollidableDecorator|ActorType.Interactable, transform3D, effectParameters,
                this.modelDictionary[objName], new MaterialProperties(0.5f, 0.5f, 0.5f));
            collidableObject.Enable(true, 1);

            this.objectManager.Add(collidableObject);
        }

        private void InitializeCollidableWalls(int worldScale)
        {
            //first we will create a prototype plane and then simply clone it for each of the skybox decorator elements (e.g. ground, front, top etc). 
            Transform3D transform = new Transform3D(new Vector3(0, 0, 0), new Vector3(worldScale, 1, worldScale / 10.0f));

            //clone the dictionary effect and set unique properties for the hero player object
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["wall"];

            CollidableObject prototypeModel = new CollidableObject("plane1", ActorType.Decorator, transform, effectParameters, this.modelDictionary["box2"]);

            CollidableObject clonePlane = null;

            #region walls
            #region back wall
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.ID = "back wall";
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];

            clonePlane.Transform.Scale = new Vector3(worldScale / 2.0f, 1, worldScale / 10.0f);

            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 4f, (2.54f * worldScale) / 20.0f, (-2.54f * worldScale) / 2.0f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2), 
                new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)), 
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);
            #endregion

            #region left wall
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 2.0f, (2.54f * worldScale) / 20.0f, 0);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);
            #endregion

            #region short right wall
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(worldScale / 4, 1, worldScale / 10.0f);
            clonePlane.Transform.Translation = new Vector3(worldScale / 128.0f, (2.54f * worldScale) / 20.0f, (-2.54f * worldScale) / 2.67f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);
            #endregion region

            #region long right wall
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(3 * worldScale / 4, 1, worldScale / 10.0f);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 4.0f, (2.54f * worldScale) / 20.0f, (2.54f * worldScale) / 8.0f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f,0.1f,0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);
            #endregion

            #region 2nd room short wall
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(worldScale / 4, 1, worldScale / 10.0f);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 8.0f, (2.54f * worldScale) / 20.0f, (-2.54f * worldScale) / 4.0f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2), 
                new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)), 
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);
            #endregion

            #region front wall
            //add the front skybox plane
            // this side will be done in 3 blocks two on each side with a space for a door and then a block on top of it
            //left side of door
            float xScale = 0.833f * worldScale / 8.0f;
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(xScale, 1, worldScale / 10.0f);
            clonePlane.Transform.Translation = new Vector3((-2.493f * worldScale) / 2.2f, (2.54f * worldScale) / 20.0f, (2.54f * worldScale) / 2f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2),
                new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);

            //right side of door
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(xScale, 1, worldScale / 10.0f);
            clonePlane.Transform.Translation = new Vector3((-2.55f * worldScale) / 3.5f, (2.54f * worldScale) / 20.0f, (2.54f * worldScale) / 2f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2),
                new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);

            //top of door way
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(xScale / 1.88f, 1, worldScale / 30);
            clonePlane.Transform.Translation = new Vector3((-2.514f * worldScale) / 2.7f, worldScale / 4.6f, (2.54f * worldScale) / 2f);
            clonePlane.AddPrimitive(new Box(new Vector3(0,20,0), Matrix.CreateRotationX(MathHelper.PiOver2),
                new Vector3(0,0,0)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);
            #endregion


            #region Exit End Wall
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.EffectParameters =
                this.effectDictionary[AppData.DarkLitModelsEffectID].Clone() as BasicEffectParameters;
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(xScale, 30, worldScale / 5.0f);
            clonePlane.Transform.Translation = new Vector3((-2.493f * worldScale) / 2.2f, (2.54f * worldScale) / 20.0f, (7f * worldScale) / 2f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2),
                new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);

            ////right side of door
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.EffectParameters =
                this.effectDictionary[AppData.DarkLitModelsEffectID].Clone() as BasicEffectParameters;
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(xScale * 0.8f, 30, worldScale / 5.0f);
            clonePlane.Transform.Translation = new Vector3((-2.55f * worldScale) / 3.5f, (2.54f * worldScale) / 20.0f, (7f * worldScale) / 2f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2),
                new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);
            #endregion


            #region dividing wall
            //right side
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["concrete2"];
            clonePlane.Transform.Scale = new Vector3(xScale, 0.5f, worldScale / 10);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 3.97f, (2.54f * worldScale) / 20.0f, (-2.54f * worldScale) / 3.55f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)), 
                new MaterialProperties(0.1f,0.1f,0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);

            //left side
            clonePlane = (CollidableObject)prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["concrete2"];
            clonePlane.Transform.Scale = new Vector3(xScale, 0.5f, worldScale / 10);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 3.97f, (2.54f * worldScale) / 20.0f, (-2.54f * worldScale) / 2.2f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            this.objectManager.Add(clonePlane);
            #endregion
            #endregion

            #region ExitHall
            #region Left Exit Wall
                clonePlane = (CollidableObject)prototypeModel.Clone();
                clonePlane.EffectParameters =
                    this.effectDictionary[AppData.DarkLitModelsEffectID].Clone() as BasicEffectParameters;
                clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
                clonePlane.Transform.Scale = new Vector3(3 * worldScale / 4, 1, worldScale / 10.0f);
                clonePlane.Transform.Translation = new Vector3((-2.56f * worldScale) / 4.0f, (2.54f * worldScale) / 20.0f, 222);
                clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)),
                    new MaterialProperties(0.1f, 0.1f, 0.1f));
                clonePlane.Enable(true, 1);
                this.objectManager.Add(clonePlane);
            #endregion

            #region Right Exit Wall

                clonePlane = (CollidableObject)prototypeModel.Clone();
                clonePlane.EffectParameters =
                    this.effectDictionary[AppData.DarkLitModelsEffectID].Clone() as BasicEffectParameters;
                clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
                clonePlane.Transform.Scale = new Vector3(3 * worldScale / 4, 1, worldScale / 10.0f);
                clonePlane.Transform.Translation = new Vector3((-4.7f * worldScale) / 4.0f, (2.54f * worldScale) / 20.0f, 222);
                clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)),
                    new MaterialProperties(0.1f, 0.1f, 0.1f));
                clonePlane.Enable(true, 1);
                this.objectManager.Add(clonePlane);

            #endregion

            #endregion
        }


        private void InitialiseStairs(int worldScale)
        {
            float xScale = 0.833f * worldScale / 8.0f;
            //first we will create a prototype plane and then simply clone it for each of the skybox decorator elements (e.g. ground, front, top etc). 
            Transform3D transform = new Transform3D(new Vector3((-2.02f * worldScale) / 2.2f, 0.8f, (6.6f * worldScale) / 2f), new Vector3(xScale * 0.66f, 4, worldScale / 60f));
            
            //clone the dictionary effect and set unique properties for the hero player object
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.DarkLitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["wall"];

            CollidableObject prototypeModel = new CollidableObject("plane1", ActorType.Decorator, transform, effectParameters, this.modelDictionary["box2"]);

           

            for (int i = 0; i < 9; i++)
            {
                CollidableObject clonePlane = null;
                clonePlane = (CollidableObject)prototypeModel.Clone();
                clonePlane.ID = "stair"+i;

                clonePlane.Transform.Translation += new Vector3(0,4*i,10*i);

                clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation, Matrix.CreateRotationX(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f, clonePlane.Transform.Scale.Z * 2.54f)),
                    new MaterialProperties(0.1f, 0.1f, 0.1f));
                clonePlane.Enable(true, 1);
                this.objectManager.Add(clonePlane);
            }
        }

        //the ground is simply a large flat box with a Box primitive collision surface attached
        private void InitializeCollidableGround(int worldScale)
        {
            CollidableObject collidableObject = null;
            Transform3D transform3D = null;

            /*
             * Note that if we use DualTextureEffectParameters then (a) we must create a model (i.e. box2.fbx) in 3DS Max with two texture channels (i.e. use Unwrap UVW twice)
             * because each texture (diffuse and lightmap) requires a separate set of UV texture coordinates, and (b), this effect does NOT allow us to set up lighting. 
             * Why? Well, we don't need lighting because we can bake a static lighting response into the second texture (the lightmap) in 3DS Max).
             * 
             * See https://knowledge.autodesk.com/support/3ds-max/learn-explore/caas/CloudHelp/cloudhelp/2016/ENU/3DSMax/files/GUID-37414F9F-5E33-4B1C-A77F-547D0B6F511A-htm.html
             * See https://www.youtube.com/watch?v=vuHdnxkXpYo&t=453s
             * See https://www.youtube.com/watch?v=AqiNpRmENIQ&t=1892sl
             * 
             */
            Model model = this.modelDictionary["box2"];

            //a simple dual texture demo - dual textures can be used with a lightMap from 3DS Max using the Render to Texture setting
            //DualTextureEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelDualEffectID].Clone() as DualTextureEffectParameters;
            //effectParameters.Texture = this.textureDictionary["grass1"];
            //effectParameters.Texture2 = this.textureDictionary["checkerboard_greywhite"];

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["concreteFloor"];

            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero, new Vector3(worldScale, 0.1f, worldScale), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new CollidableObject("ground", ActorType.CollidableGround, transform3D, effectParameters, model);
            collidableObject.AddPrimitive(new Box(transform3D.Translation, Matrix.Identity, new Vector3(worldScale * 2.54f, 0.001f, worldScale * 2.54f)), new MaterialProperties(0.8f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1); //change to false, see what happens.
            this.objectManager.Add(collidableObject);




            #region exithallway
     
            transform3D = new Transform3D(new Vector3(-91, 0, 230), Vector3.Zero, new Vector3(20, 0.1f, 80), Vector3.UnitX, Vector3.UnitY);
            effectParameters = this.effectDictionary[AppData.DarkLitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["concreteFloor"];

            collidableObject = new CollidableObject("ground", ActorType.CollidableGround, transform3D, effectParameters, model);
            collidableObject.AddPrimitive(new Box(transform3D.Translation, Matrix.Identity, new Vector3(20, 0.001f, 200)),
                new MaterialProperties(0.8f, 0.8f, 0.7f));

            
            collidableObject.Enable(true, 1);

            this.objectManager.Add(collidableObject);
            #endregion
        }

        private void InitialiseExitHallDoors()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject archetype,collidableObject;

            transform3D = new Transform3D(new Vector3(-65, 0, 140), new Vector3(90, 270, 0),
                new Vector3(0.07f, 0.05f, 0.05f), Vector3.UnitX, Vector3.UnitY);

            effectParameters = this.effectDictionary[AppData.DarkLitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["aluminum"];

            archetype = new CollidableObject("exitDoor", ActorType.CollidableDoor, transform3D, effectParameters,
                this.modelDictionary["ExitDoor"]);

            for(int i = 0; i < 6; i++)
            {
                collidableObject =(CollidableObject) archetype.Clone();
                collidableObject.Transform.Translation += new Vector3(0,0, 30 * i);

                collidableObject.AddPrimitive(new Box(archetype.Transform.Translation, Matrix.Identity,
                    new Vector3(1f, 1f, 1f)),
                    new MaterialProperties(0.2f, 0.8f, 0.7f));
                collidableObject.Enable(true, 1);

                this.objectManager.Add(collidableObject);

            }

            transform3D = new Transform3D(new Vector3(-116.8f, 0, 160), new Vector3(90, 90, 0),
                new Vector3(0.07f, 0.05f, 0.05f), Vector3.UnitX, Vector3.UnitY);

            archetype.Transform = transform3D;

            for (int i = 0; i < 6; i++)
            {
                collidableObject = (CollidableObject)archetype.Clone();
                collidableObject.Transform.Translation += new Vector3(0, 0, 30 * i);

                collidableObject.AddPrimitive(new Box(archetype.Transform.Translation, Matrix.Identity,
                    new Vector3(1f, 1f, 1f)),
                    new MaterialProperties(0.2f, 0.8f, 0.7f));
                collidableObject.Enable(true, 1);

                this.objectManager.Add(collidableObject);

            }
        }

        private void InitializeNonCollidableCeiling(int worldScale)
        {
            Transform3D transform = new Transform3D(new Vector3(0, 25, 0), Vector3.Zero, new Vector3(worldScale, 0.001f, worldScale), Vector3.UnitX, Vector3.UnitY);
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["concreteFloor"];

            ModelObject model = new ModelObject("ceiling", ActorType.NonCollidableCeiling, transform, effectParameters, this.modelDictionary["box2"]);
            this.objectManager.Add(model);

            #region exit Ceiling
            transform = new Transform3D(new Vector3(-91, 25, 230), Vector3.Zero, new Vector3(20, 0.1f, 80), Vector3.UnitX, Vector3.UnitY);
            effectParameters = this.effectDictionary[AppData.DarkLitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["concreteFloor"];
            model = new ModelObject("ceiling exit hall", ActorType.NonCollidableCeiling, transform, effectParameters, this.modelDictionary["box2"]);
            this.objectManager.Add(model);
            #endregion
        }

        private void InitializeBuildings()
        {
            Transform3D transform3D = new Transform3D(new Vector3(-100, 0, 0),
                new Vector3(0, 90, 0), 0.4f * Vector3.One, Vector3.UnitX, Vector3.UnitY);


            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["house-low-texture"];

            CollidableObject collidableObject = new TriangleMeshObject("house1", ActorType.CollidableArchitecture, transform3D,
                                effectParameters, this.modelDictionary["house"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        private void InitializeExitDoor()
        {
            //NCMG
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-86, 0, 127), new Vector3(90, 180, 0), 
                new Vector3(0.07f, 0.05f, 0.05f), Vector3.UnitX, Vector3.UnitY);

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["aluminum"];

            collidableObject = new CollidableObject("exitDoor", ActorType.CollidableDoor, transform3D, effectParameters, 
                this.modelDictionary["bunker_door"]);
            collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, 
                    new Vector3(30f,50f,5f)),
                    new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            collidableObject.AttachController(new DoorController("Door Controller", ControllerType.Rotation,this.eventDispatcher));
            this.objectManager.Add(collidableObject);
        }

        private void InitializeDoorBarriers()
        {
            //Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject = null, cloneCollider = null;

            //transform3D = new Transform3D(new Vector3(-98, 6, 124), new Vector3(-90, 0, 180), new Vector3(0.1f, 0.05f, 0.1f), Vector3.UnitX, Vector3.UnitY);
            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["concrete"];

            collidableObject = new CollidableObject("barrier - ", ActorType.CollidableArchitecture, Transform3D.Zero, 
                effectParameters, this.modelDictionary["Barrier_Mapped_01"]);

            #region Top Barrier
            
            cloneCollider = (CollidableObject)collidableObject.Clone();
            cloneCollider.ID += 1;

            cloneCollider.Transform = new Transform3D(new Vector3(-107, 6, 124), new Vector3(-90, 0, 180), new Vector3(0.07f, 0.05f, 0.07f), 
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(new Box(cloneCollider.Transform.Translation, Matrix.Identity, new Vector3(12.0f, 1.0f, 1.0f)), 
                new MaterialProperties(0.2f, 0.8f, 0.7f));

            cloneCollider.Enable(true, 1);
            cloneCollider.AttachController(new BarrierController(true, "testing", ControllerType.Rotation,this.eventDispatcher));
            this.objectManager.Add(cloneCollider);
            
            #endregion

            #region Bottom Barrier
            
            cloneCollider = (CollidableObject)collidableObject.Clone();
            cloneCollider.ID += 2;

            cloneCollider.Transform = new Transform3D(new Vector3(-80, 20, 124), new Vector3(-90, 0, 0), new Vector3(0.07f, 0.05f, 0.07f), 
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(new Box(cloneCollider.Transform.Translation, Matrix.Identity, new Vector3(12.0f, 1.0f, 1.0f)),
                new MaterialProperties(0.2f, 0.8f, 0.7f));

            cloneCollider.Enable(true, 1);
            cloneCollider.AttachController(new BarrierController(false, "testing", ControllerType.Rotation,this.eventDispatcher));
            this.objectManager.Add(cloneCollider);
            
            #endregion
        }

        private void InitializeWarTable()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-80, 0, -30), new Vector3(0, 0, 0), new Vector3(2.0f, 1.0f, 3.0f), Vector3.UnitX, Vector3.UnitY);
            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["WarTableTexture"];

            collidableObject = new TriangleMeshObject("war-table", ActorType.CollidableDecorator, transform3D, effectParameters, this.modelDictionary["war-table"],
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        private void InitializePosters()
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            ModelObject model = null, clone = null;
            model = new ModelObject("poster-", ActorType.Decorator, Transform3D.Zero, effectParameters, this.modelDictionary["box2"]);
            int i = 1;

            #region right wall
            for (i = 1; i < 3; i++)
            {
                clone = (ModelObject)model.Clone();
                clone.EffectParameters.Texture = this.textureDictionary["poster-" + i];
                clone.Transform.Translation = new Vector3(-65, 15, 50 + (i * 25));
                clone.Transform.Scale = new Vector3(0.0001f, 5, 5);
                this.objectManager.Add(clone);
            }
            #endregion

            #region left wall
            for (i = 3; i < 5; i++)
            {
                clone = (ModelObject)model.Clone();
                clone.EffectParameters.Texture = this.textureDictionary["poster-" + i];
                clone.Transform.Translation = new Vector3(-125.5f, 15, 0 + (i * 25));
                clone.Transform.Scale = new Vector3(0.0001f, 5, 5);
                this.objectManager.Add(clone);
            }
            #endregion

            #region front wall
            for (i = 1; i < 3; i++)
            {
                clone = (ModelObject)model.Clone();
                clone.EffectParameters.Texture = this.textureDictionary["poster-" + i];
                clone.Transform.Translation = new Vector3(-135 + (i * 25), 15, -125);
                clone.Transform.Rotation = new Vector3(0, 90, 0);
                clone.Transform.Scale = new Vector3(0.0001f, 5, 5);
                this.objectManager.Add(clone);
            }
            #endregion
        }

        private void InitializeAmmoBoxes()
        {
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject = null, cloneCollidable;

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["ammo-box"];

            collidableObject = new CollidableObject("munitions box", ActorType.Interactable | ActorType.CollidableDecorator  , Transform3D.Zero, effectParameters,
                this.modelDictionary["ammo-box"]);
           


            for (int i = 0; i < 3; i++)
            {
                cloneCollidable = (CollidableObject)collidableObject.Clone();

                cloneCollidable.ID = "munitions box";

                cloneCollidable.Transform = new Transform3D(new Vector3(-70, 0, -30 + (i * 10)), new Vector3(0, 0, 0), new Vector3(0.05f, 0.05f, 0.05f),
                    Vector3.UnitX, Vector3.UnitY);

                cloneCollidable.AddPrimitive(new Box(cloneCollidable.Transform.Translation, Matrix.Identity, new Vector3(3,5,6)), 
                    new MaterialProperties(0.2f, 0.8f, 0.7f));

                cloneCollidable.Enable(true, 1);
                this.objectManager.Add(cloneCollidable);
            }
        }

        private void InitializeFieldCot()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-120.0f, -2.5f, -90.0f), new Vector3(0, 0, 0), new Vector3(0.05f, 0.05f, 0.06f),
                Vector3.UnitX, Vector3.UnitY);
            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["FieldCotTexture"];

            collidableObject = new TriangleMeshObject("field cot", ActorType.CollidableDecorator, transform3D, effectParameters,
                this.modelDictionary["field-cot"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        private void InitializeFieldDesk()
        {
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject = null, cloneCollider = null;

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["FieldDeskTexture"];

            collidableObject = new CollidableObject("field desk - ", ActorType.CollidableDecorator, Transform3D.Zero, effectParameters,
                this.modelDictionary["field-desk"]);

            cloneCollider = (CollidableObject)collidableObject.Clone();
            cloneCollider.ID += 1;

            cloneCollider.Transform = new Transform3D(new Vector3(-100.0f, 0.1f, -121.0f), new Vector3(0, 90, 0), new Vector3(0.15f, 0.1f, 0.15f),
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(23, 8, 7)), new MaterialProperties(0.1f, 0.1f, 0.1f));
            cloneCollider.Enable(true, 1);
            this.objectManager.Add(cloneCollider);

            cloneCollider = (CollidableObject)collidableObject.Clone();
            cloneCollider.ID += 2;

            cloneCollider.Transform = new Transform3D(new Vector3(-70.0f, 0.1f, 70.0f), new Vector3(0, -90, 0), new Vector3(0.15f, 0.1f, 0.15f),
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.CreateRotationY(MathHelper.PiOver2), new Vector3(23, 8, 7)), new MaterialProperties(0.1f, 0.1f, 0.1f));
            cloneCollider.Enable(true, 1);
            this.objectManager.Add(cloneCollider);
        }

        private void InitializeFilingCabinet()
        {
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject = null, cloneCollider = null;

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["FilingCabinet"];

            collidableObject = new CollidableObject("filing cabinet", ActorType.Interactable | ActorType.CollidableDecorator, Transform3D.Zero, effectParameters,
                this.modelDictionary["FilingCabinet"]);
          
            //collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(4,2,4)),
            //  new MaterialProperties(1, 1, 1));
            //collidableObject.Enable(true, 1);
            //this.objectManager.Add(collidableObject);

            #region clones
            #region clones beside war table 1-3
            for (int i = 0; i < 3; i++)
            {
                cloneCollider = (CollidableObject)collidableObject.Clone();
                cloneCollider.ID = "filing cabinet";
                cloneCollider.Transform = new Transform3D(new Vector3(-125.0f, -0.4f, i * 10), new Vector3(0, 90, 0), new Vector3(0.05f, 0.05f, 0.05f),
                    Vector3.UnitX, Vector3.UnitY);
                cloneCollider.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(5, 30, 5)),
                  new MaterialProperties(1, 1, 1));
                cloneCollider.Enable(true, 1);
                this.objectManager.Add(cloneCollider);
            }
            #endregion

            #region clone 4 (2nd Room)
            cloneCollider = (CollidableObject)collidableObject.Clone();
            cloneCollider.ID = "filing cabinet";

            cloneCollider.Transform = new Transform3D(new Vector3(-10.0f, -0.4f, -68.0f), new Vector3(0, -180, 0), new Vector3(0.05f, 0.05f, 0.05f),
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(5, 30, 5)),
             new MaterialProperties(1, 1, 1));
            cloneCollider.Enable(true, 1);
            this.objectManager.Add(cloneCollider);
            #endregion

            #region clone 5 (left side of exit door)
            cloneCollider = (CollidableObject)collidableObject.Clone();
            cloneCollider.ID = "filing cabinet";

            cloneCollider.Transform = new Transform3D(new Vector3(-75.0f, -0.4f, 122.0f), new Vector3(0, -180, 0), new Vector3(0.05f, 0.05f, 0.05f),
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(5, 30, 5)),
              new MaterialProperties(1, 1, 1));
            cloneCollider.Enable(true, 1);
            this.objectManager.Add(cloneCollider);
            #endregion

            #region clone 6 (right side of exit door)
            cloneCollider = (CollidableObject)collidableObject.Clone();
            cloneCollider.ID = "filing cabinet";

            cloneCollider.Transform = new Transform3D(new Vector3(-113.0f, -0.4f, 122.0f), new Vector3(0, -180, 0), new Vector3(0.05f, 0.05f, 0.05f),
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(5, 30, 5)),
              new MaterialProperties(1, 1, 1));
            cloneCollider.Enable(true, 1);
            this.objectManager.Add(cloneCollider);
            #endregion
            #endregion
        }

        private void InitializeBookCaseDoor()
        {
            //NCMG
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-64, 0, -102), 
                new Vector3(0, 0, 0), 
                new Vector3(0.05f, 0.038f, 0.045f), 
                Vector3.UnitX, Vector3.UnitY);

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["bookcase"];

            collidableObject = new CollidableObject("bookcase door", ActorType.CollidableDoor, transform3D, effectParameters, 
                this.modelDictionary["Bookshelf_01"]);
            collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, 
                    new Vector3(8f, 30.0f, 35.0f)),
               new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            collidableObject.AttachController(new BookcaseController("Bookcase Controller", ControllerType.Rotation, this.eventDispatcher));
            this.objectManager.Add(collidableObject);
            
        }

        /*
        private void InitializeBookCaseProp()
        {
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject = null, cloneCollider = null;

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["bookcase"];

            collidableObject = new CollidableObject("Bookcase - ", ActorType.CollidableDecorator, Transform3D.Zero, effectParameters,
                this.modelDictionary["Bookshelf_01"]);

            for (int i = 0; i < 3; i++)
            {
                cloneCollider = (CollidableObject)collidableObject.Clone();
                cloneCollider.ID += 1;

                cloneCollider.Transform = new Transform3D(new Vector3(-125.0f, 0, i * 18), new Vector3(0, 0, 0), new Vector3(0.05f, 0.038f, 0.045f),
                Vector3.UnitX, Vector3.UnitY);

                cloneCollider.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.CreateRotationY(MathHelper.Pi), new Vector3(4, 10, 10)),
                new MaterialProperties(1, 1, 1));

                cloneCollider.Enable(true, 1);
                this.objectManager.Add(cloneCollider);
            }
        }
        */

        private void InitializePhonoGraph()
        {
            Transform3D transform = new Transform3D(new Vector3(-100.0f, 9.7f, -120.0f), new Vector3(0, 180, 0), new Vector3(0.02f, 0.02f, 0.02f), Vector3.UnitX, Vector3.UnitY);
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["phonograph"];

           // ModelObject model = new ModelObject("phonograph", ActorType.Interactable | ActorType.CollidableDecorator, transform, effectParameters, this.modelDictionary["Phonograph"]);
            CollidableObject collidableObject;
            collidableObject = new CollidableObject("phonograph", ActorType.CollidableDecorator | ActorType.Interactable, transform, effectParameters, this.modelDictionary["Phonograph"]);
            collidableObject.AddPrimitive(new Box(new Vector3(-100.0f, 8.7f, -120.0f),Matrix.CreateTranslation(0f, -3f, 0f),new Vector3(5,5,5)), new MaterialProperties(1,1,1));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
          //  this.objectManager.Add(model);

            AudioEmitter phonograph = new AudioEmitter();

            phonograph.Position = new Vector3(-100.0f, 7.0f, -121.0f);
            phonograph.DopplerScale = 500000f;
            phonograph.Up = Vector3.UnitY;
            phonograph.Forward = Vector3.UnitZ;

            this.soundManager.Play3DCue("game-main-soundtrack", phonograph);
            this.soundManager.PlayCue("old-computer");


        }

        private void InitializeComputer()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-40.0f, 0.0f, -68.0f), new Vector3(0, -90, 0), new Vector3(0.05f, 0.05f, 0.045f), Vector3.UnitX, Vector3.UnitY);

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["ComputerTexture"];

            collidableObject = new TriangleMeshObject("computer", ActorType.Interactable | ActorType.CollidableDecorator, transform3D, effectParameters, this.modelDictionary["computer"],
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        private void InitializeLogicPuzzleModel()
        {
            Transform3D transform = new Transform3D(new Vector3(-30.0f, 12.0f, -125.2f), new Vector3(90, -180, 180), new Vector3(0.02f, 0.02f, 0.02f), Vector3.UnitX, Vector3.UnitY);
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;

            ModelObject model = new ModelObject("Base Logic Puzzle", ActorType.Decorator, transform, effectParameters, this.modelDictionary["BaseLogicPuzzle"],StatusType.Off);
            this.objectManager.Add(model);

            

            model = new ModelObject("Simple Logic Puzzle", ActorType.Decorator, transform, effectParameters, this.modelDictionary["SimpleLogicPuzzle"],StatusType.Off);
            this.objectManager.Add(model);

            model = new ModelObject("Hard Logic Puzzle", ActorType.Decorator, transform, effectParameters, this.modelDictionary["HardLogicPuzzle"],StatusType.Off);
            this.objectManager.Add(model);
        }

        private void InitializeRiddleAnswerObject()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;
            string id = "riddle Answer";

            if(this.riddleId == 1)
            {
                transform3D = AppData.gunTransform;
                id = "gun";
            }
            else if(this.riddleId == 2)
            {
                transform3D = AppData.ClockTransform;
                id = "clock";
            }
            else
            {
                transform3D = AppData.PhoneTransform;
                id = "phone";
            }
            

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["riddleObjTexture"];

            collidableObject = new TriangleMeshObject(id, ActorType.Interactable | ActorType.CollidableDecorator, transform3D, effectParameters, 
                this.modelDictionary["riddleAnswerObj"], new MaterialProperties(0.5f, 0.5f, 0.5f));
            collidableObject.Enable(true, 1);

            this.objectManager.Add(collidableObject);
        }

        private void InitializeWinVolumeBox()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-91, 0, 130), Vector3.Zero, new Vector3(10, 0.1f, 2), Vector3.UnitX, Vector3.UnitY);
            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["concreteFloor"];

            collidableObject = new ImmovablePickupObject("win trigger volume", ActorType.Objective, transform3D, effectParameters, this.modelDictionary["box2"],
                this.modelDictionary["box2"], new MaterialProperties(0.1f, 0.1f, 0.1f), new PickupParameters("win trigger volume", 1),this.eventDispatcher);
            collidableObject.Enable(true, 1);

            this.objectManager.Add(collidableObject);
        }
        
        /*
         * Author: Cameron
         * Model taken from https://www.turbosquid.com/FullPreview/Index.cfm/ID/719267
         */
        private void InitialiseWineBottles()
        {
            Transform3D transform = new Transform3D(new Vector3(-106.8f, 4.5f, 35), 
                new Vector3(0, 0, 0), 
                new Vector3(0.003f), 
                Vector3.UnitX, Vector3.UnitY);
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;


            CollidableObject collidableObject = new CollidableObject("wine bottle",ActorType.CollidableDecorator | ActorType.Interactable,transform,effectParameters ,this.modelDictionary["wine-bottle"]);
            collidableObject.AddPrimitive(new Box(Vector3.UnitY, Matrix.Identity, new Vector3(2, 7, 2)), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }
        
        /*
         * Author: Cameron
         * Taken from https://www.turbosquid.com/FullPreview/Index.cfm/ID/726051
         */
        private void InitialiseGlobe()
        {
            Transform3D transform = new Transform3D(new Vector3(-106, 6.7f, -6), 
                new Vector3(0, 90, 0), 
                new Vector3(0.2f), 
                Vector3.UnitX, Vector3.UnitY);
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["mp"];
            CollidableObject collidable = new CollidableObject("globe",ActorType.CollidableDecorator | ActorType.Interactable,transform,effectParameters, this.modelDictionary["globe"]);
            collidable.AddPrimitive(new Box((new Vector3(0,500,0)),Matrix.CreateTranslation(new Vector3(0, -2.5f, 0)),new Vector3(3,5,3)), new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidable.Enable(true, 1);
            this.objectManager.Add(collidable);
        }
        
        /*
         * Author: Cameron
         * Taken from https://www.turbosquid.com/FullPreview/Index.cfm/ID/758204
         */
        private void InitialiseGrenade()
        {
            Transform3D transform = new Transform3D(new Vector3(-68, 0.5f, 35), 
                new Vector3(0, 90, 0), 
                new Vector3(0.05f), 
                Vector3.UnitX, Vector3.UnitY);
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["grenade"];

            CollidableObject archetype = new CollidableObject("grenade", ActorType.CollidableDecorator | ActorType.Interactable, transform, effectParameters, this.modelDictionary["grenade"]);
           

            CollidableObject clone = (CollidableObject)archetype.Clone();
            clone.ID = "grenade";
            clone.AddPrimitive(new Box(archetype.Transform.Translation, Matrix.Identity, new Vector3(1, 2, 5)), new MaterialProperties(0.2f, 0.8f, 0.7f));
            clone.Enable(true, 1);
            this.objectManager.Add(clone);

            clone = (CollidableObject)archetype.Clone();
            clone.Transform.TranslateBy(new Vector3(1, 0, -0.1f));
            clone.ID = "grenade";
            clone.Transform.RotateAroundYBy(2);
            clone.Transform.RotateAroundZBy(10);
            clone.AddPrimitive(new Box(archetype.Transform.Translation, Matrix.Identity, new Vector3(1, 2, 5)), new MaterialProperties(0.2f, 0.8f, 0.7f));
            clone.Enable(true, 1);
            this.objectManager.Add(clone);
            
            clone = (CollidableObject)archetype.Clone();
            clone.Transform.TranslateBy(new Vector3(2, 0, 0.1f));
            clone.Transform.RotateAroundYBy(-2);
            clone.Transform.RotateAroundZBy(30);
            clone.ID = "grenade";
            clone.AddPrimitive(new Box(archetype.Transform.Translation, Matrix.Identity, new Vector3(1, 2, 5)), new MaterialProperties(0.2f, 0.8f, 0.7f));
            clone.Enable(true, 1);
            this.objectManager.Add(clone);
            
            clone = (CollidableObject)archetype.Clone();
            clone.Transform.TranslateBy(new Vector3(-1, 0, -0.1f));
            clone.Transform.RotateAroundYBy(1);
            clone.ID = "grenade";
            clone.Transform.RotateAroundZBy(90);
            clone.AddPrimitive(new Box(archetype.Transform.Translation, Matrix.Identity, new Vector3(1, 2, 5)), new MaterialProperties(0.2f, 0.8f, 0.7f));
            clone.Enable(true, 1);
            this.objectManager.Add(clone);
        }
        
        /*
         * Author: Cameron
         * Taken from https://www.turbosquid.com/FullPreview/Index.cfm/ID/672646
         */
        private void InitialiseHelmet()
        {
            Transform3D transform = new Transform3D(new Vector3(-120, 1, -100), 
                new Vector3(0, 90, 0), 
                new Vector3(1), 
                Vector3.UnitX, Vector3.UnitY);
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["helmet"];

            CollidableObject collidableObject = new CollidableObject("helmet", ActorType.CollidableDecorator |ActorType.Interactable, transform, effectParameters, this.modelDictionary["helmet"]);
            collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(2, 2, 2)), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }
        
        /*
         * Author: Cameron
         */
        private void InitialiseHat()
        {
            Transform3D transform = new Transform3D(new Vector3(-106, 6.6f, -120), 
                new Vector3(0, 0, 0), 
                new Vector3(0.01f), 
                Vector3.UnitX, Vector3.UnitY);
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["hat"];


            CollidableObject collidableObject = new CollidableObject("hat", ActorType.CollidableDecorator | ActorType.Interactable, transform, effectParameters, this.modelDictionary["hat"]);
            collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(2, 2, 2)), new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }
        
        /*
         * Author: Cameron
         *
        private void InitialisePhone()
        {
            Transform3D transform = new Transform3D(new Vector3(-94, 6.6f, -120), 
                new Vector3(0, 0, 0), 
                new Vector3(0.01f), 
                Vector3.UnitX, Vector3.UnitY);
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["phone"];

            ModelObject model = new ModelObject("Phone", ActorType.Decorator, transform, effectParameters, 
                this.modelDictionary["phone"]);
            this.objectManager.Add(model);
        }
        */
        
        /*
        * Author: Cameron
        */
        private void InitialiseShelf()
        {
            Transform3D transform = new Transform3D(new Vector3(-66, 8, -50), 
                new Vector3(0, -90, 0), 
                new Vector3(0.05f), 
                Vector3.UnitX, Vector3.UnitY);
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["wood"];

            ModelObject model = new ModelObject("Shelf", ActorType.Decorator, transform, effectParameters, 
                this.modelDictionary["shelf"]);
            this.objectManager.Add(model);

            #region Right Wall

            ModelObject clone = (ModelObject)model.Clone();
            clone.Transform.TranslateBy(new Vector3(0, 0, 30));
            this.objectManager.Add(clone);
            
            clone = (ModelObject)clone.Clone();
            clone.Transform.TranslateBy(new Vector3(0, 0, 30));
            this.objectManager.Add(clone);
            
            clone = (ModelObject)clone.Clone();
            clone.Transform.TranslateBy(new Vector3(0, 0, 30));
            this.objectManager.Add(clone);

            #endregion

            #region Left Wall

            clone = (ModelObject)model.Clone();
            clone.Transform.RotateAroundYBy(180);
            clone.Transform.TranslateBy(new Vector3(-57, 0, 0));
            this.objectManager.Add(clone);
            
            clone = (ModelObject)clone.Clone();
            clone.Transform.TranslateBy(new Vector3(0, 0, 30));
            this.objectManager.Add(clone);
            
            clone = (ModelObject)clone.Clone();
            clone.Transform.TranslateBy(new Vector3(0, 0, 60));
            this.objectManager.Add(clone);

            #endregion
            
        }
        
        private void InitializeMap()
        {
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            Transform3D transform = new Transform3D(new Vector3(-70.0f, 6.82f, 70.0f), new Vector3(0, -90, 0), new Vector3(5.0f, 0.0001f, 2.5f), Vector3.UnitX, Vector3.UnitY);
            effectParameters.Texture = this.textureDictionary["map"];

            ModelObject model = new ModelObject("map", ActorType.Decorator, transform, effectParameters, this.modelDictionary["box2"]);
            this.objectManager.Add(model);
        }

        private void InitializeCrates()
        {
            BasicEffectParameters effectParameters;
            CollidableObject collidable = null, clone = null;

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["crate1"];

            collidable = new CollidableObject("crate - ", ActorType.CollidableDecorator, Transform3D.Zero, effectParameters, this.modelDictionary["box2"]);

            for(int i = 0; i < 3; i++)
            {
                clone = (CollidableObject)collidable.Clone();
                clone.ID += i;

                clone.Transform = new Transform3D(new Vector3(-123, 2.48f + (i * 5.1f), 88), new Vector3(2, 2, 2));
                clone.AddPrimitive(new Box(clone.Transform.Translation, Matrix.Identity, new Vector3(3.3f,2,3.3f)), new MaterialProperties(0.1f, 0.1f, 0.1f));
                clone.Enable(true, 1);
                this.objectManager.Add(clone);
            }

            for (int i = 0; i < 3; i++)
            {
                clone = (CollidableObject)collidable.Clone();
                clone.ID += i + 3;

                clone.Transform = new Transform3D(new Vector3(-123, 2.48f + (i * 5.1f), 60), new Vector3(2, 2, 2));
                clone.AddPrimitive(new Box(clone.Transform.Translation, Matrix.Identity, new Vector3(3.3f, 2, 3.3f)), new MaterialProperties(0.1f, 0.1f, 0.1f));
                clone.Enable(true, 1);
                this.objectManager.Add(clone);
            }
        }
        #endregion

        #region Initialize Cameras
        private void InitializeCamera(Integer2 screenResolution, string id, Viewport viewPort, Transform3D transform, IController controller, float drawDepth)
        {
            Camera3D camera = new Camera3D(id, ActorType.Camera, transform, ProjectionParameters.StandardMediumFiveThree, viewPort, drawDepth, StatusType.Update);

            if (controller != null)
                camera.AttachController(controller);

            this.cameraManager.Add(camera);
        }

        private void InitializeCutsceneCameras()
        {
            Camera3D cloneCamera = null, camera = null;
            string id = "Door Cutscene Camera";
            string viewportDictionaryKey = "full viewport";
            float drawDepth = 0;

            camera = new Camera3D(id, ActorType.Camera, new Transform3D(new Vector3(-70, 24, 40),
                new Vector3(-0.25f, -0.25f, 1), Vector3.UnitY),
                   ProjectionParameters.StandardDeepSixteenNine, this.viewPortDictionary[viewportDictionaryKey], drawDepth, StatusType.Update);

            this.cameraManager.Add(camera);

            cloneCamera = null;

            cloneCamera = new Camera3D("Door Cutscene Camera2", ActorType.Camera, new Transform3D(new Vector3(-120, 24, -70),
                new Vector3(1, -0.25f, -0.4f), Vector3.UnitY),
                   ProjectionParameters.StandardDeepSixteenNine, this.viewPortDictionary[viewportDictionaryKey], drawDepth, StatusType.Update);

            this.cameraManager.Add(cloneCamera);
        }

        private void InitializeCollidableFirstPersonDemo(Integer2 screenResolution)
        {
            Transform3D transform = null;
            string id = "";
            string viewportDictionaryKey = "";
            float drawDepth = 0;

            id = "collidable first person camera";
            viewportDictionaryKey = "full viewport";
            //doesnt matter how high on Y-axis we start the camera since it's collidable and will fall until the capsule toches the ground plane - see AppData::CollidableCameraViewHeight
            //just ensure that the Y-axis height is slightly more than AppData::CollidableCameraViewHeight otherwise the player will rise eerily upwards at the start of the game
            //as the CDCR system pushes the capsule out of the collidable ground plane 
            transform = new Transform3D(new Vector3(-98, 1.1f * AppData.CollidableCameraViewHeight, 104), -Vector3.UnitZ, Vector3.UnitY);

            Camera3D camera = new Camera3D(id, ActorType.Camera, transform,
                    ProjectionParameters.StandardDeepSixteenNine, this.viewPortDictionary[viewportDictionaryKey], drawDepth, StatusType.Update);

            //attach a CollidableFirstPersonController
            camera.AttachController(new CollidableFirstPersonCameraController(
                    camera + " controller",
                    ControllerType.CollidableFirstPerson,
                    AppData.CameraMoveKeys,
                    this.soundManager,
                    AppData.CollidableCameraMoveSpeed, AppData.CollidableCameraStrafeSpeed, AppData.CameraRotationSpeed,
                    this.managerParameters,
                    eventDispatcher,
                    camera, //parent
                    AppData.CollidableCameraCapsuleRadius,
                    AppData.CollidableCameraViewHeight,
                    1, 1, //accel, decel
                    AppData.CollidableCameraMass,
                    AppData.CollidableCameraJumpHeight,
                    Vector3.Zero)); //translation offset

            this.cameraManager.Add(camera);
        }

        #endregion

        #region Events
        private void InitializeEventDispatcher()
        {
            //initialize with an arbitrary size based on the expected number of events per update cycle, increase/reduce where appropriate
            this.eventDispatcher = new EventDispatcher(this, 20);

            //dont forget to add to the Component list otherwise EventDispatcher::Update won't get called and no event processing will occur!
            Components.Add(this.eventDispatcher);
        }

        private void StartGame()
        {
            //will be received by the menu manager and screen manager and set the menu to be shown and game to be paused
            EventDispatcher.Publish(new EventData(EventActionType.OnPause, EventCategoryType.MainMenu));
            
            

            //publish an event to set the camera
            object[] additionalEventParamsB = { "collidable first person camera 1" };
            EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalEventParamsB));
            //we could also just use the line below, but why not use our event dispatcher?
            //this.cameraManager.SetActiveCamera(x => x.ID.Equals("collidable first person camera 1"));
        }

        /*
        * Any Events That are to be initialised in main will happen in here
        */
        private void InitializeEvents()
        {
            this.eventDispatcher.InteractChanged += Interactive;
            this.eventDispatcher.PuzzleChanged += ChangeLights;
            this.eventDispatcher.RiddleChanged += ChangePopUPState;
            this.eventDispatcher.RiddleChanged += changeActorType;
            this.eventDispatcher.PlayerChanged += LoseTriggered;
            this.eventDispatcher.PlayerWinChanged += WinTriggered;
            this.eventDispatcher.PopUpChanged += ChangePopUPState;
            this.eventDispatcher.RiddleAnswerChanged += ChangeRiddleState;
            this.eventDispatcher.Reset += Reset;
            this.eventDispatcher.VolumeChanged += ChangeVolume;
            this.eventDispatcher.animationTriggered += playAnimationSound;
            this.eventDispatcher.MessageChanged += interactMessage;

        }


        private void interactMessage(EventData eventData)
        {

            GameTime gameTime = eventData.AdditionalParameters[1] as GameTime;
            int seconds = gameTime.TotalGameTime.Seconds;
            CollidableObject obj = eventData.AdditionalParameters[0] as CollidableObject;
            Predicate<Actor2D> predicate = s => s.GetID() == "message";
            Predicate<Actor2D> predicate2 = s => s.GetID() == "message2";
            Actor2D actor = uiManager.Find(predicate);
            Actor2D actor2 = uiManager.Find(predicate2);
            string message = "A " + obj.ID + " this might be useful later. ";

          


            if (actor == null)
            {
                int second = gameTime.TotalGameTime.Seconds;
                Vector2 translation = new Vector2(graphics.PreferredBackBufferWidth/60, graphics.PreferredBackBufferHeight - graphics.PreferredBackBufferHeight / 20);
                Vector2 translation2 = translation + new Vector2(-3, 0);
                Vector2 scale = new Vector2(1f, 1f);
                Vector2 scale2 = new Vector2(1f, 1f);
                Vector2 origin = new Vector2(0, 0);
                Integer2 dimensions = new Integer2(10, 10);
                Transform2D transform = new Transform2D(translation, 0, scale, origin, dimensions);
                Transform2D transform2 = new Transform2D(translation2, 0, scale2, origin, dimensions);
                SpriteFont font = this.fontDictionary["timerFont"];


                UITextObject UImessage = new UITextObject("message", ActorType.UIDynamicText, StatusType.Drawn | StatusType.Update, transform, Color.White, SpriteEffects.None, 0, message, font, second);
                UITextObject UImessage2 = new UITextObject("message2", ActorType.UIDynamicText, StatusType.Drawn | StatusType.Update, transform2, Color.Black, SpriteEffects.None, 1, message, font, second);


                uiManager.Add(UImessage);
                uiManager.Add(UImessage2);
            }
            else
            {
                UITextObject UImessage = actor as UITextObject;
                UITextObject UImessage2 = actor2 as UITextObject;



                UImessage.Text = message;
                UImessage2.Text = message;
                UImessage.SecondCreated = seconds;
                UImessage2.SecondCreated = seconds;
                UImessage.StatusType = StatusType.Drawn | StatusType.Update;
                UImessage2.StatusType = StatusType.Drawn | StatusType.Update;
            }

            



        }





        private void playAnimationSound(EventData eventData)
        {

            if(eventData.EventType == EventActionType.OpenBookcase)
            {
                this.soundManager.PlayCue("Bookcase_Sound");
            }
            else if(eventData.EventType == EventActionType.OpenDoor)
            {
                this.soundManager.PlayCue("Door_Open");
            }
        }
        /*
         * Author: Tomas
         * Object is retrieved from the event and its texture is changed based on what current texture is
         */
        private void Interactive(EventData eventData)
        {
            CollidableObject actor = eventData.Sender as CollidableObject;

            if (actor.EffectParameters.Texture == this.textureDictionary["green"])
            {
                actor.EffectParameters.Texture = this.textureDictionary["gray"];
            }
            else
            {
                actor.EffectParameters.Texture = this.textureDictionary["green"];
            }
            this.logicPuzzle.changeState(actor.ID);

        }

        private void ChangeVolume(EventData eventData)
        {

           float delta = (float)eventData.AdditionalParameters.GetValue(0);
            this.soundManager.ChangeVolume(delta,"Default");

            Predicate<UIObject> predicate = s => s.GetID() == "audio-bar";
        
            UITextureObject bar = this.menuManager.ActiveList.Find(predicate) as UITextureObject;

            if (delta < 0 && bar.SourceRectangleWidth != 0)
            {
                bar.SourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, bar.SourceRectangleWidth - bar.Texture.Width / 4, bar.SourceRectangleHeight);
            }
            else if (delta > 0 && bar.SourceRectangleWidth < bar.Texture.Width)
            {
                bar.SourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, bar.SourceRectangleWidth + bar.Texture.Width / 4, bar.SourceRectangleHeight);
            }





        }



        /**
         * Author: Tomas 
         * Used to change the color of lights similar to interactive method but does not 
         * pass the object via event data it must find it via predicate
         */
        private void ChangeLights(EventData eventData)
        {
            string id = (string)eventData.AdditionalParameters[0];
            Predicate<Actor3D> predicate = s => s.GetID() == id;
            CollidableObject gate = (CollidableObject)this.objectManager.Find(predicate);
            if (gate.EffectParameters.Texture == this.textureDictionary["gray"])
            {
                gate.EffectParameters.Texture = this.textureDictionary["green"];
            }
            else
            {
                gate.EffectParameters.Texture = this.textureDictionary["gray"];
            }

        }

        /*
         * Author: Tomas 
         *  changes the state of the pop up from down to up also dispatchers an objective to objective manager
         */
        private void ChangePopUPState(EventData eventData)
        {
            Predicate<Actor2D> pred = s => s.ActorType == ActorType.PopUP;
            UITextureObject item = this.uiManager.Find(pred) as UITextureObject;


            if (item.StatusType == StatusType.Off)
            {
                item.StatusType = StatusType.Drawn;


                if (objectiveManager.getCurrentObjective() == 1)
                {

                    EventDispatcher.Publish(new EventData(EventActionType.OnObjective, EventCategoryType.Objective));

                }

            }
            else
            {

                item.StatusType = StatusType.Off;
            }

        }

        /*
         * Author : Andrew
         *changes the actor type of the riddle answer object to collidable pickup 
         */
        private void changeActorType(EventData eventData)
        {
            Predicate<Actor3D> pred = s => s.ID == "Riddle Answer";
            if (this.riddleId == 1) { pred = s => s.ID == "gun"; }
            if (this.riddleId == 2) { pred = s => s.ID == "clock"; }
            if (this.riddleId == 3) { pred = s => s.ID == "phone"; }

            Actor3D item = this.objectManager.Find(pred) as Actor3D;
            item.ActorType = ActorType.CollidablePickup;
        }

        /*
         * Author : Andrew
         *switches the camera to a cutscene camera when the riddle answer object is picked up
         */
        private void ChangeRiddleState(EventData eventData)
        {
            Predicate<Actor3D> pred = s => s.ID == "Riddle Answer";
            if (this.riddleId == 1) { pred = s => s.ID == "gun"; }
            if (this.riddleId == 2) { pred = s => s.ID == "clock"; }
            if (this.riddleId == 3) { pred = s => s.ID == "phone"; }
            Actor3D item = this.objectManager.Find(pred) as Actor3D;

            item.StatusType = StatusType.Off;

            EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, new object[] { "Door Cutscene Camera2" }));
            EventDispatcher.Publish(new EventData(EventActionType.RiddleSolved, EventCategoryType.RiddleAnswer));
            EventDispatcher.Publish(new EventData(EventActionType.OnObjective, EventCategoryType.Objective));
            EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Cutscene, new object[] { 5, "collidable first person camera" }));
        }

        /*
         * Author: Cameron
         * This will be used to trigger different UI effects when the timer runs out
         */
        private void LoseTriggered(EventData eventData)
        {
            object[] addParams = { "lose" };
            EventDispatcher.Publish(new EventData(EventActionType.OnLose, EventCategoryType.MainMenu));
            EventDispatcher.Publish(new EventData(EventActionType.OnLose, EventCategoryType.mouseLock));
            EventDispatcher.Publish(new EventData(EventActionType.OnLose, EventCategoryType.Sound2D, addParams));
            this.soundManager.Pause3DCue("game-main-soundtrack");
        }

        /*
         * Author: Cameron
         * This will be used to trigger the end screen with the door opening and a fade to black
         */
        private void WinTriggered(EventData eventData)
        {
            object[] addParams = { "victory" };
            EventDispatcher.Publish(new EventData(EventActionType.OnWin, EventCategoryType.MainMenu));
            EventDispatcher.Publish(new EventData(EventActionType.OnPause, EventCategoryType.MainMenu));
            EventDispatcher.Publish(new EventData(EventActionType.OnWin, EventCategoryType.mouseLock));
            EventDispatcher.Publish(new EventData(EventActionType.OnWin, EventCategoryType.Sound2D, addParams));
            this.soundManager.Pause3DCue("game-main-soundtrack");
        }

        /**
        * Author: Tomas
        * used to reset all objects and cameras
        */
        private void Reset(EventData eventData)
        {
            resetFPCamera();

            resetLogicPuzzleModels();
            resetRiddleAnswer();
            resetLoseTimer();
        }
        #endregion

        #region Reset Functions

        /**
         * Author: Tomas
         * Resets logic puzzle models to default state
         */ 
        private void resetLogicPuzzleModels()
        {
            #region Reset Switches
            for(int i = 1; i < 5;i++)
            {
                Predicate<Actor3D> pred = s => s.ID == "switch-"+i;
                CollidableObject logicSwitch = (CollidableObject) this.objectManager.Find(pred);

                logicSwitch.EffectParameters.Texture = this.textureDictionary["gray"];
            }
            #endregion

            int num = logicID;
            switch(num)
            {
                case 1:
                    resetBasePuzzle();
                    break;
                case 2:
                    resetSimplePuzzle();
                    break;
                case 3:
                    resetHardPuzzle();
                    break;
                default:
                    resetBasePuzzle();
                    break;
            }

            logicModelStatusChanger(num, StatusType.Off);
            
            InitialiseLogicPuzzle();
        }


        private void resetBasePuzzle()
        {
            for (int i = 1; i < 6; i++)
            {
                Predicate<Actor3D> pred = y => y.ID == "base-gate-" + i;
                CollidableObject logicGate = (CollidableObject)this.objectManager.Find(pred);
                logicGate.EffectParameters.Texture = this.textureDictionary["gray"];
            }
        }

        private void resetSimplePuzzle()
        {
            for (int i = 1; i < 6; i++)
            {
                Predicate<Actor3D> pred = y => y.ID == "simple-gate-" + i;
                CollidableObject logicGate = (CollidableObject)this.objectManager.Find(pred);
                logicGate.EffectParameters.Texture = this.textureDictionary["gray"];
            }
        }

        private void resetHardPuzzle()
        {
            for (int i = 1; i < 7; i++)
            {
                Predicate<Actor3D> pred = y => y.ID == "hard-gate-" + i;
                CollidableObject logicGate = (CollidableObject)this.objectManager.Find(pred);
                logicGate.EffectParameters.Texture = this.textureDictionary["gray"];
            }
        }
        /**
         * Author: Tomas
         * used to reset Answer Object to Collidable prop
         */
        private void resetRiddleAnswer()
        {
            Predicate<Actor3D> pred = s => s.ID == "Riddle Answer";
            if (this.riddleId == 1) { pred = s => s.ID == "gun"; }
            if (this.riddleId == 2) { pred = s => s.ID == "clock"; }
            if (this.riddleId == 3) { pred = s => s.ID == "phone"; }
            Actor3D item = this.objectManager.Find(pred) as Actor3D;
            item.ActorType = ActorType.CollidableProp;
            item.StatusType = StatusType.Drawn;
        }

        /**
         * Author: Tomas
         * Resets The Fps camera by creating a new one and setting it as the active camera
         * this helped avoid a bug where original camera would not reset properly
         */
        private void resetFPCamera()
        {
           Integer2 screenResolution = ScreenUtility.HD720;
            Predicate<Camera3D> pred = s => s.ID == "collidable first person camera";
            

            Camera3D camera = this.cameraManager.find(pred);
            camera.ID = "x";
            //Predicate<Camera3D> predicate = s => s.ID == "x";
            //this.cameraManager.Remove(predicate);

            InitializeCollidableFirstPersonDemo(screenResolution);


            this.cameraManager.SetActiveCamera(pred);
            
        }

        
        /*
         * Author: Cameron
         * This will be used to reset the lose timer to its default value when reset is called
         */
        private void resetLoseTimer()
        {
            foreach (TimerUtility timer in timerManager.TimerList)
            {
                if (timer.ID.Equals(AppData.LoseTimerID))
                {
                    timer.Hours = AppData.LoseTimerHours;
                    timer.Minutes = AppData.LoseTimerMinutes;
                    timer.Seconds = AppData.LoseTimerSeconds;
                }
            }
        }
        

        #endregion
        #region Menu & UI

        private void AddMenuElements()
        {
            Transform2D transform = null;
            Texture2D texture = null;
            Vector2 position = Vector2.Zero;
            UIButtonObject uiButtonObject = null, clone = null;
            string sceneID = "", buttonID = "", buttonText = "";
            int verticalBtnSeparation = 105;

            #region Main Menu
            sceneID = "main menu";

            //retrieve the background texture
            texture = this.textureDictionary["Title-screen"];
            //scale the texture to fit the entire screen
            Vector2 scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);

            this.menuManager.Add(sceneID, new UITextureObject("mainmenuTexture", ActorType.UIStaticTexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));

            //add start button
            buttonID = "startbtn";
            texture = this.textureDictionary["start"];
            position = new Vector2(graphics.PreferredBackBufferWidth - texture.Width ,  texture.Height*2.5f);
            transform = new Transform2D(position,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.Gray, SpriteEffects.None, 0.1f, texture, buttonText,
                this.fontDictionary["menu"],
                Color.DarkBlue, new Vector2(0, 0));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));

            this.menuManager.Add(sceneID, uiButtonObject);

            //add exit button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "exitbtn";
            clone.Texture = this.textureDictionary["quit"];
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, verticalBtnSeparation*3);
            //change the texture blend color
            clone.Color = Color.Gray;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));
            this.menuManager.Add(sceneID, clone);
            
            clone = null;
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "controlsbtn";
            clone.Texture = this.textureDictionary["controls"];
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(-clone.Texture.Width/5, verticalBtnSeparation);
            clone.SourceRectangle = new Microsoft.Xna.Framework.Rectangle(0,0, clone.Texture.Width, clone.Texture.Height);

            //change the texture blend color
            clone.Color = Color.Gray;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));
            this.menuManager.Add(sceneID, clone);

            clone = null;
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "audiobtn";
            clone.Texture = this.textureDictionary["Audio"];
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(-clone.Texture.Width/8, verticalBtnSeparation*2);
            clone.SourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, clone.Texture.Width, clone.Texture.Height);

            //change the texture blend color
            clone.Color = Color.Gray;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));
            this.menuManager.Add(sceneID, clone);
            #endregion


            //Audio

            #region Pause Menu
            sceneID = "pause menu";

            texture = this.textureDictionary["PauseMenu"];
            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);

            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("pauseMenuTexture", ActorType.UIStaticTexture, StatusType.Drawn,
                transform, Color.White, SpriteEffects.None, 1, texture));

            clone = null;
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.Texture = this.textureDictionary["Resume"];
            clone.ID = "resumebtn";
            clone.SourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, clone.Texture.Width*1, clone.Texture.Height*1);

            clone.Transform.Translation = new Vector2(graphics.PreferredBackBufferWidth /2, graphics.PreferredBackBufferHeight/3);
            clone.SourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, clone.Texture.Width, clone.Texture.Height);



            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                  new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));
            this.menuManager.Add(sceneID, clone);
            clone = null;

            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "exitbtn";
            clone.Texture = this.textureDictionary["quit"];
           
            clone.Transform.Translation = new Vector2(graphics.PreferredBackBufferWidth / 2 + clone.Texture.Width/7, graphics.PreferredBackBufferHeight / 2);
            clone.SourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, clone.Texture.Width, clone.Texture.Height);


            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));
 
            this.menuManager.Add(sceneID, clone);

            #endregion

            #region Controls Menu

            sceneID = "controls menu";

            texture = this.textureDictionary["ControlMenu"];
            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);

            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("controlMenuTexture", ActorType.UIStaticTexture, StatusType.Drawn,
                transform, Color.White, SpriteEffects.None, 1, texture));

            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "backbtn";
            clone.Texture = this.textureDictionary["back"];
            //move down on Y-axis for next button
            clone.Transform.Translation = new Vector2(graphics.PreferredBackBufferWidth/2, clone.Texture.Height/2);
            clone.SourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, clone.Texture.Width, clone.Texture.Height);
            clone.Transform = new Transform2D(clone.Transform.Translation,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(clone.Texture.Width / 2.0f, clone.Texture.Height / 2.0f), new Integer2(clone.Texture.Width, clone.Texture.Height));
            //change the texture blend color
            clone.Color = Color.Gray;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));
            this.menuManager.Add(sceneID, clone);

            #endregion

            #region Story menu
            sceneID = "story menu";
            texture = this.textureDictionary["mission-brief"];
            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);

            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("storyMenuTexture", ActorType.UIStaticTexture, StatusType.Drawn, transform, Color.White, SpriteEffects.None, 1, texture));

            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "playbtn";
            clone.Texture = this.textureDictionary["start"];
            //clone.SourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, clone.Texture.Width, clone.Texture.Height);
            clone.Transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight * 1.2f),
                0, new Vector2(0.8f, 0.8f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width/4, texture.Height));
            clone.Color = Color.Gray;
            clone.OriginalColor = clone.Color;
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));
            this.menuManager.Add(sceneID, clone);
            #endregion

        }

        /**
         * Author: Tomas
         * Simple Game Over Menu
         */
        private void AddGameOverMenu()
        {
            string sceneID, buttonID, buttonText;
            Vector2 position = Vector2.Zero;
            UIButtonObject uiButtonObject = null, clone = null;
            int verticalBtnSeparation = 100;
            int w, h;

            w = graphics.PreferredBackBufferWidth;
            h = graphics.PreferredBackBufferHeight;
            float a, b,c,d;

            Texture2D texture = this.textureDictionary["game-over"];

            a = (float) w/texture.Width;
            b = (float)h/texture.Height;
            c = (float)1 / a;
            d = (float)1 / b;
            
            Console.WriteLine("width "+w);
            Console.WriteLine("height "+h );
            Vector2 scale = new Vector2(a,b);

            Transform2D transform = new Transform2D(new Vector2(0, 0), 0,scale, Vector2.One, new Integer2(1, 1));


            Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(0, 0, w, h);
            
            UITextureObject picture = new UITextureObject("lose-screen-background", ActorType.UIStaticTexture, StatusType.Drawn, transform, Color.White,
                SpriteEffects.None, 1, texture);


            sceneID = "lose-screen";
            this.menuManager.Add(sceneID,picture);

            texture = this.textureDictionary["restart-Button"];
            buttonID = "restart-Button";
            buttonText = "";
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f, graphics.PreferredBackBufferHeight - texture.Height);
            transform = new Transform2D(position,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.Gray, SpriteEffects.None, 0.1f, texture, buttonText,
                this.fontDictionary["menu"],
                Color.DarkBlue, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));

            this.menuManager.Add(sceneID, uiButtonObject);



            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "exitbtn";
            clone.Texture = this.textureDictionary["quit"];
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(180, verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.Gray;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));
            this.menuManager.Add(sceneID, clone);

            

        }

        /**
        * Author: Tomas
        * Simple Win Menu
        */
        private void AddWinMenu()
        {

            string sceneID, buttonID, buttonText;
            Vector2 position = Vector2.Zero;
            UIButtonObject uiButtonObject = null, clone = null;
            int verticalBtnSeparation = 100;
            int w, h;

            w = graphics.PreferredBackBufferWidth;
            h = graphics.PreferredBackBufferHeight;
            float a, b, c, d;

            Texture2D texture = this.textureDictionary["win-screen"];

            a = (float)w / texture.Width;
            b = (float)h / texture.Height;
            c = (float)1 / a;
            d = (float)1 / b;

            Vector2 scale = new Vector2(a, b);

            Transform2D transform = new Transform2D(new Vector2(0, 0), 0, scale, Vector2.One, new Integer2(1, 1));


            Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(0, 0, w, h);

            UITextureObject picture = new UITextureObject("win-screen-background", ActorType.UIStaticTexture, StatusType.Drawn, transform, Color.White,
                SpriteEffects.None, 1, texture);


            sceneID = "win-screen";
            this.menuManager.Add(sceneID, picture);

            texture = this.textureDictionary["restart-Button"];
            buttonID = "restart-Button";
            buttonText = "";
            float num = texture.Height / 2;
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f, (graphics.PreferredBackBufferHeight / 2) + num);
            transform = new Transform2D(position,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.Gray, SpriteEffects.None, 0.1f, texture, buttonText,
                this.fontDictionary["menu"],
                Color.DarkBlue, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));

            this.menuManager.Add(sceneID, uiButtonObject);

            buttonID = "main-Menu";
            texture = this.textureDictionary["mainMenu-Button"];
            position += new Vector2(-5, verticalBtnSeparation);
            transform = new Transform2D(position,
                0, new Vector2(0.6f, 0.6f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.Gray, SpriteEffects.None, 0.1f, texture, buttonText,
                this.fontDictionary["menu"],
                Color.DarkBlue, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));

            this.menuManager.Add(sceneID, uiButtonObject);


            buttonID = "exitbtn";
            texture = this.textureDictionary["quit"];
            position += new Vector2(-0, verticalBtnSeparation);
            transform = new Transform2D(position,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.Gray, SpriteEffects.None, 0.1f, texture, buttonText,
                this.fontDictionary["menu"],
                Color.DarkBlue, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));

            this.menuManager.Add(sceneID, uiButtonObject);

        }

        private void addAudioMenu()
        {
            string buttonID;
            string sceneID = "audio menu";
            Transform2D transform = null;
            Texture2D texture = null;
            Vector2 position = Vector2.Zero;
            UIButtonObject uiButtonObject = null;
            Vector2 scale = Vector2.Zero;

            texture = this.textureDictionary["audioMenu"];
            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);

            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("audioMenu", ActorType.UIStaticTexture, StatusType.Drawn,
                transform, Color.White, SpriteEffects.None, 1, texture));


            texture = this.textureDictionary["back"];
            buttonID = "backbtn";
            string buttonText = "";

            position = new Vector2(graphics.PreferredBackBufferWidth / 2 , texture.Height/1.5f );
            scale = new Vector2(0.8f,0.8f);
            Integer2 dimensions = new Integer2(texture.Width, texture.Height);
            Vector2 origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);

            transform = new Transform2D(position,0,scale, origin, dimensions);

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn, transform, Color.White, SpriteEffects.None, 0f, texture,
                buttonText, this.fontDictionary["menu"],Color.Transparent,Vector2.Zero);
          

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));
            this.menuManager.Add(sceneID, uiButtonObject);



            texture = this.textureDictionary["volUp"];
            buttonID = "volumeUpbtn";
            buttonText = "";

            position = new Vector2(graphics.PreferredBackBufferWidth / 1.3f, texture.Height );
            scale = new Vector2(0.3f, 0.3f);
            dimensions = new Integer2(texture.Width, texture.Height);
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);

            transform = new Transform2D(position, 180, scale, origin, dimensions);

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn, transform, Color.White, SpriteEffects.None, 0f, texture,
                buttonText, this.fontDictionary["menu"], Color.Transparent, Vector2.Zero);


            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));
            this.menuManager.Add(sceneID, uiButtonObject);






            texture = this.textureDictionary["volDown"];
            buttonID = "volumeDownbtn";
            buttonText = "";

            position = new Vector2(graphics.PreferredBackBufferWidth - graphics.PreferredBackBufferWidth / 1.3f, texture.Height);
            scale = new Vector2(0.3f, 0.3f);
            dimensions = new Integer2(texture.Width, texture.Height);
            origin = new Vector2(texture.Width / 2.0f, texture.Height / 2.0f);

            transform = new Transform2D(position, 0, scale, origin, dimensions);

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn, transform, Color.White, SpriteEffects.None, 0f, texture,
                buttonText, this.fontDictionary["menu"], Color.Transparent, Vector2.Zero);


            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.DarkOrange, Color.Orange));
            this.menuManager.Add(sceneID, uiButtonObject);




            sceneID = "audio menu";
            transform = null;
            texture = null;
            position = Vector2.Zero;
            scale = Vector2.Zero;

            texture = this.textureDictionary["audio-bar"];
            position = new Vector2(graphics.PreferredBackBufferWidth / 2, graphics.PreferredBackBufferHeight / 1.5f);
            origin = new Vector2(texture.Width / 2, texture.Height / 2f);
            dimensions = new Integer2(texture.Width, texture.Height);
            scale = new Vector2(0.6f,0.6f);

            transform = new Transform2D(position,0,scale,origin,dimensions);
            this.menuManager.Add(sceneID, new UITextureObject("audio-bar", ActorType.UIStaticTexture, StatusType.Drawn,
                transform, Color.White, SpriteEffects.None, -2, texture));


        }


        #endregion
        private void AddUIElements()
        {
            InitializeUIMousePointer();
            InitializeTimerUI();
        }

        private void InitializeUIMousePointer()
        {
            Texture2D texture = this.textureDictionary["reticuleDefault"];
            //show complete texture
            Microsoft.Xna.Framework.Rectangle sourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height);

            Transform2D transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2 - texture.Width/2, graphics.PreferredBackBufferHeight / 2 - texture.Height/2), 0, Vector2.One, Vector2.Zero, new Integer2(texture.Width, texture.Height));
            UITextureObject crosshair = new UITextureObject("crosshair", ActorType.UIStaticTexture, StatusType.Drawn, transform, Color.White, SpriteEffects.None, 1, texture);

           



            uiManager.Add(crosshair);
            //listens for object picking events from the object picking manager
            // UIPickingMouseObject myUIMouseObject = new UIPickingMouseObject("picking mouseObject",
            //    ActorType.UITexture,
            //    new Transform2D(Vector2.One),
            //    this.fontDictionary["mouse"],
            //    "",
            //    new Vector2(0, 40),
            //    texture,
            //    this.mouseManager,
            //    this.eventDispatcher);
            // this.uiManager.Add(myUIMouseObject);
        }

        /*
         * Author: Cameron
         * Setup the timer UI
         */
        private void InitializeTimerUI()
        {
            int count = 1;

            foreach (TimerUtility timer in timerManager.TimerList)
            {
                Transform2D timerTransform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth-130, 25 * count),
                    0, Vector2.One, Vector2.Zero, Integer2.Zero);

                UITimer uiTimer = new UITimer(timerTransform, Color.WhiteSmoke, 0.1f, 
                        fontDictionary["timerFont"], timer);
                this.uiManager.Add(uiTimer);

                /*
                //An experimental shadow
                Transform2D shadowTimerTransform = new Transform2D(new Vector2(timerTransform.Translation.X - 4,
                    timerTransform.Translation.Y - 2),
                    0, new Vector2(1.1f), Vector2.Zero, Integer2.Zero);
                UITimer uiShadowTimer = new UITimer(shadowTimerTransform, Color.Black, 1, 
                    fontDictionary["timerFont"], timer);
                this.uiManager.Add(uiShadowTimer);
                */
                count++;
            }
        }

        #region Effects
        private void InitializeEffects()
        {
            BasicEffect basicEffect = null;
            DualTextureEffect dualTextureEffect = null;
            Effect billboardEffect = null;


            #region Lit objects
            //create a BasicEffect and set the lighting conditions for all models that use this effect in their EffectParameters field
            basicEffect = new BasicEffect(graphics.GraphicsDevice);

            basicEffect.TextureEnabled = true;
            //basicEffect.LightingEnabled = false;


            basicEffect.LightingEnabled = true; // turn on the lighting subsystem.
            basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.3f); // a red light
            basicEffect.DirectionalLight0.Direction = new Vector3(1);  // coming along the x-axis
            basicEffect.DirectionalLight0.SpecularColor = new Vector3(0, 0.15f, 0); // with green highlights
            basicEffect.SpecularPower = 1f;

            basicEffect.DirectionalLight1.Enabled = true;
            basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0.25f, 0.25f, 0.2f); // a red light
            basicEffect.DirectionalLight1.Direction = new Vector3(-1);  // coming along the x-axis
            basicEffect.DirectionalLight1.SpecularColor = new Vector3(0, 0.15f, 0); // with green highlights
            basicEffect.SpecularPower = 1f;

            basicEffect.AmbientLightColor = new Vector3(0.5f);
            basicEffect.EmissiveColor = new Vector3(0.5f);

            this.effectDictionary.Add(AppData.LitModelsEffectID, new BasicEffectParameters(basicEffect));
            #endregion
            
            #region Darkened Lit objects
            //create a BasicEffect and set the lighting conditions for all models that use this effect in their EffectParameters field
            basicEffect = new BasicEffect(graphics.GraphicsDevice);

            basicEffect.TextureEnabled = true;

            basicEffect.LightingEnabled = true; // turn on the lighting subsystem.
            basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.19f); // a red light
            basicEffect.DirectionalLight0.Direction = new Vector3(1);  // coming along the x-axis
            basicEffect.DirectionalLight0.SpecularColor = new Vector3(0, 0.25f, 0); // with green highlights
            basicEffect.SpecularPower = 0.1f;

            basicEffect.DirectionalLight1.Enabled = true;
            basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0.25f, 0.25f, 0.2f); // a red light
            basicEffect.DirectionalLight1.Direction = new Vector3(-1);  // coming along the x-axis
            basicEffect.DirectionalLight1.SpecularColor = new Vector3(0); // with green highlights
            basicEffect.SpecularPower = 0.1f;

            basicEffect.AmbientLightColor = new Vector3(1);
            basicEffect.EmissiveColor = new Vector3(1);

            this.effectDictionary.Add(AppData.DarkLitModelsEffectID, new BasicEffectParameters(basicEffect));
            #endregion

            #region For Unlit objects
            //used for model objects that dont interact with lighting i.e. sky
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.TextureEnabled = true;
            //basicEffect.LightingEnabled = false;


            basicEffect.LightingEnabled = true; // turn on the lighting subsystem.
            basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.25f); // a red light
            basicEffect.DirectionalLight0.Direction = new Vector3(1);  // coming along the x-axis
            basicEffect.DirectionalLight0.SpecularColor = new Vector3(0, 0.15f, 0); // with green highlights
            basicEffect.SpecularPower = 0.5f;

            basicEffect.DirectionalLight1.Enabled = true;
            basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0.25f, 0.25f, 0.2f); // a red light
            basicEffect.DirectionalLight1.Direction = new Vector3(-1);  // coming along the x-axis
            basicEffect.DirectionalLight1.SpecularColor = new Vector3(0, 0.15f, 0); // with green highlights
            basicEffect.SpecularPower = 0.5f;

            basicEffect.AmbientLightColor = new Vector3(0.5f);
            basicEffect.EmissiveColor = new Vector3(0.5f);
            
            this.effectDictionary.Add(AppData.UnlitModelsEffectID, new BasicEffectParameters(basicEffect));
            #endregion

            #region For dual texture objects
            dualTextureEffect = new DualTextureEffect(graphics.GraphicsDevice);
            this.effectDictionary.Add(AppData.UnlitModelDualEffectID, new DualTextureEffectParameters(dualTextureEffect));
            #endregion


            #region For unlit primitive objects
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = true;
            this.effectDictionary.Add(AppData.UnLitPrimitivesEffectID, new BasicEffectParameters(basicEffect));
            #endregion

        }
        #endregion

        #region Content, Update, Draw        
        protected override void LoadContent()
        {
            //moved to Initialize
            //spriteBatch = new SpriteBatch(GraphicsDevice);

            //            #region Add Menu & UI
            //            InitializeMenu();
            //            AddMenuElements();
            //            InitializeUI();
            //            AddUIElements();
            //            #endregion

#if DEBUG
           // InitializeDebugTextInfo();
#endif
        }

        protected override void UnloadContent()
        {
            //formally call garbage collection on all ContentDictionary objects to de-allocate resources from RAM
            this.modelDictionary.Dispose();
            this.textureDictionary.Dispose();
            this.fontDictionary.Dispose();
            this.videoDictionary.Dispose();

        }

        protected override void Update(GameTime gameTime)
        {

            if(this.keyboardManager.IsKeyDown(Keys.P))
            {
                //EventDispatcher.Publish(new EventData(EventActionType.OnRestart,EventCategoryType.Reset));
                 //EventDispatcher.Publish(new EventData(EventActionType.OpenDoor,EventCategoryType.Animator));
                EventDispatcher.Publish(new EventData(EventActionType.OpenBookcase, EventCategoryType.Animator));
                //EventDispatcher.Publish(new EventData(EventActionType.RotateTopBarrier, EventCategoryType.Animator));
                //EventDispatcher.Publish(new EventData(EventActionType.RotateBottomBarrier, EventCategoryType.Animator));
                //EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, new object[] { "collidable first person camera" }));
            }
            //exit using new gamepad manager
            if (this.gamePadManager.IsPlayerConnected(PlayerIndex.One) && this.gamePadManager.IsButtonPressed(PlayerIndex.One, Buttons.Back))
                this.Exit();      
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
        #endregion
    }
}
