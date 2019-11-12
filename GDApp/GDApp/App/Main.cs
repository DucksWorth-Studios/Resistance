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
        public TimerManager timerManager { get; private set; }
        public LogicManager logicPuzzle;
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
#if DEBUG
            InitializeDebugTextInfo();
#endif

            //load game happens before cameras are loaded because we may add a third person camera that needs a reference to a loaded Actor
            LoadGame(gameLevel);

            InitializeCollidableFirstPersonDemo(screenResolution);

            //Publish Start Event(s)
            StartGame();

#if DEBUG
            InitializeDebugCollisionSkinInfo();
#endif

            InitializeEvents();
            initialiseTestObject();
            InitializeSwitches();
            InitialisePuzzleLights();
            InitialisePopUP();
            
            base.Initialize();
        }

        #region Events
        /*
         * Any Events That are to be initialised in main will happen in here
         */
         /*This method is used to initialse all events related to the main.cs
          */
        private void InitializeEvents()
        {
            this.eventDispatcher.InteractChanged += Interactive;
            this.eventDispatcher.PuzzleChanged += ChangeLights;
            this.eventDispatcher.RiddleChanged += ChangePopUPState;
            this.eventDispatcher.PlayerChanged += LoseTriggered;
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
            Console.WriteLine(actor.ID);
            this.logicPuzzle.changeState(actor.ID);
            //actor.AddPrimitive(new Box(actor.Transform.Translation, Matrix.Identity, /*important do not change - cm to inch*/2.54f * actor.Transform.Scale), new MaterialProperties(0.2f, 0.8f, 0.7f));
            //actor.Enable(true, 1);
            //this.objectManager.Remove(eventData.Sender as CollidableObject);
            //this.objectManager.Add(actor);
        }

        private void ChangeLights(EventData eventData)
        {
            string id = (string)eventData.AdditionalParameters[0];
            Predicate<Actor3D> predicate = s => s.GetID() == id;
            CollidableObject gate =(CollidableObject) this.objectManager.Find(predicate);
            if(gate.EffectParameters.Texture == this.textureDictionary["gray"])
            {
                gate.EffectParameters.Texture = this.textureDictionary["green"];
            }
            else
            {
                gate.EffectParameters.Texture = this.textureDictionary["gray"];
            }
            
        }
        private void ChangePopUPState(EventData eventData)
        {
            Predicate<Actor2D> pred = s => s.ActorType == ActorType.PopUP;
            UITextureObject item = this.uiManager.Find(pred) as UITextureObject;
            if(item.StatusType == StatusType.Off)
            {
                item.StatusType = StatusType.Drawn;
            }
            else
            {
                item.StatusType = StatusType.Off;
            }     

        }
        /*
         * Author: Cameron
         * This will be used to trigger different UI effects when the timer runs out
         */
        private void LoseTriggered(EventData eventData)
        {
            System.Diagnostics.Debug.WriteLine("Lose event triggered");
        }

        #endregion
                
        #region TestObjects

        private void InitialisePopUP()
        {
            int w,x,y,z;
            int temp = graphics.PreferredBackBufferWidth / 4;
            x = graphics.PreferredBackBufferWidth / 6;
            y = graphics.PreferredBackBufferHeight / 6;

            w = graphics.PreferredBackBufferWidth - (x*2);
            z = graphics.PreferredBackBufferHeight - (y * 2);
            Transform2D transform = new Transform2D(new Vector2(x,y), 0, Vector2.One,Vector2.One,new Integer2(1,1));
            Microsoft.Xna.Framework.Rectangle rect = new Microsoft.Xna.Framework.Rectangle(x,y,w,z);
            Texture2D texture = this.textureDictionary["green"];
            UITextureObject picture = new UITextureObject("PopUp",ActorType.PopUP,StatusType.Off,transform,Color.White,
                SpriteEffects.None,1,texture,rect, new Vector2(1,2));

            this.uiManager.Add(picture);
        }

        private void initialiseTestObject()
        {
            Model model = this.modelDictionary["box2"];
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["gray"];
            Transform3D transform = new Transform3D(new Vector3(-20, 10, -25), new Vector3(0, 0, 0), new Vector3(2, 4, 1), Vector3.UnitX, Vector3.UnitY);
            CollidableObject collidableObject = new CollidableObject("HEY",ActorType.PopUP,transform,effectParameters,model);
            collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity, 2.54f * collidableObject.Transform.Scale), new MaterialProperties(0.2f, 0.8f, 0.7f));

            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        private void InitializeSwitches()
        {
            CollidableObject collidableObject, archetypeCollidableObject = null;
            
            Model model = this.modelDictionary["box2"];
            BasicEffectParameters effectParameters = (this.effectDictionary[AppData.LitModelsEffectID] as BasicEffectParameters).Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["gray"];
            
            archetypeCollidableObject = new CollidableObject("switch-", ActorType.Interactable, Transform3D.Zero, effectParameters, model);

            int count = 0;
            for (int i = 0; i < 4; ++i)
            {
                ++count;
                collidableObject = (CollidableObject)archetypeCollidableObject.Clone();
                collidableObject.ID = "switch-" +count;
                

                collidableObject.Transform = new Transform3D(new Vector3(10 * i, 10, -25), new Vector3(0, 0, 0), new Vector3(2, 4, 1), Vector3.UnitX, Vector3.UnitY);
                collidableObject.AddPrimitive(new Box(collidableObject.Transform.Translation, Matrix.Identity,2.54f * collidableObject.Transform.Scale), new MaterialProperties(0.2f, 0.8f, 0.7f));

                //increase the mass of the boxes in the demo to see how collidable first person camera interacts vs. spheres (at mass = 1)
                collidableObject.Enable(true, 1);
                this.objectManager.Add(collidableObject);
                
            }
        }

        private void InitialisePuzzleLights()
        {
            CollidableObject collidableObject, archetypeCollidableObject = null;
            Model model = this.modelDictionary["sphere"];

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["gray"];

            //make once then clone
            archetypeCollidableObject = new CollidableObject("sphere", ActorType.Light, Transform3D.Zero, effectParameters, model);
            int count = 0;
            for (int i = 0; i < 4; i++)
            {
                ++count;
                collidableObject = (CollidableObject)archetypeCollidableObject.Clone();

                collidableObject.ID = "gate-" + count;
                collidableObject.Transform = new Transform3D(new Vector3(10 * i, 30, -25), new Vector3(0, 0, 0),
                    0.082f * Vector3.One, //notice theres a certain amount of tweaking the radii with reference to the collision sphere radius of 2.54f below
                    Vector3.UnitX, Vector3.UnitY);

                collidableObject.AddPrimitive(new Sphere(collidableObject.Transform.Translation, 2.54f), new MaterialProperties(0.2f, 0.8f, 0.7f));
                collidableObject.Enable(true, 1);
                this.objectManager.Add(collidableObject);
            }

        }
        #endregion


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

            this.logicPuzzle = new LogicManager(this);
            Components.Add(logicPuzzle);

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

            this.timerManager = new TimerManager("Lose Timer", AppData.LoseTimerHours, AppData.LoseTimerMinutes, AppData.LoseTimerSeconds, this, eventDispatcher, StatusType.Off);
            Components.Add(timerManager);
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
            this.modelDictionary.Load("Assets/Models/Architecture/Buildings/house");
            this.modelDictionary.Load("Assets/Models/Architecture/Doors/Barrier_Mapped_00", "barrier");
            this.modelDictionary.Load("Assets/Models/Architecture/Doors/BunkerDoor_Mapped_00", "bunker_door");

            //props
            this.modelDictionary.Load("Assets/Models/Props/War_map_table", "table");
            this.modelDictionary.Load("Assets/Models/Props/Ceiling_Lamp", "Ceiling_lights");
            this.modelDictionary.Load("Assets/Models/Props/AmmoBox");
            this.modelDictionary.Load("Assets/Models/Props/FieldCot");
            this.modelDictionary.Load("Assets/Models/Props/Field_Desk");
            #endregion

            #region Textures
            //environment
            this.textureDictionary.Load("Assets/GDDebug/Textures/checkerboard");
            this.textureDictionary.Load("Assets/Textures/Foliage/Ground/grass1");
            this.textureDictionary.Load("Assets/Textures/Architecture/Walls/wall-texture", "wall");

            //menu - buttons
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/genericbtn");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/quit");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Buttons/start");

            //menu - backgrounds
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/Title-screen");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/mainmenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/audiomenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/controlsmenu");
            this.textureDictionary.Load("Assets/Textures/UI/Menu/Backgrounds/exitmenuwithtrans");

            //ui (or hud) elements
            this.textureDictionary.Load("Assets/Textures/UI/HUD/reticuleDefault");
            this.textureDictionary.Load("Assets/Textures/UI/HUD/progress_gradient");

            //architecture
            this.textureDictionary.Load("Assets/Textures/Architecture/Buildings/house-low-texture");
            this.textureDictionary.Load("Assets/Textures/Architecture/Doors/Concrete", "concrete");
            this.textureDictionary.Load("Assets/Textures/Architecture/Doors/BrushedAluminum", "aluminum");
            //this.textureDictionary.Load("Assets/Textures/Architecture/Walls/wall");
            this.textureDictionary.Load("Assets/Textures/Props/Crates/crate1");
            //dual texture demo - see Main::InitializeCollidableGround()
            this.textureDictionary.Load("Assets/GDDebug/Textures/checkerboard_greywhite");

            //Load Colors
            this.textureDictionary.Load("Assets/Colours/gray");
            this.textureDictionary.Load("Assets/Colours/green");


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
            //collidable
            InitializeCollidableWalls(worldScale);
          
            //Collidable
            InitializeCollidableGround(worldScale);

            ////add level elements
            //InitializeBuildings();
            InitializeExitDoor();
            InitializeDoorBarriers();

            //init props
            InitializeWarTable();
            InitializeCeilingLights();
            InitializeAmmoBoxes();
            InitializeFieldCot();
            InitializeFieldDesk();

            ////add primitive objects - where developer defines the vertices manually
            //InitializePrimitives();

        }

        //private void InitializePrimitives()
        //{
        //    //get a copy of the effect parameters
        //    BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnLitPrimitivesEffectID].Clone() as BasicEffectParameters;
        //    effectParameters.Texture = this.textureDictionary["checkerboard"];
        //    effectParameters.DiffuseColor = Color.Yellow;
        //    effectParameters.Alpha = 0.4f;

        //    //define location
        //    Transform3D transform = new Transform3D(new Vector3(0, 40, 0), new Vector3(40, 4, 1));

        //    //create primitive
        //    PrimitiveObject primitiveObject = new PrimitiveObject("simple primitive", ActorType.Primitive,
        //        transform, effectParameters, StatusType.Drawn | StatusType.Update, this.vertexDataDictionary[AppData.TexturedQuadID]);

        //    PrimitiveObject clonedPrimitiveObject = null;

        //    for (int i = 1; i <= 4; i++)
        //    {
        //        clonedPrimitiveObject = primitiveObject.Clone() as PrimitiveObject;
        //        clonedPrimitiveObject.Transform.Translation += new Vector3(0, 5 * i, 0);

        //        //we could also attach controllers here instead to give each a different rotation
        //        clonedPrimitiveObject.AttachController(new RotationController("rot controller", ControllerType.Rotation, new Vector3(0.1f * i, 0, 0)));

        //        //add to manager
        //        this.objectManager.Add(clonedPrimitiveObject);
        //    }

        //}

        //skybox is a non-collidable series of ModelObjects with no lighting
        private void InitializeCollidableWalls(int worldScale)
        {
            //first we will create a prototype plane and then simply clone it for each of the skybox decorator elements (e.g. ground, front, top etc). 
            Transform3D transform = new Transform3D(new Vector3(0, 0, 0), new Vector3(worldScale, 1, worldScale / 4));

            //clone the dictionary effect and set unique properties for the hero player object
            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["checkerboard"];

            //create a archetype to use for cloning
            ModelObject planePrototypeModelObject = new ModelObject("plane1", ActorType.Decorator, transform, effectParameters, this.modelDictionary["box2"]);

            //will be re-used for all planes
            ModelObject clonePlane = null;

            #region Walls
            #region Back wall
            //add the back skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            //scale the length of the plane to be half of the worldscale
            clonePlane.Transform.Scale = new Vector3(worldScale / 2.0f, 1, worldScale / 4);
            //rotate the default plane 90 degrees around the X-axis (use the thumb and curled fingers of your right hand to determine +ve or -ve rotation value)
            clonePlane.Transform.Rotation = new Vector3(90, 0, 0);
            
            /*
             * Move the plane back to meet with the back edge of the grass (by based on the original 3DS Max model scale)
             */

            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 4f, 0, (-2.54f * worldScale) / 2f);
            this.objectManager.Add(clonePlane);
            #endregion

            #region Left wall
            //longest wall of the L shape level
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Rotation = new Vector3(90, 90, 0);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 2.0f, 0, 0);
            this.objectManager.Add(clonePlane);
            #endregion

            #region short right wall
            //add the right skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(worldScale/ 4, 1, worldScale / 4);
            clonePlane.Transform.Rotation = new Vector3(90, -90, 0);
            clonePlane.Transform.Translation = new Vector3(worldScale / 128.0f, 0, (-2.54f * worldScale) / 2.67f);
            this.objectManager.Add(clonePlane);
            #endregion

            #region long right wall
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(3 * worldScale / 4, 1, worldScale / 4);
            clonePlane.Transform.Rotation = new Vector3(90, -90, 0);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 4.0f, 0, (2.54f * worldScale) / 8.0f);
            this.objectManager.Add(clonePlane);
            #endregion

            #region 2nd room short wall
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(worldScale / 4, 1, worldScale / 4);
            clonePlane.Transform.Rotation = new Vector3(-90, 0, 180);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 8.0f, 0, (-2.54f * worldScale) / 4.0f);
            this.objectManager.Add(clonePlane);
            #endregion


            ////add the top skybox plane
            //clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            //clonePlane.EffectParameters.Texture = this.textureDictionary["sky"];
            ////notice the combination of rotations to correctly align the sky texture with the sides
            //clonePlane.Transform.Rotation = new Vector3(180, -90, 0);
            //clonePlane.Transform.Translation = new Vector3(0, ((2.54f * worldScale) / 2.0f), 0);
            //this.objectManager.Add(clonePlane);

            #region front wall
            //add the front skybox plane
            // this side will be done in 3 blocks two on each side with a space for a door and then a block on top of it
            //left side of door
            float xScale = 0.833f * worldScale / 10.0f;
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(xScale, 1, worldScale / 4);
            clonePlane.Transform.Rotation = new Vector3(-90, 0, 180);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 2.2f, 0, (2.54f * worldScale) / 2f);
            this.objectManager.Add(clonePlane);

            //right side of door
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(xScale, 1, worldScale / 4);
            clonePlane.Transform.Rotation = new Vector3(-90, 0, 180);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 3.5f, 0, (2.54f * worldScale) / 2f);
            this.objectManager.Add(clonePlane);

            //top of door way
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.EffectParameters.Texture = this.textureDictionary["wall"];
            clonePlane.Transform.Scale = new Vector3(worldScale / 11.7f, 1, worldScale / 33);
            clonePlane.Transform.Rotation = new Vector3(-90, 0, 180);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 2.7f, worldScale / 3.55f, (2.54f * worldScale) / 2f);
            this.objectManager.Add(clonePlane);
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
             * See https://www.youtube.com/watch?v=AqiNpRmENIQ&t=1892s
             * 
             */
            Model model = this.modelDictionary["box2"];

            //a simple dual texture demo - dual textures can be used with a lightMap from 3DS Max using the Render to Texture setting
            //DualTextureEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelDualEffectID].Clone() as DualTextureEffectParameters;
            //effectParameters.Texture = this.textureDictionary["grass1"];
            //effectParameters.Texture2 = this.textureDictionary["checkerboard_greywhite"];

            BasicEffectParameters effectParameters = this.effectDictionary[AppData.UnlitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["grass1"];

            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero, new Vector3(worldScale, 0.001f, worldScale), Vector3.UnitX, Vector3.UnitY);
            collidableObject = new CollidableObject("ground", ActorType.CollidableGround, transform3D, effectParameters, model);
            collidableObject.AddPrimitive(new JigLibX.Geometry.Plane(transform3D.Up, transform3D.Translation), new MaterialProperties(0.8f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1); //change to false, see what happens.
            this.objectManager.Add(collidableObject);
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
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-94, 12, 127), new Vector3(-90, 0, 0), new Vector3(0.11f, 0.01f, 0.07f), Vector3.UnitX, Vector3.UnitY);

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;
            effectParameters.Texture = this.textureDictionary["aluminum"];

            collidableObject = new TriangleMeshObject("exitDoor", ActorType.CollidableDoor, transform3D, effectParameters,
                this.modelDictionary["bunker_door"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
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
                effectParameters, this.modelDictionary["barrier"]);

            #region first barrier
            cloneCollider = (CollidableObject)collidableObject.Clone();
            cloneCollider.ID += 1;

            cloneCollider.Transform = new Transform3D(new Vector3(-100, 6, 124), new Vector3(-90, 0, 180), new Vector3(0.1f, 0.05f, 0.1f), Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(new Box(cloneCollider.Transform.Translation, Matrix.Identity, 2.54f * cloneCollider.Transform.Scale), 
                new MaterialProperties(0.2f, 0.8f, 0.7f));

            cloneCollider.Enable(true, 1);
            this.objectManager.Add(cloneCollider);
            #endregion

            #region second region
            //second barrier
            cloneCollider = (CollidableObject)collidableObject.Clone();
            cloneCollider.ID += 2;

            cloneCollider.Transform = new Transform3D(new Vector3(-88, 20, 124), new Vector3(-90, 0, 0), new Vector3(0.1f, 0.05f, 0.1f), Vector3.UnitX, Vector3.UnitY);
            cloneCollider.AddPrimitive(new Box(cloneCollider.Transform.Translation, Matrix.Identity, 2.54f * cloneCollider.Transform.Scale),
                new MaterialProperties(0.2f, 0.8f, 0.7f));

            cloneCollider.Enable(true, 1);
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

            collidableObject = new TriangleMeshObject("warTable", ActorType.CollidableDecorator, transform3D, effectParameters, this.modelDictionary["table"],
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        private void InitializeCeilingLights()
        {
            BasicEffectParameters effectParameters;
            ModelObject modelObject = null, cloneModel = null;

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;

            modelObject = new ModelObject("Ceiling light - ", ActorType.Decorator, Transform3D.Zero, effectParameters,
                this.modelDictionary["Ceiling_lights"]);

            #region first ceiling light
            cloneModel = (ModelObject)modelObject.Clone();
            cloneModel.ID += 1;

            cloneModel.Transform = new Transform3D(new Vector3(0, 20, 0), new Vector3(0, 0, 0), new Vector3(2.0f, 2.0f, 2.0f), 
                Vector3.UnitX, Vector3.UnitY);

            this.objectManager.Add(cloneModel);
            #endregion
        }

        private void InitializeAmmoBoxes()
        {
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject = null, cloneCollidable;

            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;

            collidableObject = new CollidableObject("Ammo box - ", ActorType.CollidableDecorator, Transform3D.Zero, effectParameters,
                this.modelDictionary["AmmoBox"]);


            for (int i = 0; i < 3; i++)
            {
                cloneCollidable = (CollidableObject)collidableObject.Clone();

                cloneCollidable.ID += i;
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

            transform3D = new Transform3D(new Vector3(-120.0f, 3.5f, -90.0f), new Vector3(0, 0, 0), new Vector3(0.03f, 0.03f, 0.05f),
                Vector3.UnitX, Vector3.UnitY);
            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;

            collidableObject = new TriangleMeshObject("field cot", ActorType.CollidableDecorator, transform3D, effectParameters,
                this.modelDictionary["FieldCot"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        private void InitializeFieldDesk()
        {
            Transform3D transform3D;
            BasicEffectParameters effectParameters;
            CollidableObject collidableObject;

            transform3D = new Transform3D(new Vector3(-100.0f, -2.0f, -131.0f), new Vector3(0, 90, 0), new Vector3(3.0f, 2.5f, 3.5f),
               Vector3.UnitX, Vector3.UnitY);
            effectParameters = this.effectDictionary[AppData.LitModelsEffectID].Clone() as BasicEffectParameters;

            collidableObject = new TriangleMeshObject("field desk", ActorType.CollidableDecorator, transform3D, effectParameters,
                this.modelDictionary["Field_Desk"], new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
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
        #endregion
        
        #region Menu & UI
        private void AddMenuElements()
        {
            Transform2D transform = null;
            Texture2D texture = null;
            Vector2 position = Vector2.Zero;
            UIButtonObject uiButtonObject = null, clone = null;
            string sceneID = "", buttonID = "", buttonText = "";
            int verticalBtnSeparation = 75;

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
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 575);
            texture = this.textureDictionary["start"];
            transform = new Transform2D(position,
                0, new Vector2(0.8f, 0.8f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, Color.CornflowerBlue, SpriteEffects.None, 0.1f, texture, buttonText,
                this.fontDictionary["menu"],
                Color.DarkGray, new Vector2(0, 2));

            uiButtonObject.AttachController(new UIScaleSineLerpController("sineScaleLerpController2", ControllerType.SineScaleLerp,
              new TrigonometricParameters(0.1f, 0.2f, 1)));
            this.menuManager.Add(sceneID, uiButtonObject);

            //add exit button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "exitbtn";
            clone.Texture = this.textureDictionary["quit"];
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, verticalBtnSeparation);
            //change the texture blend color
            clone.Color = Color.Red;
            //store the original color since if we modify with a controller and need to reset
            clone.OriginalColor = clone.Color;
            //attach another controller on the exit button just to illustrate multi-controller approach
            clone.AttachController(new UIColorSineLerpController("colorSineLerpController", ControllerType.SineColorLerp,
                    new TrigonometricParameters(1, 0.4f, 0), Color.IndianRed, Color.DarkRed));
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
            InitializeTimerUI();
        }
        private void InitializeUIMousePointer()
        {
            Texture2D texture = this.textureDictionary["reticuleDefault"];
            //show complete texture
            Microsoft.Xna.Framework.Rectangle sourceRectangle = new Microsoft.Xna.Framework.Rectangle(0, 0, texture.Width, texture.Height);

            Transform2D transform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth / 2 - texture.Width/2, graphics.PreferredBackBufferHeight / 2 - texture.Height/2), 0, Vector2.One, Vector2.Zero, new Integer2(texture.Width, texture.Height));
            UITextureObject crosshair = new UITextureObject("crosshair", ActorType.UIStaticTexture, StatusType.Drawn, transform, Color.White, SpriteEffects.None, 0, texture);

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
            int count = 1;

            foreach (TimerUtility timer in timerManager.TimerList)
            {
                Transform2D timerTransform = new Transform2D(new Vector2(graphics.PreferredBackBufferWidth-100, 25 * count),
                    0, Vector2.One, Vector2.Zero, Integer2.Zero);

                UITimer uiTimer = new UITimer(timerTransform, 0.1f, fontDictionary["timerFont"], timer);
                this.uiManager.Add(uiTimer);
                count++;
            }
        }

        #endregion

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
            basicEffect.PreferPerPixelLighting = true;
            basicEffect.EnableDefaultLighting();
            this.effectDictionary.Add(AppData.LitModelsEffectID, new BasicEffectParameters(basicEffect));
            #endregion

            #region For Unlit objects
            //used for model objects that dont interact with lighting i.e. sky
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.TextureEnabled = true;
            basicEffect.LightingEnabled = false;
            this.effectDictionary.Add(AppData.UnlitModelsEffectID, new BasicEffectParameters(basicEffect));
            #endregion

            #region For dual texture objects
            dualTextureEffect = new DualTextureEffect(graphics.GraphicsDevice);
            this.effectDictionary.Add(AppData.UnlitModelDualEffectID, new DualTextureEffectParameters(dualTextureEffect));
            #endregion

            #region For unlit billboard objects
            billboardEffect = Content.Load<Effect>("Assets/Effects/billboard");
            this.effectDictionary.Add(AppData.UnlitBillboardsEffectID, new BillboardEffectParameters(billboardEffect));
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
            if (this.gamePadManager.IsPlayerConnected(PlayerIndex.One) && this.gamePadManager.IsButtonPressed(PlayerIndex.One, Buttons.Back))
                this.Exit();      
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
        #endregion
    }
}
