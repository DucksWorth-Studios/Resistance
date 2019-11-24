#define DEMO

using System;
using System.Collections.Generic;
using System.Diagnostics;
using GDLibrary;
using JigLibX.Collision;
using JigLibX.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using PrimitiveType = Microsoft.Xna.Framework.Graphics.PrimitiveType;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

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
        #region Constructor

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        #endregion

        private void AddUIElements()
        {
            InitializeUIMousePointer();
            InitializeTimerUI();
        }

        private void InitializeUIMousePointer()
        {
            var texture = textureDictionary["reticuleDefault"];
            //show complete texture
            var sourceRectangle = new Rectangle(0, 0, texture.Width, texture.Height);

            var transform =
                new Transform2D(
                    new Vector2(graphics.PreferredBackBufferWidth / 2 - texture.Width / 2,
                        graphics.PreferredBackBufferHeight / 2 - texture.Height / 2), 0, Vector2.One, Vector2.Zero,
                    new Integer2(texture.Width, texture.Height));
            var crosshair = new UITextureObject("crosshair", ActorType.UIStaticTexture, StatusType.Drawn, transform,
                Color.White, SpriteEffects.None, 1, texture);


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

        private void InitializeTimerUI()
        {
            var count = 1;

            foreach (var timer in timerManager.TimerList)
            {
                var timerTransform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth - 100, 25 * count),
                    0, Vector2.One, Vector2.Zero, Integer2.Zero);

                var uiTimer = new UITimer(timerTransform, 0.1f, fontDictionary["timerFont"], timer);
                uiManager.Add(uiTimer);
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
            basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.38f, 0.38f, 0.38f); // a red light
            basicEffect.DirectionalLight0.Direction = new Vector3(1, 1, 1); // coming along the x-axis
            basicEffect.DirectionalLight0.SpecularColor = new Vector3(0, 0.25f, 0); // with green highlights
            basicEffect.SpecularPower = 1f;

            basicEffect.DirectionalLight1.Enabled = true;
            basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.4f); // a red light
            basicEffect.DirectionalLight1.Direction = new Vector3(-1, -1, -1); // coming along the x-axis
            basicEffect.DirectionalLight1.SpecularColor = new Vector3(0, 0.25f, 0); // with green highlights
            basicEffect.SpecularPower = 1f;

            basicEffect.AmbientLightColor = new Vector3(1, 1, 1);
            basicEffect.EmissiveColor = new Vector3(1, 1, 1);

            effectDictionary.Add(AppData.LitModelsEffectID, new BasicEffectParameters(basicEffect));

            #endregion

            #region For Unlit objects

            //used for model objects that dont interact with lighting i.e. sky
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.TextureEnabled = true;
            //basicEffect.LightingEnabled = false;


            basicEffect.LightingEnabled = true; // turn on the lighting subsystem.
            basicEffect.DirectionalLight0.DiffuseColor = new Vector3(0.38f, 0.38f, 0.38f); // a red light
            basicEffect.DirectionalLight0.Direction = new Vector3(1, 1, 1); // coming along the x-axis
            basicEffect.DirectionalLight0.SpecularColor = new Vector3(0, 0.25f, 0); // with green highlights
            basicEffect.SpecularPower = 1f;

            basicEffect.DirectionalLight1.Enabled = true;
            basicEffect.DirectionalLight1.DiffuseColor = new Vector3(0.5f, 0.5f, 0.4f); // a red light
            basicEffect.DirectionalLight1.Direction = new Vector3(-1, -1, -1); // coming along the x-axis
            basicEffect.DirectionalLight1.SpecularColor = new Vector3(0, 0.25f, 0); // with green highlights
            basicEffect.SpecularPower = 1f;

            basicEffect.AmbientLightColor = new Vector3(1, 1, 1);
            basicEffect.EmissiveColor = new Vector3(1, 1, 1);

            effectDictionary.Add(AppData.UnlitModelsEffectID, new BasicEffectParameters(basicEffect));

            #endregion

            #region For dual texture objects

            dualTextureEffect = new DualTextureEffect(graphics.GraphicsDevice);
            effectDictionary.Add(AppData.UnlitModelDualEffectID, new DualTextureEffectParameters(dualTextureEffect));

            #endregion

            #region For unlit billboard objects

            billboardEffect = Content.Load<Effect>("Assets/Effects/billboard");
            effectDictionary.Add(AppData.UnlitBillboardsEffectID, new BillboardEffectParameters(billboardEffect));

            #endregion

            #region For unlit primitive objects

            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.VertexColorEnabled = true;
            effectDictionary.Add(AppData.UnLitPrimitivesEffectID, new BasicEffectParameters(basicEffect));

            #endregion
        }

        #endregion

        #region Fields

#if DEBUG
        //used to visualize debug info (e.g. FPS) and also to draw collision skins
        //private DebugDrawer debugDrawer;
        //private PhysicsDebugDrawer physicsDebugDrawer;
#endif

        private readonly GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

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
        public LogicManager logicPuzzle;

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

        #endregion

        #region Properties

        #endregion

        #region Initialization

        protected override void Initialize()
        {
            //moved instanciation here to allow menu and ui managers to be moved to InitializeManagers()
            spriteBatch = new SpriteBatch(GraphicsDevice);

            var gameLevel = 1;
            var isMouseVisible = true;
            var screenResolution = ScreenUtility.HD720;
            var screenType = ScreenUtility.ScreenType.SingleScreen;
            var numberOfGamePadPlayers = 1;

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
#if DEBUG
            //InitializeDebugTextInfo();
#endif

            //load game happens before cameras are loaded because we may add a third person camera that needs a reference to a loaded Actor
            LoadGame(gameLevel);

            InitializeCollidableFirstPersonDemo(screenResolution);
            InitializeCutsceneCameras();
            //Publish Start Event(s)
            StartGame();

#if DEBUG
            //InitializeDebugCollisionSkinInfo();
#endif

            InitializeEvents();
            initialiseTestObject();
            InitializeSwitches();
            InitialisePuzzleLights();
            InitialisePopUP();
            InitialiseObjectiveHUD();
            loadCurrentObjective();


            base.Initialize();
        }


        /**
         * Authors : Tomas & Aaron
         * This initialises the popup and scales it accrding to screen size
         */
        private void InitialisePopUP()
        {
            var texture = textureDictionary["popup"];

            int w, x, y, z, tw, th;
            var temp = graphics.PreferredBackBufferWidth / 4;
            x = graphics.PreferredBackBufferWidth / 6;
            y = graphics.PreferredBackBufferHeight / 6;
            w = graphics.PreferredBackBufferWidth - x * 2;
            z = graphics.PreferredBackBufferHeight - y * 2;
            tw = texture.Width;
            th = texture.Height;


            var scale = new Vector2(
                (float) x / 700,
                (float) y / 390);


            var translation = new Vector2(
                x,
                y);

            var transform = new Transform2D(translation, 0, scale, new Vector2(0, 0), new Integer2(0, 0));


            // Transform2D transform = new Transform2D(new Vector2(x,y), 0, new Vector2(1f, 1f),new Vector2(1,1),new Integer2(w,z));
            var rect = new Rectangle(0, 0, tw, th);


            var picture = new UITextureObject("PopUp", ActorType.PopUP, StatusType.Off, transform, Color.White,
                SpriteEffects.None, 0, texture, rect, new Vector2(0, 0));

            uiManager.Add(picture);
        }


        private void InitialiseObjectiveHUD()
        {
            var texture = textureDictionary["Objective"];

            int x, y, tw, th;
            tw = texture.Width;
            th = texture.Height;
            x = graphics.PreferredBackBufferWidth;
            y = graphics.PreferredBackBufferHeight;


            var scale = new Vector2(
                x / y,
                x / y);


            var translation = new Vector2(
                (float) (graphics.PreferredBackBufferWidth / 2) - x / y * tw / 2,
                1);

            var transform = new Transform2D(translation, 0, scale, new Vector2(0, 0), new Integer2(0, 0));


            // Transform2D transform = new Transform2D(new Vector2(x,y), 0, new Vector2(1f, 1f),new Vector2(1,1),new Integer2(w,z));
            var rect = new Rectangle(0, 0, tw, th);


            var picture = new UITextureObject("Objective", ActorType.UIDynamicText, StatusType.Drawn, transform,
                Color.White,
                SpriteEffects.None, 0, texture, rect, new Vector2(0, 0));

            uiManager.Add(picture);
        }


        private void loadCurrentObjective()
        {
            var texture = objectiveManager.InitializeObjectivesUI();

            int x, y, tw, th;
            tw = texture.Width;
            th = texture.Height;
            x = graphics.PreferredBackBufferWidth;
            y = graphics.PreferredBackBufferHeight;


            var scale = new Vector2(
                x / y,
                x / y);


            var translation = new Vector2(
                (float) (graphics.PreferredBackBufferWidth / 2) - x / y * tw / 2,
                (float) y / 18);

            var transform = new Transform2D(translation, 0, scale, new Vector2(0, 0), new Integer2(0, 0));


            // Transform2D transform = new Transform2D(new Vector2(x,y), 0, new Vector2(1f, 1f),new Vector2(1,1),new Integer2(w,z));
            var rect = new Rectangle(0, 0, tw, th);


            var picture = new UITextureObject("currentObjective", ActorType.Objective, StatusType.Drawn, transform,
                Color.White,
                SpriteEffects.None, 0, texture, rect, new Vector2(0, 0));

            uiManager.Add(picture);
        }

        /**
         * Author: Tomas
         * Initialises switch objects for logic puzzle
         */
        private void InitializeSwitches()
        {
            CollidableObject collidableObject, archetypeCollidableObject = null;

            var model = modelDictionary["box2"];
            var effectParameters =
                (effectDictionary[AppData.LitModelsEffectID] as BasicEffectParameters).Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["gray"];

            archetypeCollidableObject = new CollidableObject("switch-", ActorType.Interactable, Transform3D.Zero,
                effectParameters, model);

            var count = 0;
            for (var i = 1; i < 5; ++i)
            {
                ++count;
                collidableObject = (CollidableObject) archetypeCollidableObject.Clone();
                collidableObject.ID = "switch-" + count;


                collidableObject.Transform = new Transform3D(new Vector3(-46, 5.5f * i, -125), new Vector3(0, 0, 0),
                    new Vector3(1, 1, 1), Vector3.UnitX, Vector3.UnitY);
                collidableObject.AddPrimitive(
                    new Box(collidableObject.Transform.Translation, Matrix.Identity,
                        2.54f * collidableObject.Transform.Scale), new MaterialProperties(0.2f, 0.8f, 0.7f));

                //increase the mass of the boxes in the demo to see how collidable first person camera interacts vs. spheres (at mass = 1)
                collidableObject.Enable(true, 1);
                objectManager.Add(collidableObject);
            }
        }

        /**
         * Author Tomas
         * Initialise the lights for logic puzzle gates
         */
        private void InitialisePuzzleLights()
        {
            CollidableObject collidableObject, archetypeCollidableObject = null;
            var model = modelDictionary["sphere"];

            var effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["gray"];

            //make once then clone
            archetypeCollidableObject =
                new CollidableObject("sphere", ActorType.Light, Transform3D.Zero, effectParameters, model);

            collidableObject = (CollidableObject) archetypeCollidableObject.Clone();

            #region Gate-1

            collidableObject.ID = "gate-1";
            collidableObject.Transform = new Transform3D(new Vector3(-36.5f, 12.75f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3
                    .One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);

            #endregion


            #region Gate-2

            collidableObject = (CollidableObject) archetypeCollidableObject.Clone();
            collidableObject.ID = "gate-2";
            collidableObject.Transform = new Transform3D(new Vector3(-33, 19.75f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3
                    .One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);

            #endregion


            #region Gate-3

            collidableObject = (CollidableObject) archetypeCollidableObject.Clone();
            collidableObject.ID = "gate-3";
            collidableObject.Transform = new Transform3D(new Vector3(-25.75f, 9, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3
                    .One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);

            #endregion

            #region Gate-4

            collidableObject = (CollidableObject) archetypeCollidableObject.Clone();
            collidableObject.ID = "gate-4";
            collidableObject.Transform = new Transform3D(new Vector3(-13.5f, 13.75f, -125.25f), new Vector3(0, 0, 0),
                0.0082f * Vector3
                    .One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.0082f),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);

            #endregion

            #region End Light

            collidableObject = (CollidableObject) archetypeCollidableObject.Clone();
            collidableObject.ID = "gate-5";
            collidableObject.Transform = new Transform3D(new Vector3(-11f, 13.75f, -125.25f), new Vector3(0, 0, 0),
                0.025f * Vector3
                    .One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                Vector3.UnitX, Vector3.UnitY);

            collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 0.025f),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);

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
            var model = modelDictionary["box2"];
            var effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["riddletexture"];
            var transform = new Transform3D(new Vector3(-90, 6.9f, -120), new Vector3(-90, 0, 0),
                new Vector3(1, 1, 0.0001f), Vector3.UnitX, Vector3.UnitY);
            var collidableObject =
                new CollidableObject("Riddle Pickup", ActorType.PopUP, transform, effectParameters, model);
            collidableObject.AddPrimitive(
                new Box(collidableObject.Transform.Translation, Matrix.Identity,
                    2.54f * collidableObject.Transform.Scale), new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);
        }

        #endregion


        private void InitializeManagers(Integer2 screenResolution,
            ScreenUtility.ScreenType screenType, bool isMouseVisible, int numberOfGamePadPlayers) //1 - 4
        {
            //add sound manager
            soundManager = new SoundManager(this, eventDispatcher, StatusType.Update, "Content/Assets/Audio/",
                "Music.xgs", "Music Wave Bank.xwb", "Music Sound Bank.xsb");
            Components.Add(soundManager);

            cameraManager = new CameraManager(this, 1, eventDispatcher);
            Components.Add(cameraManager);

            //create the object manager - notice that its not a drawablegamecomponent. See ScreeManager::Draw()
            objectManager = new ObjectManager(this, cameraManager, eventDispatcher, 10);

            //add keyboard manager
            keyboardManager = new KeyboardManager(this);
            Components.Add(keyboardManager);

            //create the manager which supports multiple camera viewports
            screenManager = new ScreenManager(this, graphics, screenResolution, screenType,
                objectManager, cameraManager, keyboardManager,
                AppData.KeyPauseShowMenu, eventDispatcher, StatusType.Off);
            Components.Add(screenManager);

            //CD-CR using JigLibX and add debug drawer to visualise collision skins
            physicsManager = new PhysicsManager(this, eventDispatcher, StatusType.Off, AppData.BigGravity);
            Components.Add(physicsManager);

            //add mouse manager
            mouseManager = new MouseManager(this, isMouseVisible, physicsManager);
            Components.Add(mouseManager);

            //add gamepad manager
            if (numberOfGamePadPlayers > 0)
            {
                gamePadManager = new GamePadManager(this, numberOfGamePadPlayers);
                Components.Add(gamePadManager);
            }

            //menu manager
            menuManager = new MyAppMenuManager(this, mouseManager, keyboardManager, cameraManager, spriteBatch,
                eventDispatcher, StatusType.Off);
            //set the main menu to be the active menu scene
            menuManager.SetActiveList("main menu");
            //this.menuManager.SetActiveList("lose-screen");
            Components.Add(menuManager);

            //ui (e.g. reticule, inventory, progress)
            uiManager = new UIManager(this, spriteBatch, eventDispatcher, 10, StatusType.Off);
            Components.Add(uiManager);


            //this object packages together all managers to give the mouse object the ability to listen for all forms of input from the user, as well as know where camera is etc.
            managerParameters = new ManagerParameters(objectManager,
                cameraManager, mouseManager, keyboardManager, gamePadManager, screenManager, soundManager);

            logicPuzzle = new LogicManager(this, eventDispatcher);
            Components.Add(logicPuzzle);

            #region Pick Manager

            //call this function anytime we want to decide if a mouse over object is interesting to the PickingManager
            //See https://www.codeproject.com/Articles/114931/Understanding-Predicate-Delegates-in-C
            var collisionPredicate = new Predicate<CollidableObject>(CollisionUtility.IsCollidableObjectOfInterest);
            //create the projectile archetype that the manager can fire

            //listens for picking with the mouse on valid (based on specified predicate) collidable objects and pushes notification events to listeners
            pickingManager = new PickingManager(this, eventDispatcher, StatusType.Off,
                managerParameters,
                PickingBehaviourType.InteractWithObject, AppData.PickStartDistance, AppData.PickEndDistance,
                collisionPredicate);
            Components.Add(pickingManager);

            #endregion

            cutsceneTimer = new CutsceneTimer("CutsceneTimer", eventDispatcher, this);
            Components.Add(cutsceneTimer);


            timerManager = new TimerManager(AppData.LoseTimerID, AppData.LoseTimerHours, AppData.LoseTimerMinutes,
                AppData.LoseTimerSeconds, this, eventDispatcher, StatusType.Off);
            Components.Add(timerManager);


            objectiveManager = new ObjectiveManager(this, eventDispatcher, StatusType.Off, 0, spriteBatch,
                textureDictionary, uiManager);
            Components.Add(objectiveManager);
        }

        private void LoadDictionaries()
        {
            //models
            modelDictionary = new ContentDictionary<Model>("model dictionary", Content);

            //textures
            textureDictionary = new ContentDictionary<Texture2D>("texture dictionary", Content);

            //fonts
            fontDictionary = new ContentDictionary<SpriteFont>("font dictionary", Content);

            //curves - notice we use a basic Dictionary and not a ContentDictionary since curves and rails are NOT media content
            curveDictionary = new Dictionary<string, Transform3DCurve>();

            //rails
            railDictionary = new Dictionary<string, RailParameters>();

            //viewports - used to store different viewports to be applied to multi-screen layouts
            viewPortDictionary = new Dictionary<string, Viewport>();

            //stores default effect parameters
            effectDictionary = new Dictionary<string, EffectParameters>();

            //notice we go back to using a content dictionary type since we want to pass strings and have dictionary load content
            videoDictionary = new ContentDictionary<Video>("video dictionary", Content);

            //used to store IVertexData (i.e. when we want to draw primitive objects, as in I-CA)
            vertexDataDictionary = new Dictionary<string, IVertexData>();
        }

        private void LoadAssets()
        {
            #region Models

            //geometric samples
            modelDictionary.Load("Assets/Models/plane1", "plane1");
            //this.modelDictionary.Load("Assets/Models/plane", "plane");
            modelDictionary.Load("Assets/Models/box2", "box2");
            modelDictionary.Load("Assets/Models/sphere", "sphere");
            //architecture
            modelDictionary.Load("Assets/Models/Architecture/Buildings/house");
            modelDictionary.Load("Assets/Models/Architecture/Doors/Barrier_Mapped_01", "barrier");
            modelDictionary.Load("Assets/Models/Architecture/Doors/BunkerDoor_Mapped_01", "bunker_door");

            //props
            modelDictionary.Load("Assets/Models/Props/lamp");
            modelDictionary.Load("Assets/Models/Props/ammo-box");
            modelDictionary.Load("Assets/Models/Props/field-cot");
            modelDictionary.Load("Assets/Models/Props/field-desk");
            modelDictionary.Load("Assets/Models/Props/war-table");
            modelDictionary.Load("Assets/Models/Props/FilingCabinet");
            modelDictionary.Load("Assets/Models/Props/Bookshelf_01");
            modelDictionary.Load("Assets/Models/Props/Phonograph");
            modelDictionary.Load("Assets/Models/Props/computer");
            modelDictionary.Load("Assets/Models/Props/LogicPuzzle");
            modelDictionary.Load("Assets/Models/Props/Gun");

            #endregion

            #region Textures

            //environment
            textureDictionary.Load("Assets/GDDebug/Textures/checkerboard");
            textureDictionary.Load("Assets/Textures/Foliage/Ground/grass1");
            textureDictionary.Load("Assets/Textures/Architecture/concrete2", "wall");
            textureDictionary.Load("Assets/Textures/Architecture/concrete", "concreteFloor");
            textureDictionary.Load("Assets/Textures/Architecture/concrete2");

            //menu - buttons
            textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/genericbtn");
            textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/quit");
            textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/start");
            textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/restart-Button");
            textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/Resume");

            textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/back-Button", "back");
            textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/control-button", "controls");
            textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/mainMenu-Button");


            //menu - backgrounds
            textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/Title-screen");
            textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/game-over");
            textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/PauseMenu");
            textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/control-screen", "ControlMenu");
            textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/win-screen");

            //ui (or hud) elements
            textureDictionary.Load("Assets/Textures/UI/HUD/reticuleDefault");
            textureDictionary.Load("Assets/Textures/UI/HUD/progress_gradient");
            textureDictionary.Load("Assets/Textures/UI/HUD/Objective");
            textureDictionary.Load("Assets/Textures/UI/HUD/Escape");
            textureDictionary.Load("Assets/Textures/UI/HUD/Riddle");
            textureDictionary.Load("Assets/Textures/UI/HUD/Logic");

            //architecture
            textureDictionary.Load("Assets/Textures/Architecture/Buildings/house-low-texture");
            textureDictionary.Load("Assets/Textures/Architecture/Doors/Concrete", "concrete");
            textureDictionary.Load("Assets/Textures/Architecture/Doors/BrushedAluminum", "aluminum");
            //this.textureDictionary.Load("Assets/Textures/Architecture/Walls/wall");
            textureDictionary.Load("Assets/Textures/Props/Crates/crate1");
            //dual texture demo - see Main::InitializeCollidableGround()
            textureDictionary.Load("Assets/GDDebug/Textures/checkerboard_greywhite");

            //Load Colors
            textureDictionary.Load("Assets/Colours/gray");
            textureDictionary.Load("Assets/Colours/green");
            textureDictionary.Load("Assets/Colours/black");
            //load riddle pop up
            textureDictionary.Load("Assets/Textures/UI/HUD/Popup/the-riddle", "popup");

            //props
            textureDictionary.Load("Assets/Textures/Props/Resistance/ammo-box");
            textureDictionary.Load("Assets/Textures/Props/Resistance/ComputerTexture");
            textureDictionary.Load("Assets/Textures/Props/Resistance/FieldCotTexture");
            textureDictionary.Load("Assets/Textures/Props/Resistance/FieldDeskTexture");
            textureDictionary.Load("Assets/Textures/Props/Resistance/WarTableTexture");
            textureDictionary.Load("Assets/Textures/Props/Resistance/LightTexture");
            textureDictionary.Load("Assets/Textures/Props/Resistance/FilingCabinet");
            textureDictionary.Load("Assets/Textures/Props/Resistance/bookcase");
            textureDictionary.Load("Assets/Textures/Props/Resistance/phonograph");
            textureDictionary.Load("Assets/Textures/Props/Interactable/GunTexture");

            //interactable
            textureDictionary.Load("Assets/Textures/Props/Interactable/riddletexture");
#if DEBUG
            //demo
            //this.textureDictionary.Load("Assets/GDDebug/Textures/ml");
            //this.textureDictionary.Load("Assets/GDDebug/Textures/checkerboard");
#endif

            #endregion

            #region Fonts

#if DEBUG
            //this.fontDictionary.Load("Assets/GDDebug/Fonts/debug");
#endif
            fontDictionary.Load("Assets/Fonts/menu");
            fontDictionary.Load("Assets/Fonts/mouse");
            fontDictionary.Load("Assets/Fonts/timerFont");

            #endregion

            #region Video

            videoDictionary.Load("Assets/Video/sample");

            #endregion

            #region Animations

            //contains a single animation "Take001"
            modelDictionary.Load("Assets/Models/Animated/dude");

            //squirrel - one file per animation
            modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Idle");
            modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Jump");
            modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Punch");
            modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Standing");
            modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Tailwhip");
            modelDictionary.Load("Assets/Models/Animated/Squirrel/RedRun4");
            modelDictionary.Load("Assets/Models/Architecture/Doors/Barrier_Mapped_01");

            #endregion
        }

        private void LoadVertexData()
        {
            PrimitiveType primitiveType;
            int primitiveCount;
            IVertexData vertexData = null;

            #region Textured Quad

            //get vertices for textured quad
            var vertices = VertexFactory.GetTextureQuadVertices(out primitiveType, out primitiveCount);

            //make a vertex data object to store and draw the vertices
            vertexData = new BufferedVertexData<VertexPositionColorTexture>(graphics.GraphicsDevice, vertices,
                primitiveType, primitiveCount);

            //add to the dictionary for use by things like billboards - see InitializeBillboards()
            vertexDataDictionary.Add(AppData.TexturedQuadID, vertexData);

            #endregion

            #region Billboard Quad - we must use this type when creating billboards

            // get vertices for textured billboard
            var verticesBillboard = VertexFactory.GetVertexBillboard(1, out primitiveType, out primitiveCount);

            //make a vertex data object to store and draw the vertices
            vertexData = new BufferedVertexData<VertexBillboard>(graphics.GraphicsDevice, verticesBillboard,
                primitiveType, primitiveCount);

            //add to the dictionary for use by things like billboards - see InitializeBillboards()
            vertexDataDictionary.Add(AppData.TexturedBillboardQuadID, vertexData);

            #endregion
        }

        private void LoadViewports(Integer2 screenResolution)
        {
            //the full screen viewport with optional padding
            int leftPadding = 0, topPadding = 0, rightPadding = 0, bottomPadding = 0;
            var paddedFullViewPort = ScreenUtility.Pad(new Viewport(0, 0, screenResolution.X, screenResolution.Y),
                leftPadding, topPadding, rightPadding, bottomPadding);
            viewPortDictionary.Add("full viewport", paddedFullViewPort);

            //work out the dimensions of the small camera views along the left hand side of the screen
            var smallViewPortHeight =
                144; //6 small cameras along the left hand side of the main camera view i.e. total height / 5 = 720 / 5 = 144
            var smallViewPortWidth =
                5 * smallViewPortHeight /
                3; //we should try to maintain same ProjectionParameters aspect ratio for small cameras as the large     
            //the five side viewports in multi-screen mode
            viewPortDictionary.Add("column0 row0", new Viewport(0, 0, smallViewPortWidth, smallViewPortHeight));
            viewPortDictionary.Add("column0 row1",
                new Viewport(0, 1 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            viewPortDictionary.Add("column0 row2",
                new Viewport(0, 2 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            viewPortDictionary.Add("column0 row3",
                new Viewport(0, 3 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            viewPortDictionary.Add("column0 row4",
                new Viewport(0, 4 * smallViewPortHeight, smallViewPortWidth, smallViewPortHeight));
            //the larger view to the right in column 1
            viewPortDictionary.Add("column1 row0",
                new Viewport(smallViewPortWidth, 0, screenResolution.X - smallViewPortWidth, screenResolution.Y));

            //picture-in-picture viewport
            var viewPortDimensions = new Integer2(240, 150); //set to 16:10 ratio as with screen dimensions
            var verticalOffset = 20;
            var rightHorizontalOffset = 20;
            viewPortDictionary.Add("PIP viewport", new Viewport(
                screenResolution.X - viewPortDimensions.X - rightHorizontalOffset,
                verticalOffset, viewPortDimensions.X, viewPortDimensions.Y));
        }

#if DEBUG
        //private void InitializeDebugTextInfo()
        //{
        //    //add debug info in top left hand corner of the screen
        //    this.debugDrawer = new DebugDrawer(this, this.managerParameters, spriteBatch,
        //        this.fontDictionary["debug"], Color.Black, new Vector2(5, 5), this.eventDispatcher, StatusType.Off);
        //    Components.Add(this.debugDrawer);

        //}

        //private void InitializeDebugCollisionSkinInfo()
        //{
        //    //show the collision skins
        //    this.physicsDebugDrawer = new PhysicsDebugDrawer(this, this.cameraManager, this.objectManager,
        //        this.screenManager, this.eventDispatcher, StatusType.Off);
        //    Components.Add(this.physicsDebugDrawer);
        //}
#endif

        #endregion

        #region Load Game Content

        //load the contents for the level specified
        private void LoadGame(int level)
        {
            var worldScale = 100;
            //collidable
            InitializeCollidableWalls(worldScale);
            InitializeCollidableGround(worldScale);
            InitializeNonCollidableCeiling(worldScale);

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
        }


        private void InitializeCollidableWalls(int worldScale)
        {
            //first we will create a prototype plane and then simply clone it for each of the skybox decorator elements (e.g. ground, front, top etc). 
            var transform = new Transform3D(new Vector3(0, 0, 0), new Vector3(worldScale, 1, worldScale / 10.0f));

            //clone the dictionary effect and set unique properties for the hero player object
            var effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["checkerboard"];

            var prototypeModel = new CollidableObject("plane1", ActorType.Decorator, transform, effectParameters,
                modelDictionary["box2"]);

            CollidableObject clonePlane = null;

            #region walls

            #region back wall

            clonePlane = (CollidableObject) prototypeModel.Clone();
            clonePlane.ID = "back wall";
            clonePlane.EffectParameters.Texture = textureDictionary["wall"];

            clonePlane.Transform.Scale = new Vector3(worldScale / 2.0f, 1, worldScale / 10.0f);

            clonePlane.Transform.Translation = new Vector3(-2.54f * worldScale / 4f, 2.54f * worldScale / 20.0f,
                -2.54f * worldScale / 2.0f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation,
                    Matrix.CreateRotationX(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f,
                        clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            objectManager.Add(clonePlane);

            #endregion

            #region left wall

            clonePlane = (CollidableObject) prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = textureDictionary["wall"];
            clonePlane.Transform.Translation = new Vector3(-2.54f * worldScale / 2.0f, 2.54f * worldScale / 20.0f, 0);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation,
                    Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f,
                        clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            objectManager.Add(clonePlane);

            #endregion

            #region short right wall

            clonePlane = (CollidableObject) prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(worldScale / 4, 1, worldScale / 10.0f);
            clonePlane.Transform.Translation = new Vector3(worldScale / 128.0f, 2.54f * worldScale / 20.0f,
                -2.54f * worldScale / 2.67f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation,
                    Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f,
                        clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            objectManager.Add(clonePlane);

            #endregion region

            #region long right wall

            clonePlane = (CollidableObject) prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(3 * worldScale / 4, 1, worldScale / 10.0f);
            clonePlane.Transform.Translation = new Vector3(-2.54f * worldScale / 4.0f, 2.54f * worldScale / 20.0f,
                2.54f * worldScale / 8.0f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation,
                    Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f,
                        clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            objectManager.Add(clonePlane);

            #endregion

            #region 2nd room short wall

            clonePlane = (CollidableObject) prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(worldScale / 4, 1, worldScale / 10.0f);
            clonePlane.Transform.Translation = new Vector3(-2.54f * worldScale / 8.0f, 2.54f * worldScale / 20.0f,
                -2.54f * worldScale / 4.0f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation,
                    Matrix.CreateRotationX(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f,
                        clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            objectManager.Add(clonePlane);

            #endregion

            #region front wall

            //add the front skybox plane
            // this side will be done in 3 blocks two on each side with a space for a door and then a block on top of it
            //left side of door
            var xScale = 0.833f * worldScale / 8.0f;
            clonePlane = (CollidableObject) prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(xScale, 1, worldScale / 10.0f);
            clonePlane.Transform.Translation = new Vector3(-2.54f * worldScale / 2.2f, 2.54f * worldScale / 20.0f,
                2.54f * worldScale / 2f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation,
                    Matrix.CreateRotationX(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f,
                        clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            objectManager.Add(clonePlane);

            //right side of door
            clonePlane = (CollidableObject) prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(xScale, 1, worldScale / 10.0f);
            clonePlane.Transform.Translation = new Vector3(-2.54f * worldScale / 3.5f, 2.54f * worldScale / 20.0f,
                2.54f * worldScale / 2f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation,
                    Matrix.CreateRotationX(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f,
                        clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            objectManager.Add(clonePlane);

            //top of door way
            clonePlane = (CollidableObject) prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(xScale / 1.6f, 1, worldScale / 40);
            clonePlane.Transform.Translation =
                new Vector3(-2.54f * worldScale / 2.7f, worldScale / 4.35f, 2.54f * worldScale / 2f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation,
                    Matrix.CreateRotationX(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f,
                        clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            objectManager.Add(clonePlane);

            #endregion

            #region dividing wall

            //right side
            clonePlane = (CollidableObject) prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = textureDictionary["concrete2"];
            clonePlane.Transform.Scale = new Vector3(xScale, 0.5f, worldScale / 10);
            clonePlane.Transform.Translation = new Vector3(-2.54f * worldScale / 3.97f, 2.54f * worldScale / 20.0f,
                -2.54f * worldScale / 3.55f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation,
                    Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f,
                        clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            objectManager.Add(clonePlane);

            //left side
            clonePlane = (CollidableObject) prototypeModel.Clone();
            clonePlane.EffectParameters.Texture = textureDictionary["concrete2"];
            clonePlane.Transform.Scale = new Vector3(xScale, 0.5f, worldScale / 10);
            clonePlane.Transform.Translation = new Vector3(-2.54f * worldScale / 3.97f, 2.54f * worldScale / 20.0f,
                -2.54f * worldScale / 2.2f);
            clonePlane.AddPrimitive(new Box(clonePlane.Transform.Translation,
                    Matrix.CreateRotationX(MathHelper.PiOver2) * Matrix.CreateRotationY(MathHelper.PiOver2),
                    new Vector3(clonePlane.Transform.Scale.X * 2.54f, clonePlane.Transform.Scale.Y * 2.54f,
                        clonePlane.Transform.Scale.Z * 2.54f)),
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            clonePlane.Enable(true, 1);
            objectManager.Add(clonePlane);

            #endregion

            #endregion
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
            var model = modelDictionary["box2"];

            //a simple dual texture demo - dual textures can be used with a lightMap from 3DS Max using the Render to Texture setting
            //DualTextureEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelDualEffectID].Clone() as DualTextureEffectParameters;
            //effectParameters.Texture = this.textureDictionary["grass1"];
            //effectParameters.Texture2 = this.textureDictionary["checkerboard_greywhite"];

            var effectParameters = effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["concreteFloor"];

            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero, new Vector3(worldScale, 0.1f, worldScale),
                Vector3.UnitX, Vector3.UnitY);
            collidableObject = new CollidableObject("ground", ActorType.CollidableGround, transform3D, effectParameters,
                model);
            collidableObject.AddPrimitive(
                new Box(transform3D.Translation, Matrix.Identity,
                    new Vector3(worldScale * 2.54f, 0.001f, worldScale * 2.54f)),
                new MaterialProperties(0.8f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1); //change to false, see what happens.
            objectManager.Add(collidableObject);
        }

        private void InitializeNonCollidableCeiling(int worldScale)
        {
            var transform = new Transform3D(new Vector3(0, 25, 0), Vector3.Zero,
                new Vector3(worldScale, 0.001f, worldScale), Vector3.UnitX, Vector3.UnitY);
            var effectParameters = effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["concreteFloor"];

            var model = new ModelObject("ceiling", ActorType.NonCollidableCeiling, transform, effectParameters,
                modelDictionary["box2"]);
            objectManager.Add(model);
        }

        private void InitializeBuildings()
        {
            var transform3D = new Transform3D(new Vector3(-100, 0, 0),
                new Vector3(0, 90, 0), 0.4f * Vector3.One, Vector3.UnitX, Vector3.UnitY);


            var effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["house-low-texture"];

            CollidableObject collidableObject = new TriangleMeshObject("house1", ActorType.CollidableArchitecture,
                transform3D,
                effectParameters, modelDictionary["house"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);
        }

        private void InitializeExitDoor()
        {
            //NCMG
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-85, 0, 127), new Vector3(90, 180, 0),
                new Vector3(0.09f, 0.1f, 0.06f), Vector3.UnitX, Vector3.UnitY);

            effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["aluminum"];

            collidableObject = new CollidableObject("exitDoor", ActorType.CollidableDoor, transform3D, effectParameters,
                modelDictionary["bunker_door"]);
            collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity,
                    new Vector3(40.0f, 40.0f, 2f)),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            collidableObject.AttachController(new DoorController("Door Controller", ControllerType.Rotation,
                eventDispatcher));
            objectManager.Add(collidableObject);
        }

        private void InitializeDoorBarriers()
        {
            //Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject = null, cloneCollider = null;

            //transform3D = new Transform3D(new Vector3(-98, 6, 124), new Vector3(-90, 0, 180), new Vector3(0.1f, 0.05f, 0.1f), Vector3.UnitX, Vector3.UnitY);
            effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["concrete"];

            collidableObject = new CollidableObject("barrier - ", ActorType.CollidableArchitecture, Transform3D.Zero,
                effectParameters, modelDictionary["Barrier_Mapped_01"]);

            #region Top Barrier

            cloneCollider = (CollidableObject) collidableObject.Clone();
            cloneCollider.ID += 1;

            cloneCollider.Transform = new Transform3D(new Vector3(-107, 6, 124), new Vector3(-90, 0, 180),
                new Vector3(0.07f, 0.05f, 0.07f),
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(
                new Box(cloneCollider.Transform.Translation, Matrix.Identity, new Vector3(12.0f, 1.0f, 1.0f)),
                new MaterialProperties(0.2f, 0.8f, 0.7f));

            cloneCollider.Enable(true, 1);
            cloneCollider.AttachController(new BarrierController(true, "testing", ControllerType.Rotation,
                eventDispatcher));
            objectManager.Add(cloneCollider);

            #endregion

            #region Bottom Barrier

            cloneCollider = (CollidableObject) collidableObject.Clone();
            cloneCollider.ID += 2;

            cloneCollider.Transform = new Transform3D(new Vector3(-80, 20, 124), new Vector3(-90, 0, 0),
                new Vector3(0.07f, 0.05f, 0.07f),
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(
                new Box(cloneCollider.Transform.Translation, Matrix.Identity, new Vector3(12.0f, 1.0f, 1.0f)),
                new MaterialProperties(0.2f, 0.8f, 0.7f));

            cloneCollider.Enable(true, 1);
            cloneCollider.AttachController(new BarrierController(false, "testing", ControllerType.Rotation,
                eventDispatcher));
            objectManager.Add(cloneCollider);

            #endregion
        }

        private void InitializeWarTable()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-80, 0, -30), new Vector3(0, 0, 0), new Vector3(2.0f, 1.0f, 3.0f),
                Vector3.UnitX, Vector3.UnitY);
            effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["WarTableTexture"];

            collidableObject = new TriangleMeshObject("war-table", ActorType.CollidableDecorator, transform3D,
                effectParameters, modelDictionary["war-table"],
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);
        }


        private void InitializeAmmoBoxes()
        {
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject = null, cloneCollidable;

            effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["ammo-box"];

            collidableObject = new CollidableObject("Ammo box - ", ActorType.CollidableDecorator, Transform3D.Zero,
                effectParameters,
                modelDictionary["ammo-box"]);


            for (var i = 0; i < 3; i++)
            {
                cloneCollidable = (CollidableObject) collidableObject.Clone();

                cloneCollidable.ID += i;
                cloneCollidable.Transform = new Transform3D(new Vector3(-70, 0, -30 + i * 10), new Vector3(0, 0, 0),
                    new Vector3(0.05f, 0.05f, 0.05f),
                    Vector3.UnitX, Vector3.UnitY);

                cloneCollidable.AddPrimitive(
                    new Box(cloneCollidable.Transform.Translation, Matrix.Identity, new Vector3(3, 5, 6)),
                    new MaterialProperties(0.2f, 0.8f, 0.7f));

                cloneCollidable.Enable(true, 1);
                objectManager.Add(cloneCollidable);
            }
        }

        private void InitializeFieldCot()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-120.0f, -2.5f, -90.0f), new Vector3(0, 0, 0),
                new Vector3(0.05f, 0.05f, 0.06f),
                Vector3.UnitX, Vector3.UnitY);
            effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["FieldCotTexture"];

            collidableObject = new TriangleMeshObject("field cot", ActorType.CollidableDecorator, transform3D,
                effectParameters,
                modelDictionary["field-cot"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);
        }

        private void InitializeFieldDesk()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-100.0f, 0.0f, -121.0f), new Vector3(0, 90, 0),
                new Vector3(0.15f, 0.1f, 0.15f),
                Vector3.UnitX, Vector3.UnitY);
            effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["FieldDeskTexture"];

            collidableObject = new CollidableObject("field desk", ActorType.CollidableDecorator, transform3D,
                effectParameters,
                modelDictionary["field-desk"]);
            collidableObject.AddPrimitive(
                new Box(new Vector3(-100.0f, 0.0f, -100.0f), Matrix.Identity, new Vector3(30.0f, 8.0f, 7.0f)),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);
        }

        private void InitializeFilingCabinet()
        {
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject = null, cloneCollider = null;

            effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["FilingCabinet"];

            collidableObject = new CollidableObject("filing cabinet - ", ActorType.CollidableDecorator,
                Transform3D.Zero, effectParameters,
                modelDictionary["FilingCabinet"]);
            //collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(4,2,4)),
            //  new MaterialProperties(1, 1, 1));
            //collidableObject.Enable(true, 1);
            //this.objectManager.Add(collidableObject);

            #region clones

            #region clones beside war table 1-3

            for (var i = 0; i < 3; i++)
            {
                cloneCollider = (CollidableObject) collidableObject.Clone();
                cloneCollider.ID += 1;

                cloneCollider.Transform = new Transform3D(new Vector3(-125.0f, -0.4f, i * 10), new Vector3(0, 90, 0),
                    new Vector3(0.05f, 0.05f, 0.05f),
                    Vector3.UnitX, Vector3.UnitY);
                cloneCollider.AddPrimitive(
                    new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(5, 5, 5)),
                    new MaterialProperties(1, 1, 1));
                cloneCollider.Enable(true, 1);
                objectManager.Add(cloneCollider);
            }

            #endregion

            #region clone 4 (2nd Room)

            cloneCollider = (CollidableObject) collidableObject.Clone();
            cloneCollider.ID += 4;

            cloneCollider.Transform = new Transform3D(new Vector3(-10.0f, -0.4f, -68.0f), new Vector3(0, -180, 0),
                new Vector3(0.05f, 0.05f, 0.05f),
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(
                new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(5, 5, 5)),
                new MaterialProperties(1, 1, 1));
            cloneCollider.Enable(true, 1);
            objectManager.Add(cloneCollider);

            #endregion

            #region clone 5 (left side of exit door)

            cloneCollider = (CollidableObject) collidableObject.Clone();
            cloneCollider.ID += 5;

            cloneCollider.Transform = new Transform3D(new Vector3(-75.0f, -0.4f, 122.0f), new Vector3(0, -180, 0),
                new Vector3(0.05f, 0.05f, 0.05f),
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(
                new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(5, 5, 5)),
                new MaterialProperties(1, 1, 1));
            cloneCollider.Enable(true, 1);
            objectManager.Add(cloneCollider);

            #endregion

            #region clone 6 (right side of exit door)

            cloneCollider = (CollidableObject) collidableObject.Clone();
            cloneCollider.ID += 6;

            cloneCollider.Transform = new Transform3D(new Vector3(-113.0f, -0.4f, 122.0f), new Vector3(0, -180, 0),
                new Vector3(0.05f, 0.05f, 0.05f),
                Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(
                new Box(collidableObject.Transform.Translation, Matrix.Identity, new Vector3(5, 5, 5)),
                new MaterialProperties(1, 1, 1));
            cloneCollider.Enable(true, 1);
            objectManager.Add(cloneCollider);

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

            effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["bookcase"];

            collidableObject = new CollidableObject("bookcase door", ActorType.CollidableDoor, transform3D,
                effectParameters,
                modelDictionary["Bookshelf_01"]);
            collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity,
                    new Vector3(8f, 30.0f, 35.0f)),
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            collidableObject.AttachController(new BookcaseController("Bookcase Controller", ControllerType.Rotation,
                eventDispatcher));
            objectManager.Add(collidableObject);
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
            var transform = new Transform3D(new Vector3(-100.0f, 7.0f, -121.0f), new Vector3(0, 180, 0),
                new Vector3(0.02f, 0.02f, 0.02f), Vector3.UnitX, Vector3.UnitY);
            var effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["phonograph"];

            var model = new ModelObject("phonograph", ActorType.Decorator, transform, effectParameters,
                modelDictionary["Phonograph"]);
            objectManager.Add(model);

            var phonograph = new AudioEmitter();

            phonograph.Position = new Vector3(-100.0f, 7.0f, -121.0f);
            phonograph.DopplerScale = 500000f;

            soundManager.Play3DCue("game-main-soundtrack", phonograph);
        }

        private void InitializeComputer()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-40.0f, 0.0f, -68.0f), new Vector3(0, -90, 0),
                new Vector3(0.05f, 0.05f, 0.045f), Vector3.UnitX, Vector3.UnitY);

            effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["ComputerTexture"];

            collidableObject = new TriangleMeshObject("computer", ActorType.CollidableDecorator, transform3D,
                effectParameters, modelDictionary["computer"],
                new MaterialProperties(0.1f, 0.1f, 0.1f));
            collidableObject.Enable(true, 1);
            objectManager.Add(collidableObject);
        }

        private void InitializeLogicPuzzleModel()
        {
            var transform = new Transform3D(new Vector3(-30.0f, 12.0f, -125.2f), new Vector3(90, -180, 180),
                new Vector3(0.02f, 0.02f, 0.02f), Vector3.UnitX, Vector3.UnitY);
            var effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;

            var model = new ModelObject("logic puzzle", ActorType.Decorator, transform, effectParameters,
                modelDictionary["LogicPuzzle"]);
            objectManager.Add(model);
        }

        private void InitializeRiddleAnswerObject()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-89, 8.73f, 25), new Vector3(0, 0, 90),
                new Vector3(0.5f, 0.5f, 0.5f), Vector3.UnitX, Vector3.UnitY);
            effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["GunTexture"];

            collidableObject = new TriangleMeshObject("Riddle Answer", ActorType.CollidableDecorator, transform3D,
                effectParameters,
                modelDictionary["Gun"], new MaterialProperties(0.1f, 0.1f, 0.1f));
            collidableObject.Enable(true, 1);

            objectManager.Add(collidableObject);
        }

        private void InitializeWinVolumeBox()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-91, 0, 130), Vector3.Zero, new Vector3(10, 0.1f, 2),
                Vector3.UnitX, Vector3.UnitY);
            effectParameters = effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = textureDictionary["gray"];

            collidableObject = new ImmovablePickupObject("win trigger volume", ActorType.Objective, transform3D,
                effectParameters, modelDictionary["box2"],
                modelDictionary["box2"], new MaterialProperties(0.1f, 0.1f, 0.1f),
                new PickupParameters("win trigger volume", 1));
            collidableObject.Enable(true, 1);

            objectManager.Add(collidableObject);
        }

        #endregion

        #region Initialize Cameras

        private void InitializeCamera(Integer2 screenResolution, string id, Viewport viewPort, Transform3D transform,
            IController controller, float drawDepth)
        {
            var camera = new Camera3D(id, ActorType.Camera, transform, ProjectionParameters.StandardMediumFiveThree,
                viewPort, drawDepth, StatusType.Update);

            if (controller != null)
                camera.AttachController(controller);

            cameraManager.Add(camera);
        }

        private void InitializeCutsceneCameras()
        {
            Camera3D cloneCamera = null, camera = null;
            var id = "Door Cutscene Camera";
            var viewportDictionaryKey = "full viewport";
            float drawDepth = 0;

            camera = new Camera3D(id, ActorType.Camera, new Transform3D(
                    new Vector3(-70, 1.1f * AppData.CollidableCameraViewHeight + 6, 40),
                    new Vector3(-0.25f, -0.25f, 1), Vector3.UnitY),
                ProjectionParameters.StandardDeepSixteenNine, viewPortDictionary[viewportDictionaryKey], drawDepth,
                StatusType.Update);

            cameraManager.Add(camera);

            cloneCamera = null;

            cloneCamera = new Camera3D("Door Cutscene Camera2", ActorType.Camera, new Transform3D(
                    new Vector3(-120, 1.1f * AppData.CollidableCameraViewHeight + 6, -70),
                    new Vector3(1, -0.25f, -0.4f), Vector3.UnitY),
                ProjectionParameters.StandardDeepSixteenNine, viewPortDictionary[viewportDictionaryKey], drawDepth,
                StatusType.Update);

            cameraManager.Add(cloneCamera);
        }

        private void InitializeCollidableFirstPersonDemo(Integer2 screenResolution)
        {
            Transform3D transform = null;
            var id = "";
            var viewportDictionaryKey = "";
            float drawDepth = 0;

            id = "collidable first person camera";
            viewportDictionaryKey = "full viewport";
            //doesnt matter how high on Y-axis we start the camera since it's collidable and will fall until the capsule toches the ground plane - see AppData::CollidableCameraViewHeight
            //just ensure that the Y-axis height is slightly more than AppData::CollidableCameraViewHeight otherwise the player will rise eerily upwards at the start of the game
            //as the CDCR system pushes the capsule out of the collidable ground plane 
            transform = new Transform3D(new Vector3(-98, 1.1f * AppData.CollidableCameraViewHeight, 104),
                -Vector3.UnitZ, Vector3.UnitY);

            var camera = new Camera3D(id, ActorType.Camera, transform,
                ProjectionParameters.StandardDeepSixteenNine, viewPortDictionary[viewportDictionaryKey], drawDepth,
                StatusType.Update);

            //attach a CollidableFirstPersonController
            camera.AttachController(new CollidableFirstPersonCameraController(
                camera + " controller",
                ControllerType.CollidableFirstPerson,
                AppData.CameraMoveKeys,
                AppData.CollidableCameraMoveSpeed, AppData.CollidableCameraStrafeSpeed, AppData.CameraRotationSpeed,
                managerParameters,
                eventDispatcher,
                camera, //parent
                AppData.CollidableCameraCapsuleRadius,
                AppData.CollidableCameraViewHeight,
                1, 1, //accel, decel
                AppData.CollidableCameraMass,
                AppData.CollidableCameraJumpHeight,
                Vector3.Zero)); //translation offset

            cameraManager.Add(camera);
        }

        #endregion

        #region Events

        private void InitializeEventDispatcher()
        {
            //initialize with an arbitrary size based on the expected number of events per update cycle, increase/reduce where appropriate
            eventDispatcher = new EventDispatcher(this, 20);

            //dont forget to add to the Component list otherwise EventDispatcher::Update won't get called and no event processing will occur!
            Components.Add(eventDispatcher);
        }

        private void StartGame()
        {
            //will be received by the menu manager and screen manager and set the menu to be shown and game to be paused
            EventDispatcher.Publish(new EventData(EventActionType.OnPause, EventCategoryType.MainMenu));


            //publish an event to set the camera
            object[] additionalEventParamsB = {"collidable first person camera 1"};
            EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera,
                additionalEventParamsB));
            //we could also just use the line below, but why not use our event dispatcher?
            //this.cameraManager.SetActiveCamera(x => x.ID.Equals("collidable first person camera 1"));
        }

        /*
        * Any Events That are to be initialised in main will happen in here
        */
        private void InitializeEvents()
        {
            eventDispatcher.InteractChanged += Interactive;
            eventDispatcher.PuzzleChanged += ChangeLights;
            eventDispatcher.RiddleChanged += ChangePopUPState;
            eventDispatcher.RiddleChanged += changeActorType;
            eventDispatcher.PlayerChanged += LoseTriggered;
            eventDispatcher.PlayerWinChanged += WinTriggered;
            eventDispatcher.PopUpChanged += ChangePopUPState;
            eventDispatcher.RiddleAnswerChanged += ChangeRiddleState;
            eventDispatcher.Reset += Reset;
        }

        /*
         * Author: Tomas
         * Object is retrieved from the event and its texture is changed based on what current texture is
         */
        private void Interactive(EventData eventData)
        {
            var actor = eventData.Sender as CollidableObject;
            if (actor.EffectParameters.Texture == textureDictionary["green"])
                actor.EffectParameters.Texture = textureDictionary["gray"];
            else
                actor.EffectParameters.Texture = textureDictionary["green"];
            logicPuzzle.changeState(actor.ID);
        }

        /**
         * Author: Tomas 
         * Used to change the color of lights similar to interactive method but does not 
         * pass the object via event data it must find it via predicate
         */
        private void ChangeLights(EventData eventData)
        {
            var id = (string) eventData.AdditionalParameters[0];
            Predicate<Actor3D> predicate = s => s.GetID() == id;
            var gate = (CollidableObject) objectManager.Find(predicate);
            if (gate.EffectParameters.Texture == textureDictionary["gray"])
                gate.EffectParameters.Texture = textureDictionary["green"];
            else
                gate.EffectParameters.Texture = textureDictionary["gray"];
        }

        /*
         * Author: Tomas 
         *  changes the state of the pop up from down to up also dispatchers an objective to objective manager
         */
        private void ChangePopUPState(EventData eventData)
        {
            Predicate<Actor2D> pred = s => s.ActorType == ActorType.PopUP;
            var item = uiManager.Find(pred) as UITextureObject;


            if (item.StatusType == StatusType.Off)
            {
                item.StatusType = StatusType.Drawn;


                if (objectiveManager.getCurrentObjective() == 1)
                    EventDispatcher.Publish(new EventData(EventActionType.OnObjective, EventCategoryType.Objective));
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
            var item = objectManager.Find(pred);
            item.ActorType = ActorType.CollidablePickup;
        }

        /*
         * Author : Andrew
         *switches the camera to a cutscene camera when the riddle answer object is picked up
         */
        private void ChangeRiddleState(EventData eventData)
        {
            Predicate<Actor3D> pred = s => s.ID == "Riddle Answer";
            var item = objectManager.Find(pred);

            item.StatusType = StatusType.Off;

            EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera,
                new object[] {"Door Cutscene Camera2"}));
            EventDispatcher.Publish(new EventData(EventActionType.RiddleSolved, EventCategoryType.RiddleAnswer));
            EventDispatcher.Publish(new EventData(EventActionType.OnObjective, EventCategoryType.Objective));
            EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Cutscene,
                new object[] {5, "collidable first person camera"}));
        }

        /*
         * Author: Cameron
         * This will be used to trigger different UI effects when the timer runs out
         */
        private void LoseTriggered(EventData eventData)
        {
            EventDispatcher.Publish(new EventData(EventActionType.OnLose, EventCategoryType.MainMenu));
            EventDispatcher.Publish(new EventData(EventActionType.OnLose, EventCategoryType.mouseLock));
            Debug.WriteLine("Lose event triggered");
        }

        /*
         * Author: Cameron
         * This will be used to trigger the end screen with the door opening and a fade to black
         */
        private void WinTriggered(EventData eventData)
        {
            EventDispatcher.Publish(new EventData(EventActionType.OnWin, EventCategoryType.MainMenu));
            EventDispatcher.Publish(new EventData(EventActionType.OnPause, EventCategoryType.MainMenu));
            EventDispatcher.Publish(new EventData(EventActionType.OnWin, EventCategoryType.mouseLock));
        }

        /**
        * Author: Tomas
        * used to reset all objects and cameras
        */
        private void Reset(EventData eventData)
        {
            resetLogicPuzzleModels();
            resetRiddleAnswer();
            resetFPCamera();
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

            for (var i = 1; i < 5; i++)
            {
                Predicate<Actor3D> pred = s => s.ID == "switch-" + i;
                var logicSwitch = (CollidableObject) objectManager.Find(pred);

                logicSwitch.EffectParameters.Texture = textureDictionary["gray"];
            }

            #endregion

            #region Gates

            for (var i = 1; i < 6; i++)
            {
                Predicate<Actor3D> pred = s => s.ID == "gate-" + i;
                var logicGate = (CollidableObject) objectManager.Find(pred);

                logicGate.EffectParameters.Texture = textureDictionary["gray"];
            }

            #endregion
        }

        /**
         * Author: Tomas
         * used to reset Answer Object to Collidable prop
         */
        private void resetRiddleAnswer()
        {
            Predicate<Actor3D> pred = s => s.ID == "Riddle Answer";
            var item = objectManager.Find(pred);
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
            var screenResolution = ScreenUtility.HD720;
            Predicate<Camera3D> pred = s => s.ID == "collidable first person camera";
            cameraManager.Remove(pred);

            InitializeCollidableFirstPersonDemo(screenResolution);


            cameraManager.SetActiveCamera(pred);
        }


        /*
         * Author: Cameron
         * This will be used to reset the lose timer to its default value when reset is called
         */
        private void resetLoseTimer()
        {
            foreach (var timer in timerManager.TimerList)
                if (timer.ID.Equals(AppData.LoseTimerID))
                {
                    timer.Hours = AppData.LoseTimerHours;
                    timer.Minutes = AppData.LoseTimerMinutes;
                    timer.Seconds = AppData.LoseTimerSeconds;
                }
        }

        #endregion

        #region Menu & UI

        private void AddMenuElements()
        {
            Transform2D transform = null;
            Texture2D texture = null;
            var position = Vector2.Zero;
            UIButtonObject uiButtonObject = null, clone = null;
            string sceneID = "", buttonID = "", buttonText = "";
            var verticalBtnSeparation = 50;

            #region Main Menu

            sceneID = "main menu";

            //retrieve the background texture
            texture = textureDictionary["Title-screen"];
            //scale the texture to fit the entire screen
            var scale = new Vector2((float) graphics.PreferredBackBufferWidth / texture.Width,
                (float) graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);

            menuManager.Add(sceneID, new UITextureObject("mainmenuTexture", ActorType.UIStaticTexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));

            //add start button
            buttonID = "startbtn";
            texture = textureDictionary["start"];
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f,
                graphics.PreferredBackBufferHeight - texture.Height);
            transform = new Transform2D(position,
                0, new Vector2(0.6f, 0.4f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.Gray, SpriteEffects.None, 0.1f, texture, buttonText,
                fontDictionary["menu"],
                Color.DarkBlue, new Vector2(0, 0));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2",
                ControllerType.SineScaleLerp,
                new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController",
                ControllerType.SineColorLerp,
                new TrigonometricParameters(1, 0.4f, 0), Color.Blue, Color.DarkBlue));

            menuManager.Add(sceneID, uiButtonObject);

            //add exit button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject) uiButtonObject.Clone();
            clone.ID = "exitbtn";
            clone.Texture = textureDictionary["quit"];
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, verticalBtnSeparation * 1.9f);
            //change the texture blend color
            clone.Color = Color.Gray;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController",
                ControllerType.SineColorLerp,
                new TrigonometricParameters(1, 0.4f, 0), Color.IndianRed, Color.DarkRed));
            menuManager.Add(sceneID, clone);

            #endregion


            clone = null;
            clone = (UIButtonObject) uiButtonObject.Clone();
            clone.ID = "controlsbtn";
            clone.Texture = textureDictionary["controls"];
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(-clone.Texture.Width / 10, verticalBtnSeparation);
            clone.SourceRectangle = new Rectangle(0, 0, clone.Texture.Width, clone.Texture.Height);

            //change the texture blend color
            clone.Color = Color.Gray;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController",
                ControllerType.SineColorLerp,
                new TrigonometricParameters(1, 0.4f, 0), Color.Indigo, Color.Violet));
            menuManager.Add(sceneID, clone);

            #region Pause Menu

            sceneID = "pause menu";

            texture = textureDictionary["PauseMenu"];
            scale = new Vector2((float) graphics.PreferredBackBufferWidth / texture.Width,
                (float) graphics.PreferredBackBufferHeight / texture.Height);

            transform = new Transform2D(scale);
            menuManager.Add(sceneID, new UITextureObject("pauseMenuTexture", ActorType.UIStaticTexture,
                StatusType.Drawn,
                transform, Color.White, SpriteEffects.None, 1, texture));

            clone = null;
            clone = (UIButtonObject) uiButtonObject.Clone();
            clone.ID = "resumebtn";
            clone.Transform = new Transform2D(position,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(clone.Texture.Width / 2.0f, clone.Texture.Height / 2.0f),
                new Integer2(clone.Texture.Width, clone.Texture.Height));
            clone.Texture = textureDictionary["Resume"];
            clone.Transform.Translation -= new Vector2(0, verticalBtnSeparation * 7);
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController",
                ControllerType.SineColorLerp,
                new TrigonometricParameters(1, 0.4f, 0), Color.Blue, Color.DarkBlue));
            menuManager.Add(sceneID, clone);


            clone = (UIButtonObject) uiButtonObject.Clone();
            clone.ID = "exitbtn";
            clone.Texture = textureDictionary["quit"];
            clone.Transform = new Transform2D(position,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(clone.Texture.Width / 2.0f, clone.Texture.Height / 2.0f),
                new Integer2(clone.Texture.Width, clone.Texture.Height));
            clone.Transform.Translation -= new Vector2(0, verticalBtnSeparation * 5);
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController",
                ControllerType.SineColorLerp,
                new TrigonometricParameters(1, 0.4f, 0), Color.IndianRed, Color.Red));

            menuManager.Add(sceneID, clone);

            #endregion

            #region Controls Menu

            sceneID = "controls menu";

            texture = textureDictionary["ControlMenu"];
            scale = new Vector2((float) graphics.PreferredBackBufferWidth / texture.Width,
                (float) graphics.PreferredBackBufferHeight / texture.Height);

            transform = new Transform2D(scale);
            menuManager.Add(sceneID, new UITextureObject("controlMenuTexture", ActorType.UIStaticTexture,
                StatusType.Drawn,
                transform, Color.White, SpriteEffects.None, 1, texture));

            clone = (UIButtonObject) uiButtonObject.Clone();
            clone.ID = "backbtn";
            clone.Texture = textureDictionary["back"];
            //move down on Y-axis for next button
            clone.Transform.Translation = new Vector2(graphics.PreferredBackBufferWidth / 2, clone.Texture.Height / 2);
            clone.SourceRectangle = new Rectangle(0, 0, clone.Texture.Width, clone.Texture.Height);
            clone.Transform = new Transform2D(clone.Transform.Translation,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(clone.Texture.Width / 2.0f, clone.Texture.Height / 2.0f),
                new Integer2(clone.Texture.Width, clone.Texture.Height));
            //change the texture blend color
            clone.Color = Color.Gray;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController",
                ControllerType.SineColorLerp,
                new TrigonometricParameters(1, 0.4f, 0), Color.Indigo, Color.Violet));
            menuManager.Add(sceneID, clone);

            #endregion
        }

        /**
         * Author: Tomas
         * Simple Game Over Menu
         */
        private void AddGameOverMenu()
        {
            string sceneID, buttonID, buttonText;
            var position = Vector2.Zero;
            UIButtonObject uiButtonObject = null, clone = null;
            var verticalBtnSeparation = 100;
            int w, h;

            w = graphics.PreferredBackBufferWidth;
            h = graphics.PreferredBackBufferHeight;
            float a, b, c, d;

            var texture = textureDictionary["game-over"];

            a = (float) w / texture.Width;
            b = (float) h / texture.Height;
            c = 1 / a;
            d = 1 / b;

            Console.WriteLine("width " + w);
            Console.WriteLine("height " + h);
            var scale = new Vector2(a, b);

            var transform = new Transform2D(new Vector2(0, 0), 0, scale, Vector2.One, new Integer2(1, 1));


            var rect = new Rectangle(0, 0, w, h);

            var picture = new UITextureObject("lose-screen-background", ActorType.UIStaticTexture, StatusType.Drawn,
                transform, Color.White,
                SpriteEffects.None, 1, texture);


            sceneID = "lose-screen";
            menuManager.Add(sceneID, picture);

            texture = textureDictionary["restart-Button"];
            buttonID = "restart-Button";
            buttonText = "";
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f,
                graphics.PreferredBackBufferHeight - texture.Height);
            transform = new Transform2D(position,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.Gray, SpriteEffects.None, 0.1f, texture, buttonText,
                fontDictionary["menu"],
                Color.DarkBlue, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2",
                ControllerType.SineScaleLerp,
                new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController",
                ControllerType.SineColorLerp,
                new TrigonometricParameters(1, 0.4f, 0), Color.Green, Color.Green));

            menuManager.Add(sceneID, uiButtonObject);


            clone = (UIButtonObject) uiButtonObject.Clone();
            clone.ID = "exitbtn";
            clone.Texture = textureDictionary["quit"];
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(180, verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.Gray;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController",
                ControllerType.SineColorLerp,
                new TrigonometricParameters(1, 0.4f, 0), Color.IndianRed, Color.DarkRed));
            menuManager.Add(sceneID, clone);
        }

        /**
        * Author: Tomas
        * Simple Win Menu
        */
        private void AddWinMenu()
        {
            string sceneID, buttonID, buttonText;
            var position = Vector2.Zero;
            UIButtonObject uiButtonObject = null, clone = null;
            var verticalBtnSeparation = 100;
            int w, h;

            w = graphics.PreferredBackBufferWidth;
            h = graphics.PreferredBackBufferHeight;
            float a, b, c, d;

            var texture = textureDictionary["win-screen"];

            a = (float) w / texture.Width;
            b = (float) h / texture.Height;
            c = 1 / a;
            d = 1 / b;

            Console.WriteLine("width " + w);
            Console.WriteLine("height " + h);
            var scale = new Vector2(a, b);

            var transform = new Transform2D(new Vector2(0, 0), 0, scale, Vector2.One, new Integer2(1, 1));


            var rect = new Rectangle(0, 0, w, h);

            var picture = new UITextureObject("win-screen-background", ActorType.UIStaticTexture, StatusType.Drawn,
                transform, Color.White,
                SpriteEffects.None, 1, texture);


            sceneID = "win-screen";
            menuManager.Add(sceneID, picture);

            texture = textureDictionary["restart-Button"];
            buttonID = "restart-Button";
            buttonText = "";
            float num = texture.Height / 2;
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f,
                graphics.PreferredBackBufferHeight / 2 + num);
            transform = new Transform2D(position,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.Gray, SpriteEffects.None, 0.1f, texture, buttonText,
                fontDictionary["menu"],
                Color.DarkBlue, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2",
                ControllerType.SineScaleLerp,
                new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController",
                ControllerType.SineColorLerp,
                new TrigonometricParameters(1, 0.4f, 0), Color.Green, Color.Green));

            menuManager.Add(sceneID, uiButtonObject);

            buttonID = "main-Menu";
            texture = textureDictionary["mainMenu-Button"];
            position += new Vector2(-5, verticalBtnSeparation);
            transform = new Transform2D(position,
                0, new Vector2(0.6f, 0.6f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.Gray, SpriteEffects.None, 0.1f, texture, buttonText,
                fontDictionary["menu"],
                Color.DarkBlue, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2",
                ControllerType.SineScaleLerp,
                new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController",
                ControllerType.SineColorLerp,
                new TrigonometricParameters(1, 0.4f, 0), Color.White, Color.Cyan));

            menuManager.Add(sceneID, uiButtonObject);


            buttonID = "exitbtn";
            texture = textureDictionary["quit"];
            position += new Vector2(-0, verticalBtnSeparation);
            transform = new Transform2D(position,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.Gray, SpriteEffects.None, 0.1f, texture, buttonText,
                fontDictionary["menu"],
                Color.DarkBlue, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2",
                ControllerType.SineScaleLerp,
                new TrigonometricParameters(0.1f, 0.2f, 1)));
            uiButtonObject.AttachController(new UIColorSineLerpController("colorSineLerpController",
                ControllerType.SineColorLerp,
                new TrigonometricParameters(1, 0.4f, 0), Color.DarkRed, Color.Red));

            menuManager.Add(sceneID, uiButtonObject);
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

            //#if DEBUG
            //            InitializeDebugTextInfo();
            //#endif
        }

        protected override void UnloadContent()
        {
            //formally call garbage collection on all ContentDictionary objects to de-allocate resources from RAM
            modelDictionary.Dispose();
            textureDictionary.Dispose();
            fontDictionary.Dispose();
            videoDictionary.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            if (keyboardManager.IsKeyDown(Keys.P))
                //EventDispatcher.Publish(new EventData(EventActionType.OnRestart,EventCategoryType.Reset));
                // EventDispatcher.Publish(new EventData(EventActionType.OpenDoor,EventCategoryType.Animator));
                //EventDispatcher.Publish(new EventData(EventActionType.OpenBookcase, EventCategoryType.Animator));
                //EventDispatcher.Publish(new EventData(EventActionType.RotateTopBarrier, EventCategoryType.Animator));
                //EventDispatcher.Publish(new EventData(EventActionType.RotateBottomBarrier, EventCategoryType.Animator));
                EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera,
                    new object[] {"collidable first person camera"}));
            //exit using new gamepad manager
            if (gamePadManager.IsPlayerConnected(PlayerIndex.One) &&
                gamePadManager.IsButtonPressed(PlayerIndex.One, Buttons.Back))
                Exit();
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