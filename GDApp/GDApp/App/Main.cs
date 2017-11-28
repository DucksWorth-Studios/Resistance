#define DEMO

using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JigLibX.Geometry;
using JigLibX.Collision;
using System.Collections.Generic;
using System;
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

        private ManagerParameters managerParameters;

        //demo remove later
        private HeroPlayerObject heroPlayerObject;
        private ModelObject drivableBoxObject;
        private HeroAnimatedPlayerObject animatedHeroPlayerObject;

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

            //set the title
            Window.Title = "3DGD - My Amazing Game 1.0";

            //Initialize EventDispatcher
            InitializeEventDispatcher();

            //Load Dictionaries, Media Assets and Non-media Assets
            LoadDictionaries();
            LoadAssets();
            LoadCurvesAndRails();
            LoadViewports(screenResolution);

            //Initialize Effects
            InitializeEffects();

            //Initialize the Managers
            InitializeManagers(screenResolution, screenType, isMouseVisible, numberOfGamePadPlayers);

            //add menu and UI elements
            AddMenuElements();
            AddUIElements();
#if DEBUG
            InitializeDebugTextInfo();
#endif

            //load game happens before cameras are loaded because we may add a third person camera that needs a reference to a loaded Actor
            LoadGame(gameLevel);

            //Initialize cameras based on the desired screen layout
            if (screenType == ScreenUtility.ScreenType.SingleScreen)
            {
                InitializeCollidableFirstPersonDemo(screenResolution);
                //or
                //InitializeSingleScreenCycleableCameraDemo(screenResolution);
                //or
                InitializeCollidableThirdPersonDemo(screenResolution);
            }
            else if (screenType == ScreenUtility.ScreenType.MultiScreen)//multi-screen
            {
                InitializeMultiScreenCameraDemo(screenResolution);
            }

            //Publish Start Event(s)
            StartGame();

#if DEBUG
            InitializeDebugCollisionSkinInfo();
#endif

            base.Initialize();
        }

        private void InitializeManagers(Integer2 screenResolution, 
            ScreenUtility.ScreenType screenType, bool isMouseVisible, int numberOfGamePadPlayers) //1 - 4
        {
            //add sound manager
            this.soundManager = new SoundManager(this, this.eventDispatcher, StatusType.Update, "Content/Assets/Audio/", "Demo2DSound.xgs", "WaveBank1.xwb", "SoundBank1.xsb");
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
            this.menuManager.SetActiveList("mainmenu");
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
                PickingBehaviourType.PickAndPlace, AppData.PickStartDistance, AppData.PickEndDistance, collisionPredicate);
            Components.Add(this.pickingManager);
            #endregion


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

        }

        private void LoadAssets()
        {
            #region Models
            //geometric samples
            this.modelDictionary.Load("Assets/Models/plane1", "plane1");
            this.modelDictionary.Load("Assets/Models/plane", "plane");
            this.modelDictionary.Load("Assets/Models/box2", "box2");
            this.modelDictionary.Load("Assets/Models/torus");
            this.modelDictionary.Load("Assets/Models/sphere");

            //triangle mesh high/low poly demo
            this.modelDictionary.Load("Assets/Models/teapot");
            this.modelDictionary.Load("Assets/Models/teapot_mediumpoly");
            this.modelDictionary.Load("Assets/Models/teapot_lowpoly");

            //player - replace with animation eventually
            this.modelDictionary.Load("Assets/Models/cylinder");

            //architecture
            this.modelDictionary.Load("Assets/Models/Architecture/Buildings/house");
            this.modelDictionary.Load("Assets/Models/Architecture/Walls/wall");
            this.modelDictionary.Load("Assets/Models/Landscape/canyon");

            //dual texture demo
            this.modelDictionary.Load("Assets/Models/box");
            this.modelDictionary.Load("Assets/Models/box1");
            #endregion

            #region Textures
            //environment
            this.textureDictionary.Load("Assets/Textures/Props/Crates/crate1"); //demo use of the shorter form of Load() that generates key from asset name
            this.textureDictionary.Load("Assets/Textures/Props/Crates/crate2");
            this.textureDictionary.Load("Assets/Debug/Textures/checkerboard");
            this.textureDictionary.Load("Assets/Textures/Foliage/Ground/grass1");
            this.textureDictionary.Load("Assets/Textures/Skybox/back");
            this.textureDictionary.Load("Assets/Textures/Skybox/left");
            this.textureDictionary.Load("Assets/Textures/Skybox/right");
            this.textureDictionary.Load("Assets/Textures/Skybox/sky");
            this.textureDictionary.Load("Assets/Textures/Skybox/front");
            this.textureDictionary.Load("Assets/Textures/Foliage/Trees/tree2");

            //dual texture demo
            this.textureDictionary.Load("Assets/Textures/Foliage/Ground/grass_midlevel");
            this.textureDictionary.Load("Assets/Textures/Foliage/Ground/grass_highlevel");

            //menu - buttons
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/genericbtn");

            //menu - backgrounds
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/mainmenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/audiomenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/controlsmenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/exitmenuwithtrans");

            //ui (or hud) elements
            this.textureDictionary.Load("Assets/Textures/UI/HUD/reticuleDefault");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/progress_gradient");

            //architecture
            this.textureDictionary.Load("Assets/Textures/Architecture/Buildings/house-low-texture");
            this.textureDictionary.Load("Assets/Textures/Architecture/Walls/wall");
            this.textureDictionary.Load("Assets/Textures/Landscape/canyon");

            //dual texture demo - see Main::InitializeCollidableGround()
            this.textureDictionary.Load("Assets/Debug/Textures/checkerboard_greywhite");
            

#if DEBUG
            //demo
            this.textureDictionary.Load("Assets/Debug/Textures/ml");
            this.textureDictionary.Load("Assets/Debug/Textures/checkerboard");
#endif
            #endregion

            #region Fonts
#if DEBUG
            this.fontDictionary.Load("Assets/Debug/Fonts/debug");
#endif
            this.fontDictionary.Load("Assets/Fonts/menu");
            this.fontDictionary.Load("Assets/Fonts/mouse");
            #endregion

            #region Video
            this.videoDictionary.Load("Assets/Video/sample");
            #endregion

            #region Animations
            //contains a single animation "Take001"
            this.modelDictionary.Load("Assets/Models/Animated/dude");

            //squirrel - one file per animation
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Idle");
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Jump");
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Punch");
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Standing");
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/Red_Tailwhip");
            this.modelDictionary.Load("Assets/Models/Animated/Squirrel/RedRun4");
            #endregion

        }

        private void LoadCurvesAndRails()
        {
            int cameraHeight = 5;

            #region Curves
            //create the camera curve to be applied to the track controller
            Transform3DCurve curveA = new Transform3DCurve(CurveLoopType.Oscillate); //experiment with other CurveLoopTypes
            curveA.Add(new Vector3(40, cameraHeight, 80), -Vector3.UnitX, Vector3.UnitY, 0); //start position
            curveA.Add(new Vector3(0, 10, 60), -Vector3.UnitZ, Vector3.UnitY, 4);
            curveA.Add(new Vector3(0, 40, 0), -Vector3.UnitY, -Vector3.UnitZ, 8); //curve mid-point
            curveA.Add(new Vector3(0, 10, 60), -Vector3.UnitZ, Vector3.UnitY, 12);
            curveA.Add(new Vector3(40, cameraHeight, 80), -Vector3.UnitX, Vector3.UnitY, 16); //end position - same as start for zero-discontinuity on cycle
            //add to the dictionary
            this.curveDictionary.Add("unique curve name 1", curveA);
            #endregion

            #region Rails
            //create the track to be applied to the non-collidable track camera 1
            this.railDictionary.Add("rail1 - parallel to x-axis", new RailParameters("rail1 - parallel to x-axis", new Vector3(-80, 10, 40), new Vector3(80, 10, 40)));
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
                verticalOffset,  viewPortDimensions.X, viewPortDimensions.Y));
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
            int worldScale = 250;

            //Non - collidable
            InitializeNonCollidableSkyBox(worldScale);
            InitializeNonCollidableFoliage(worldScale);
            InitializeNonCollidableDriveableObject();
            InitializeNonCollidableDecoratorObjects();

            ////Collidable
            InitializeCollidableGround(worldScale);
            //demo high vertex count trianglemesh
            InitializeStaticCollidableTriangleMeshObjects();
            //demo medium and low vertex count trianglemesh
            InitializeStaticCollidableMediumPolyTriangleMeshObjects();
            InitializeStaticCollidableLowPolyTriangleMeshObjects();
            //demo dynamic collidable objects with user-defined collision primitives
            InitializeDynamicCollidableObjects();

            ////adds the hero of the game - see InitializeSingleScreenFirstThirdPersonDemo()
            InitializeCollidableHeroPlayerObject();

            ////add level elements
            InitializeBuildings();
            InitializeWallsFences();

            //add video display
            InitializeVideoDisplay();

            //add animated characters
            InitializeAnimatedPlayer();

        }

        private void InitializeAnimatedPlayer()
        {
            Transform3D transform3D = null;

            transform3D = new Transform3D(new Vector3(0, 20, 30),
                new Vector3(-90, 0, 0), //y-z are reversed because the capsule is rotation by 90 degrees around X-axis - See CharacterObject constructor
                 0.1f * Vector3.One, 
                 -Vector3.UnitZ, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            //if we dont specify a texture then the object manager will draw using whatever textures were baked into the animation in 3DS Max
            effectParameters.Texture = null;

            //dictionary that stores all the animation FBX files (e.g. walk.fbx, run.fbx)
            Dictionary<string, Model> animatedModelTakeDictionary = new Dictionary<string, Model>();

            //the dude model comes only with one animation in one FBX file so the statement below looks a little silly
            //notice how I add the model by the name of the take as specified in the 3DS Max file (e.g. "jump", this.modelDictionary["Red_Jump"])
            animatedModelTakeDictionary.Add("Take 001", this.modelDictionary["dude"]);

            this.animatedHeroPlayerObject = new HeroAnimatedPlayerObject("the dude",
                ActorType.Player, transform3D, 
                effectParameters, 
                AppData.PlayerOneMoveKeys,
                AppData.PlayerRadius, 
                AppData.PlayerHeight,
                1, 1,  //accel, decel
                AppData.PlayerMoveSpeed,
                AppData.PlayerRotationSpeed,               
                AppData.PlayerJumpHeight, 
                new Vector3(0, -3.5f, 0), //offset inside capsule
                this.keyboardManager, 
                "Take 001", //start take
                animatedModelTakeDictionary);
            this.animatedHeroPlayerObject.Enable(false, AppData.PlayerMass);

            this.objectManager.Add(animatedHeroPlayerObject);

        }

        private void InitializeCollidableHeroPlayerObject()
        {
            Transform3D transform = new Transform3D(new Vector3(0, 25, 20), 
                new Vector3(90,0,0),
                new Vector3(1,3.5f,1), -Vector3.UnitZ, Vector3.UnitY);

            //clone the dictionary effect and set unique properties for the hero player object
            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["checkerboard"];

            //make the hero a field since we need to point the third person camera controller at this object
            this.heroPlayerObject = new HeroPlayerObject(AppData.PlayerOneID, 
                AppData.PlayerOneProgressControllerID, //used to increment/decrement progress on pickup, win, or lose
                ActorType.Player, transform, 
                effectParameters,  
                this.modelDictionary["cylinder"], 
                AppData.PlayerTwoMoveKeys,
                AppData.PlayerRadius, AppData.PlayerHeight,
                1.8f, 1.7f,
                AppData.PlayerJumpHeight,
                new Vector3(0, 5, 0), 
                this.keyboardManager);
            this.heroPlayerObject.Enable(false, AppData.PlayerMass);

            //don't forget to add it - or else we wont see it!
            this.objectManager.Add(this.heroPlayerObject);
        }

        //skybox is a non-collidable series of ModelObjects with no lighting
        private void InitializeNonCollidableSkyBox(int worldScale)
        {
            //first we will create a prototype plane and then simply clone it for each of the skybox decorator elements (e.g. ground, front, top etc). 
            Transform3D transform = new Transform3D(new Vector3(0, 0, 0), new Vector3(worldScale, 1, worldScale));

            //clone the dictionary effect and set unique properties for the hero player object
            BasicEffectParameters effectParameters = this.effectDictionary["unlitModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["checkerboard"];

            //create a archetype to use for cloning
            ModelObject planePrototypeModelObject = new ModelObject("plane1", ActorType.Decorator, transform, effectParameters, this.modelDictionary["plane1"]);

            //will be re-used for all planes
            ModelObject clonePlane = null;

            #region Skybox
            //add the back skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["back"];
            //rotate the default plane 90 degrees around the X-axis (use the thumb and curled fingers of your right hand to determine +ve or -ve rotation value)
            clonePlane.Transform.Rotation = new Vector3(90, 0, 0);
            
            /*
             * Move the plane back to meet with the back edge of the grass (by based on the original 3DS Max model scale)
             * Note:
             * - the interaction between 3DS Max and XNA units which result in the scale factor used below (i.e. 1 x 2.54 x worldScale)/2
             * - that I move the plane down a little on the Y-axiz, purely for aesthetic purposes
             */
            clonePlane.Transform.Translation = new Vector3(0, 0, (-2.54f * worldScale) / 2.0f);
            this.objectManager.Add(clonePlane);

            //As an exercise the student should add the remaining 4 skybox planes here by repeating the clone, texture assignment, rotation, and translation steps above...
            //add the left skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["left"];
            clonePlane.Transform.Rotation = new Vector3(90, 90, 0);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 2.0f, 0, 0);
            this.objectManager.Add(clonePlane);

            //add the right skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["right"];
            clonePlane.Transform.Rotation = new Vector3(90, -90, 0);
            clonePlane.Transform.Translation = new Vector3((2.54f * worldScale) / 2.0f, 0, 0);
            this.objectManager.Add(clonePlane);

            //add the top skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["sky"];
            //notice the combination of rotations to correctly align the sky texture with the sides
            clonePlane.Transform.Rotation = new Vector3(180, -90, 0);
            clonePlane.Transform.Translation = new Vector3(0, ((2.54f * worldScale) / 2.0f), 0);
            this.objectManager.Add(clonePlane);

            //add the front skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["front"];
            clonePlane.Transform.Rotation = new Vector3(-90, 0, 180);
            clonePlane.Transform.Translation = new Vector3(0, 0, (2.54f * worldScale) / 2.0f);
            this.objectManager.Add(clonePlane);
            #endregion
        }

        //tree is a non-collidable ModelObject (i.e. in final game the player wont ever reach the far-distance tree) with no-lighting
        private void InitializeNonCollidableFoliage(int worldScale)
        {
            //first we will create a prototype plane and then simply clone it for each of the decorator elements (e.g. trees etc). 
            Transform3D transform = new Transform3D(new Vector3(0, 0, 0), new Vector3(worldScale, 1, worldScale));

            //clone the dictionary effect and set unique properties for the hero player object
            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["checkerboard"];
            //a fix to ensure that any image containing transparent pixels will be sent to the correct draw list in ObjectManager
            effectParameters.Alpha = 0.99f;

            ModelObject planePrototypeModelObject = new ModelObject("plane1", ActorType.Decorator, transform, effectParameters, this.modelDictionary["plane1"]);

            //will be re-used for all planes
            ModelObject clonePlane = null;

            //tree
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["tree2"];
            clonePlane.Transform.Rotation = new Vector3(90, 0, 0);
            /*
             * ISRoT - Scale operations are applied before rotation in XNA so to make the tree tall (i.e. 10) we actually scale 
             * along the Z-axis (remember the original plane is flat on the XZ axis) and then flip the plane to stand upright.
             */
            clonePlane.Transform.Scale = new Vector3(5, 1, 10);
            //y-displacement is (10(XNA) x 2.54f(3DS Max))/2 = 12.7f
            clonePlane.Transform.Translation = new Vector3(0, ((clonePlane.Transform.Scale.Z * 2.54f) / 2), -20);
            this.objectManager.Add(clonePlane);
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
             * See https://www.youtube.com/watch?v=AqiNpRmENIQ&t=1892s
             * 
             */
            Model model = this.modelDictionary["box2"];

            //clone the dictionary effect and set unique properties for the hero player object
            DualTextureEffectParameters effectParameters = this.effectDictionary["unlitModelDualEffect"].Clone() as DualTextureEffectParameters;
            effectParameters.Texture = this.textureDictionary["grass1"];
            effectParameters.Texture2 = this.textureDictionary["checkerboard_greywhite"];

            //BasicEffectParameters effectParameters = this.effectDictionary["unlitModelBasicEffect"].GetDeepCopy() as BasicEffectParameters;
            //effectParameters.Texture = this.textureDictionary["grass1"];
            //effectParameters.DiffuseColor = Color.White;

            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero, new Vector3(worldScale, 0.001f, worldScale), Vector3.UnitX, Vector3.UnitY);

            collidableObject = new CollidableObject("ground", ActorType.CollidableGround, transform3D, effectParameters,  model);
            collidableObject.AddPrimitive(new JigLibX.Geometry.Plane(transform3D.Up, transform3D.Translation), new MaterialProperties(0.8f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1); //change to false, see what happens.
            this.objectManager.Add(collidableObject);
        }

        //Triangle mesh objects wrap a tight collision surface around complex shapes - the downside is that TriangleMeshObjects CANNOT be moved
        private void InitializeStaticCollidableTriangleMeshObjects()
        {
            Transform3D transform3D = new Transform3D(new Vector3(-50, 10, 0), new Vector3(45, 45, 0), 0.1f * Vector3.One, Vector3.UnitX, Vector3.UnitY);
            //clone the dictionary effect and set unique properties for the hero player object

            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["ml"];
            effectParameters.DiffuseColor = Color.White;
            effectParameters.SpecularPower = 32; //pow((N, H), SpecularPower)
            effectParameters.EmissiveColor = Color.Red;

            CollidableObject collidableObject = new TriangleMeshObject("torus", ActorType.CollidableProp, transform3D, effectParameters,
                            this.modelDictionary["torus"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        //Demos use of a low-polygon model to generate the triangle mesh collision skin - saving CPU cycles on CDCR checking
        private void InitializeStaticCollidableMediumPolyTriangleMeshObjects()
        {
            Transform3D transform3D = new Transform3D(new Vector3(-30, 3, 0),
                new Vector3(0, 0, 0), 0.08f * Vector3.One, Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["checkerboard"];

            CollidableObject collidableObject = new TriangleMeshObject("teapot", ActorType.CollidableProp, transform3D, effectParameters, 
                        this.modelDictionary["teapot"], this.modelDictionary["teapot_mediumpoly"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        //Demos use of a low-polygon model to generate the triangle mesh collision skin - saving CPU cycles on CDCR checking
        private void InitializeStaticCollidableLowPolyTriangleMeshObjects()
        {
            Transform3D transform3D = new Transform3D(new Vector3(-10, 3, 0),
                new Vector3(0, 0, 0), 0.08f * Vector3.One, Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["checkerboard"];
            //lets set the diffuse color also, for fun.
            effectParameters.DiffuseColor = Color.Blue;

            CollidableObject collidableObject = new TriangleMeshObject("teapot", ActorType.CollidableProp, transform3D, effectParameters, 
                this.modelDictionary["teapot"], this.modelDictionary["teapot_lowpoly"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        //if you want objects to be collidable AND moveable then you must attach either a box, sphere, or capsule primitives to the object
        private void InitializeDynamicCollidableObjects()
        {
            CollidableObject collidableObject, archetypeCollidableObject = null;
            Model model = null;

            #region Spheres
            model = this.modelDictionary["sphere"];
            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["checkerboard"];

            //make once then clone
            archetypeCollidableObject = new CollidableObject("sphere ", ActorType.CollidablePickup, Transform3D.Zero, effectParameters, model);

            for (int i = 0; i < 10; i++)
            {
                collidableObject = (CollidableObject)archetypeCollidableObject.Clone();

                collidableObject.ID += i;
                collidableObject.Transform = new Transform3D(new Vector3(-50, 100 + 10 * i, i), new Vector3(0, 0, 0),
                    0.082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                    Vector3.UnitX, Vector3.UnitY);

                collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 2.54f), new MaterialProperties(0.2f, 0.8f, 0.7f));
                collidableObject.Enable(false, 1);
                this.objectManager.Add(collidableObject);
            }
            #endregion

            #region Box
            model = this.modelDictionary["box2"];
            effectParameters = (this.effectDictionary["litModelBasicEffect"] as BasicEffectParameters).Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["crate2"];
            //make once then clone
            archetypeCollidableObject = new CollidableObject("box - ", ActorType.CollidablePickup, Transform3D.Zero, effectParameters, model);

            int count = 0;
            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 5; j++)
                {
                    collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
                    collidableObject.ID += count;
                    count++;

                    collidableObject.Transform = new Transform3D(new Vector3(25 + 5 * j, 15 + 10 * i, 0), new Vector3(0, 0, 0), new Vector3(2, 4, 1), Vector3.UnitX, Vector3.UnitY);
                    collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, /*important do not change - cm to inch*/2.54f * collidableObject.Transform.Scale), new MaterialProperties(0.2f, 0.8f, 0.7f));

                    //increase the mass of the boxes in the demo to see how collidable first person camera interacts vs. spheres (at mass = 1)
                    collidableObject.Enable(false, 1);
                    this.objectManager.Add(collidableObject);
                }
            }

            #endregion
        }

        //demo of a non-collidable ModelObject with attached third person controller
        private void InitializeNonCollidableDriveableObject()
        {
            //place the drivable model to the left of the existing models and specify that forward movement is along the -ve z-axis
            Transform3D transform = new Transform3D(new Vector3(-10, 5, 25), -Vector3.UnitZ, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["crate1"];
            effectParameters.DiffuseColor = Color.Gold;

            //initialise the drivable model object - we've made this variable a field to allow it to be visible to the rail camera controller - see InitializeCameras()
            this.drivableBoxObject = new ModelObject("drivable box1", ActorType.Player, transform, effectParameters, this.modelDictionary["box2"]);

            //attach a DriveController
            drivableBoxObject.AttachController(new DriveController("driveController1", ControllerType.Drive,
                AppData.PlayerOneMoveKeys, AppData.PlayerMoveSpeed, AppData.PlayerStrafeSpeed, AppData.PlayerRotationSpeed,
                this.managerParameters));

            //add to the objectManager so that it will be drawn and updated
            this.objectManager.Add(drivableBoxObject);
        }

        //demo of some semi-transparent non-collidable ModelObjects
        private void InitializeNonCollidableDecoratorObjects()
        {
            //position the object
            Transform3D transform = new Transform3D(new Vector3(0, 5, 0), Vector3.Zero, Vector3.One, Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["crate1"];
            effectParameters.DiffuseColor = Color.Gold;
            effectParameters.Alpha = 0.5f;

            //initialise the boxObject
            ModelObject boxObject = new ModelObject("some box 1", ActorType.Decorator, transform, effectParameters, this.modelDictionary["box2"]);
            //add to the objectManager so that it will be drawn and updated
            this.objectManager.Add(boxObject);

            //a clone variable that we can reuse
            ModelObject clone = null;

            //add a clone of the box model object to test the clone
            clone = (ModelObject)boxObject.Clone();
            clone.Transform.Translation = new Vector3(5, 5, 0);
            //scale it to make it look different
            clone.Transform.Scale = new Vector3(1, 4, 1);
            //change its color
            clone.EffectParameters.DiffuseColor = Color.Red;
            this.objectManager.Add(clone);

            //add more clones here...
        }

        private void InitializeBuildings()
        {
            Transform3D transform3D = new Transform3D(new Vector3(-100, 0, 0),
                new Vector3(0, 90, 0), 0.4f * Vector3.One, Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["house-low-texture"];

            CollidableObject collidableObject = new TriangleMeshObject("house1", ActorType.CollidableArchitecture, transform3D, 
                                effectParameters, this.modelDictionary["house"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        private void InitializeWallsFences()
        {
            Transform3D transform3D = new Transform3D(new Vector3(-140, 0, -14),
                new Vector3(0, -90, 0), 0.4f * Vector3.One, Vector3.UnitX, Vector3.UnitY);

            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["wall"];

            CollidableObject collidableObject = new TriangleMeshObject("wall1", ActorType.CollidableArchitecture, transform3D, 
                            effectParameters, this.modelDictionary["wall"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }
  
        private void InitializeVideoDisplay()
        {
     
            BasicEffectParameters effectParameters = this.effectDictionary["litModelBasicEffect"].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["checkerboard"];
            //make the screen really shiny
            effectParameters.SpecularPower = 256;

            //put the display up on the Y-axis, obviously we can rotate by setting transform3D.Rotation
            Transform3D transform3D = new Transform3D(new Vector3(0, 20, 0), new Vector3(16, 10, 0.1f));

            /* 
             * Does the display need to be collidable? if so use a CollidableObject and not a ModelObject.
             * Notice we dont pass in a texture since the video controller will set this.
             */
            ModelObject videoModelObject = new ModelObject(AppData.VideoIDMainHall, ActorType.Decorator, 
                transform3D, effectParameters, this.modelDictionary["box"]);

            ////create the controller
            VideoController videoController = new VideoController(AppData.VideoIDMainHall + " video " + AppData.ControllerIDSuffix, ControllerType.Video, this.eventDispatcher,
                this.textureDictionary["checkerboard"], this.videoDictionary["sample"], 0.5f);

            //make it rotate like a commercial video display
            videoModelObject.AttachController(new RotationController(AppData.VideoIDMainHall + " rotation " + AppData.ControllerIDSuffix,
                ControllerType.Rotation, new Vector3(0, 0.02f, 0)));
            //attach
            videoModelObject.AttachController(videoController);
            //add to object manager
            this.objectManager.Add(videoModelObject);
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

        private void InitializeMultiScreenCameraDemo(Integer2 screenResolution)
        {
            Transform3D transform = null;
            IController controller = null;
            string id = "";
            string viewportDictionaryKey = "";
            int cameraHeight = 5;

            //non-collidable camera 1
            id = "non-collidable FPC 1";
            viewportDictionaryKey = "column1 row0";
            transform = new Transform3D(new Vector3(0, cameraHeight, 10), -Vector3.UnitZ, Vector3.UnitY);
            controller = new FirstPersonCameraController(id + " controller", ControllerType.FirstPerson, AppData.CameraMoveKeys, AppData.CameraMoveSpeed, AppData.CameraStrafeSpeed, AppData.CameraRotationSpeed,
                this.managerParameters);
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller, 0);

            //security camera 1
            id = "non-collidable security 1";
            viewportDictionaryKey = "column0 row0";
            transform = new Transform3D(new Vector3(0, cameraHeight, 20), -Vector3.UnitZ, Vector3.UnitY);
            controller = new SecurityCameraController(id + " controller", ControllerType.Security, 60, AppData.SecurityCameraRotationSpeedSlow, AppData.SecurityCameraRotationAxisYaw);
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller, 0);

            //security camera 2
            id = "non-collidable security 2";
            viewportDictionaryKey = "column0 row1";
            transform = new Transform3D(new Vector3(0, cameraHeight, 20), -Vector3.UnitZ, Vector3.UnitY);
            controller = new SecurityCameraController(id + " controller", ControllerType.Security, 45, AppData.SecurityCameraRotationSpeedMedium, new Vector3(1, 1, 0));
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller, 0);

            //security camera 3
            id = "non-collidable security 3";
            viewportDictionaryKey = "column0 row2";
            transform = new Transform3D(new Vector3(0, cameraHeight, 20), -Vector3.UnitZ, Vector3.UnitY);
            controller = new SecurityCameraController(id + " controller", ControllerType.Security, 30, AppData.SecurityCameraRotationSpeedFast, new Vector3(4, 1, 0));
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller, 0);

            //track camera 1
            id = "non-collidable track 1";
            viewportDictionaryKey = "column0 row3";
            transform = new Transform3D(new Vector3(0, cameraHeight, 20), -Vector3.UnitZ, Vector3.UnitY);
            controller = new CurveController(id + " controller", ControllerType.Track, this.curveDictionary["unique curve name 1"], PlayStatusType.Play);
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller, 0);

            //rail camera 1
            id = "non-collidable rail 1";
            viewportDictionaryKey = "column0 row4";
            //since the camera will be set on a rail it doesnt matter what the initial transform is set to
            transform = Transform3D.Zero;

            //track animated player if it's available
            if(this.animatedHeroPlayerObject != null)
                controller = new RailController(id + " controller", ControllerType.Rail, this.animatedHeroPlayerObject, this.railDictionary["rail1 - parallel to x-axis"]);
            else
                controller = new RailController(id + " controller", ControllerType.Rail, this.drivableBoxObject, this.railDictionary["rail1 - parallel to x-axis"]);



            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller, 0);
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
            transform = new Transform3D(new Vector3(0, 1.1f * AppData.CollidableCameraViewHeight, 60), -Vector3.UnitZ, Vector3.UnitY);

            Camera3D camera = new Camera3D(id, ActorType.Camera, transform,
                    ProjectionParameters.StandardDeepSixteenNine, this.viewPortDictionary[viewportDictionaryKey], drawDepth, StatusType.Update);

            //attach a CollidableFirstPersonController
            camera.AttachController(new CollidableFirstPersonCameraController(
                    camera + " controller",
                    ControllerType.CollidableFirstPerson,
                    AppData.CameraMoveKeys,
                    AppData.CollidableCameraMoveSpeed, AppData.CollidableCameraStrafeSpeed, AppData.CameraRotationSpeed,
                    this.managerParameters,
                    camera, //parent
                    AppData.CollidableCameraCapsuleRadius,
                    AppData.CollidableCameraViewHeight,
                    1, 1, //accel, decel
                    AppData.CollidableCameraMass,
                    AppData.CollidableCameraJumpHeight,
                    Vector3.Zero)); //translation offset

            this.cameraManager.Add(camera);
        }

        //adds three camera from 3 different perspectives that we can cycle through
        private void InitializeSingleScreenCycleableCameraDemo(Integer2 screenResolution)
        {

            InitializeCollidableFirstPersonDemo(screenResolution);

            Transform3D transform = null;
            IController controller = null;
            string id = "";
            string viewportDictionaryKey = "full viewport";

            //track camera 1
            id = "non-collidable track 1";
            transform = new Transform3D(new Vector3(0, 0, 20), -Vector3.UnitZ, Vector3.UnitY);
            controller = new CurveController(id + " controller", ControllerType.Track, this.curveDictionary["unique curve name 1"], PlayStatusType.Play);
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller, 0);

            //rail camera 1
            id = "non-collidable rail 1";
            //since the camera will be set on a rail it doesnt matter what the initial transform is set to
            transform = Transform3D.Zero;

            //track animated player if it's available
            if (this.animatedHeroPlayerObject != null)
                controller = new RailController(id + " controller", ControllerType.Rail, this.animatedHeroPlayerObject, this.railDictionary["rail1 - parallel to x-axis"]);
            else
                controller = new RailController(id + " controller", ControllerType.Rail, this.drivableBoxObject, this.railDictionary["rail1 - parallel to x-axis"]);


            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller, 0);

        }

        //adds a third person looking at collidable HeroPlayerObject
        private void InitializeCollidableThirdPersonDemo(Integer2 screenResolution)
        {
            Transform3D transform = null;
            IController controller = null;
            string id = "";
            string viewportDictionaryKey = "full viewport";

            //track camera 1
            id = "third person collidable";
            //doesnt matter since it will reset based on target actor data
            transform = Transform3D.Zero;

            if (this.animatedHeroPlayerObject != null)
            {
                controller = new ThirdPersonController(id + "ctrllr", ControllerType.ThirdPerson, this.animatedHeroPlayerObject,
                AppData.CameraThirdPersonDistance, AppData.CameraThirdPersonScrollSpeedDistanceMultiplier,
                AppData.CameraThirdPersonElevationAngleInDegrees, AppData.CameraThirdPersonScrollSpeedElevatationMultiplier,
                LerpSpeed.Fast, LerpSpeed.Medium, this.mouseManager);
            }
            else
            {
              controller = new ThirdPersonController(id + "ctrllr", ControllerType.ThirdPerson, this.heroPlayerObject,
              AppData.CameraThirdPersonDistance, AppData.CameraThirdPersonScrollSpeedDistanceMultiplier,
              AppData.CameraThirdPersonElevationAngleInDegrees, AppData.CameraThirdPersonScrollSpeedElevatationMultiplier,
              LerpSpeed.Fast, LerpSpeed.Medium, this.mouseManager);
            }


            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller, 0);

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
            object[] additionalEventParamsB = { "collidable first person camera 1"};
            EventDispatcher.Publish(new EventData(EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalEventParamsB));
            //we could also just use the line below, but why not use our event dispatcher?
            //this.cameraManager.SetActiveCamera(x => x.ID.Equals("collidable first person camera 1"));
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
            int verticalBtnSeparation = 50;

            #region Main Menu
            sceneID = "main menu";

            //retrieve the background texture
            texture = this.textureDictionary["mainmenu"];
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
            buttonText = "Start";
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200);
            texture = this.textureDictionary["genericbtn"];
            transform = new Transform2D(position,
                0, new Vector2(1.8f, 0.6f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.LightPink, SpriteEffects.None, 0.1f, texture, buttonText,
                this.fontDictionary["menu"],
                Color.DarkGray, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            this.menuManager.Add(sceneID, uiButtonObject);


            //add audio button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "audiobtn";
            clone.Text = "Audio";
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.LightGreen;
            this.menuManager.Add(sceneID, clone);

            //add controls button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "controlsbtn";
            clone.Text = "Controls";
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 2 * verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.LightBlue;
            this.menuManager.Add(sceneID, clone);

            //add exit button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "exitbtn";
            clone.Text = "Exit";
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 3 * verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.LightYellow;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.LightSeaGreen, Color.LightGreen));
            this.menuManager.Add(sceneID, clone);
            #endregion

            #region Audio Menu
            sceneID = "audio menu";

            //retrieve the audio menu background texture
            texture = this.textureDictionary["audiomenu"];
            //scale the texture to fit the entire screen
            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("audiomenuTexture", ActorType.UIStaticTexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));


            //add volume up button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "volumeUpbtn";
            clone.Text = "Volume Up";
            //change the texture blend color
            clone.Color = Color.LightPink;
            this.menuManager.Add(sceneID, clone);

            //add volume down button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, verticalBtnSeparation);
            clone.ID = "volumeDownbtn";
            clone.Text = "Volume Down";
            //change the texture blend color
            clone.Color = Color.LightGreen;
            this.menuManager.Add(sceneID, clone);

            //add volume mute button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 2 * verticalBtnSeparation);
            clone.ID = "volumeMutebtn";
            clone.Text = "Volume Mute";
            //change the texture blend color
            clone.Color = Color.LightBlue;
            this.menuManager.Add(sceneID, clone);

            //add volume mute button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 3 * verticalBtnSeparation);
            clone.ID = "volumeUnMutebtn";
            clone.Text = "Volume Un-mute";
            //change the texture blend color
            clone.Color = Color.LightSalmon;
            this.menuManager.Add(sceneID, clone);

            //add back button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 4 * verticalBtnSeparation);
            clone.ID = "backbtn";
            clone.Text = "Back";
            //change the texture blend color
            clone.Color = Color.LightYellow;
            this.menuManager.Add(sceneID, clone);
            #endregion

            #region Controls Menu
            sceneID = "controls menu";

            //retrieve the controls menu background texture
            texture = this.textureDictionary["controlsmenu"];
            //scale the texture to fit the entire screen
            scale = new Vector2((float)graphics.PreferredBackBufferWidth / texture.Width,
                (float)graphics.PreferredBackBufferHeight / texture.Height);
            transform = new Transform2D(scale);
            this.menuManager.Add(sceneID, new UITextureObject("controlsmenuTexture", ActorType.UIStaticTexture,
                StatusType.Drawn, //notice we dont need to update a static texture
                transform, Color.White, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));

            //add back button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 9 * verticalBtnSeparation);
            clone.ID = "backbtn";
            clone.Text = "Back";
            //change the texture blend color
            clone.Color = Color.LightYellow;
            this.menuManager.Add(sceneID, clone);
            #endregion
        }
        private void AddUIElements()
        {
            InitializeUIMousePointer();
            InitializeUIProgress();
            InitializeUIInventoryMenu();
        }
        private void InitializeUIMousePointer()
        {
            Texture2D texture = this.textureDictionary["reticuleDefault"];
            //show complete texture
            Microsoft.Xna.Framework.Rectangle sourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height);

            //listens for object picking events from the object picking manager
            UIPickingMouseObject myUIMouseObject = new UIPickingMouseObject("picking mouseObject",
                ActorType.UITexture,
                new Transform2D(Vector2.One),
                this.fontDictionary["mouse"],
                "",
                new Vector2(0, 40),
                texture,
                this.mouseManager,
                this.eventDispatcher);
            this.uiManager.Add(myUIMouseObject);
        }
        private void InitializeUIProgress()
        {
            float separation = 20; //spacing between progress bars

            Transform2D transform = null;
            Texture2D texture = null;
            UITextureObject textureObject = null;
            Vector2 position = Vector2.Zero;
            Vector2 scale = Vector2.Zero;
            float verticalOffset = 20;
            int startValue;

            texture = this.textureDictionary["progress_gradient"];
            scale = new Vector2(1, 0.75f);

            #region Player 1 Progress Bar
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f - texture.Width * scale.X - separation, verticalOffset);
            transform = new Transform2D(position, 0, scale, 
                Vector2.Zero, /*new Vector2(texture.Width/2.0f, texture.Height/2.0f),*/
                new Integer2(texture.Width, texture.Height));

            textureObject = new UITextureObject(AppData.PlayerOneProgressID,
                    ActorType.UITexture,
                    StatusType.Drawn | StatusType.Update,
                    transform, Color.Green,
                    SpriteEffects.None,
                    1,
                    texture);

            //add a controller which listens for pickupeventdata send when the player (or red box) collects the box on the left
            startValue = 3; //just a random number between 0 and max to demonstrate we can set initial progress value
            textureObject.AttachController(new UIProgressController(AppData.PlayerOneProgressControllerID, ControllerType.UIProgress, startValue, 10, this.eventDispatcher));
            this.uiManager.Add(textureObject);
            #endregion


            #region Player 2 Progress Bar
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f + separation, verticalOffset);
            transform = new Transform2D(position, 0, scale, Vector2.Zero, new Integer2(texture.Width, texture.Height));

            textureObject = new UITextureObject(AppData.PlayerTwoProgressID,
                    ActorType.UITexture,
                    StatusType.Drawn | StatusType.Update,
                    transform, 
                    Color.Red,
                    SpriteEffects.None,
                    1,
                    texture);

            //add a controller which listens for pickupeventdata send when the player (or red box) collects the box on the left
            startValue = 7; //just a random number between 0 and max to demonstrate we can set initial progress value
            textureObject.AttachController(new UIProgressController(AppData.PlayerTwoProgressControllerID, ControllerType.UIProgress, startValue, 10, this.eventDispatcher));
            this.uiManager.Add(textureObject);
            #endregion
        }
        private void InitializeUIInventoryMenu()
        {
            //throw new NotImplementedException();
        }
        #endregion

        #region Effects
        private void InitializeEffects()
        {
            //create a BasicEffect and set the lighting conditions for all models that use this effect in their EffectParameters field
            BasicEffect litModelBasicEffect = new BasicEffect(graphics.GraphicsDevice);

            litModelBasicEffect.TextureEnabled = true;
            litModelBasicEffect.PreferPerPixelLighting = true;
            litModelBasicEffect.EnableDefaultLighting();
            this.effectDictionary.Add("litModelBasicEffect", new BasicEffectParameters(litModelBasicEffect));

            //used for model objects that dont interact with lighting i.e. sky
            BasicEffect unlitModelBasicEffect = new BasicEffect(graphics.GraphicsDevice);
            unlitModelBasicEffect.TextureEnabled = true;
            unlitModelBasicEffect.LightingEnabled = false;
            this.effectDictionary.Add("unlitModelBasicEffect", new BasicEffectParameters(unlitModelBasicEffect));

            DualTextureEffect dualTextureEffect = new DualTextureEffect(graphics.GraphicsDevice);
            this.effectDictionary.Add("unlitModelDualEffect", new DualTextureEffectParameters(dualTextureEffect));

        }
        #endregion

        #region Content, Update, Draw        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
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
            this.modelDictionary.Dispose();
            this.textureDictionary.Dispose();
            this.fontDictionary.Dispose();
            this.videoDictionary.Dispose();

        }

        protected override void Update(GameTime gameTime)
        {
            //exit using new gamepad manager
            if(this.gamePadManager.IsPlayerConnected(PlayerIndex.One) && this.gamePadManager.IsButtonPressed(PlayerIndex.One, Buttons.Back))
                this.Exit();

#if DEMO
            #region Demo
            demoVideoDisplay();
            demoSoundManager();
            demoCameraChange();
            demoAlphaChange();
            demoUIProgressUpdate();
            demoHideDebugInfo();
            demoGamePadManager();
            #endregion
#endif
            base.Update(gameTime);
        }

        #region DEMO
        private void demoVideoDisplay()
        {
            if (this.keyboardManager.IsFirstKeyPress(Keys.F3))
            {
                //pass the target ID for the controller to play the right video
                object[] additonalParameters = { AppData.VideoIDMainHall + " video " + AppData.ControllerIDSuffix };
                EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Video, additonalParameters));
            }
            else if (this.keyboardManager.IsFirstKeyPress(Keys.F4))
            {
                //pass the target ID for the controller to play the right video
                object[] additonalParameters = { AppData.VideoIDMainHall + " video " + AppData.ControllerIDSuffix };
                EventDispatcher.Publish(new EventData(EventActionType.OnPause, EventCategoryType.Video, additonalParameters));
            }

            if (this.keyboardManager.IsFirstKeyPress(Keys.Up))
            {
                //pass the target ID for the controller to play the right video
                object[] additonalParameters = { AppData.VideoIDMainHall + " video " + AppData.ControllerIDSuffix, 0.05f};
                EventDispatcher.Publish(new EventData(EventActionType.OnVolumeUp, EventCategoryType.Video, additonalParameters));
            }
            else if (this.keyboardManager.IsFirstKeyPress(Keys.Down))
            {
                //pass the target ID for the controller to play the right video
                object[] additonalParameters = { AppData.VideoIDMainHall + " video " + AppData.ControllerIDSuffix, 0.05f};
                EventDispatcher.Publish(new EventData(EventActionType.OnMute, EventCategoryType.Video, additonalParameters));
            }


        }
        private void demoGamePadManager()
        {
            if (this.gamePadManager.IsButtonPressed(PlayerIndex.One, Buttons.RightTrigger))
            {
                //do something....
            }
        }
        private void demoSoundManager()
        {
            if (this.keyboardManager.IsFirstKeyPress(Keys.F2))
            {
                object[] additionalParameters = {"boing"};
                EventDispatcher.Publish(new EventData(EventActionType.OnPlay, EventCategoryType.Sound2D, additionalParameters));

                //stopping a cue
                //object[] additionalParameters = { "boing", new Integer(0) };
                //EventDispatcher.Publish(new EventData(EventActionType.OnStop, EventCategoryType.Sound2D, additionalParameters));

            }
        }
        private void demoCameraChange()
        {
            //only single in single screen layout since cycling in multi-screen is meaningless
            if (this.screenManager.ScreenType == ScreenUtility.ScreenType.SingleScreen && this.keyboardManager.IsFirstKeyPress(Keys.F1))
            {
                EventDispatcher.Publish(new EventData(EventActionType.OnCameraCycle, EventCategoryType.Camera));
            }
        }
        private void demoHideDebugInfo()
        {
            //show/hide debug info
            if (this.keyboardManager.IsFirstKeyPress(Keys.F7))
            {
                EventDispatcher.Publish(new EventData(EventActionType.OnToggleDebug, EventCategoryType.Debug));
            }
        }
        private void demoUIProgressUpdate()
        {
            //testing event generation for UIProgressController
            if (this.keyboardManager.IsFirstKeyPress(Keys.F9))
            {
                //increase the left progress controller by 2
                object[] additionalEventParams = { AppData.PlayerOneProgressControllerID, -1};
                EventDispatcher.Publish(new EventData(EventActionType.OnHealthDelta, EventCategoryType.Player, additionalEventParams));
            }
            else if (this.keyboardManager.IsFirstKeyPress(Keys.F10))
            {
                //increase the left progress controller by 2
                object[] additionalEventParams = { AppData.PlayerOneProgressControllerID, 1 };
                EventDispatcher.Publish(new EventData(EventActionType.OnHealthDelta, EventCategoryType.Player, additionalEventParams));
            }

            if (this.keyboardManager.IsFirstKeyPress(Keys.F11))
            {
                //increase the left progress controller by 2
                object[] additionalEventParams = { AppData.PlayerTwoProgressControllerID, -1};
                EventDispatcher.Publish(new EventData(EventActionType.OnHealthDelta, EventCategoryType.Player, additionalEventParams));
            }
            else if (this.keyboardManager.IsFirstKeyPress(Keys.F12))
            {
                //increase the left progress controller by 2
                object[] additionalEventParams = { AppData.PlayerTwoProgressControllerID, 3 };
                EventDispatcher.Publish(new EventData(EventActionType.OnHealthDelta, EventCategoryType.Player, additionalEventParams));
            }
        }
        private void demoAlphaChange()
        {
            //testing event generation on opacity change - see DrawnActor3D::Alpha setter
            if (this.keyboardManager.IsFirstKeyPress(Keys.F5))
            {
                this.drivableBoxObject.Alpha -= 0.05f;
            }
            else if (this.keyboardManager.IsFirstKeyPress(Keys.F6))
            {
                this.drivableBoxObject.Alpha += 0.05f;
            }
        }
        #endregion

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
        #endregion
    }
}
