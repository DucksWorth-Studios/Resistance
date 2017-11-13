using System;
using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using JigLibX.Geometry;
using JigLibX.Collision;
using System.Collections.Generic;
/*
Issue with one side of SkyBox frustum culling.
Physics is not updating when boxes are removed.
ScreenManager - enum - this.ScreenType = (ScreenUtilityScreenType)eventData.AdditionalEventParameters[0];
check clone on new eventdata
add a collidable player object
add a collidable pickup object and remove on collision with player
PiP
scripting camera changes using event data read from XML?
menu - click sound
menu transparency
*/

namespace GDApp
{
    public class Main : Microsoft.Xna.Framework.Game
    {

        #region Fields
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

        //receives, handles and routes events
        public EventDispatcher eventDispatcher { get; private set; }

        //default shader for rendering 3D models
        private BasicEffect modelEffect;
        //user for the skybox since the lighting is baked into the textures used in the skybox
        private BasicEffect unlitModelEffect;

        //stores loaded game resources
        private ContentDictionary<Model> modelDictionary;
        private ContentDictionary<Texture2D> textureDictionary;
        private ContentDictionary<SpriteFont> fontDictionary;

        //stores curves and rails used by cameras
        private Dictionary<string, Transform3DCurve> curveDictionary;
        private Dictionary<string, RailParameters> railDictionary;
        //stores viewport layouts for multi-screen layout
        private Dictionary<string, Viewport> viewPortDictionary;

        //demo remove later
        private ModelObject drivableBoxObject;

#if DEBUG
        //used to visualize debug info (e.g. FPS) and also to draw collision skins
        private DebugDrawer debugDrawer;
        private PhysicsDebugDrawer physicsDebugDrawer;
#endif
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
            int gameLevel = 1;
            bool isMouseVisible = true;
            Integer2 screenResolution = ScreenUtility.HD720;
            ScreenUtility.ScreenType screenType = ScreenUtility.ScreenType.SingleScreen;

            //set the title
            Window.Title = "3DGD - My Amazing Game 1.0";

            //Initialize Effects
            InitializeEffects();

            //Initialize EventDispatcher
            InitializeEventDispatcher();

            //Initialize the Managers
            InitializeManagers(screenResolution, screenType, isMouseVisible);

            //Load Dictionaries, Media Assets and Non-media Assets
            LoadDictionaries();
            LoadAssets();
            LoadCurvesAndRails();
            LoadViewports(screenResolution);

            //load game happens before cameras are loaded because we may add a third person camera that needs a reference to a loaded Actor
            LoadGame(gameLevel);

            //Initialize cameras based on the desired screen layout
            if (screenType == ScreenUtility.ScreenType.SingleScreen)
            {
                // InitializeSingleScreenCameraDemo(screenResolution);
                //or
                InitializeSingleScreenCycleableCameraDemo(screenResolution);
            }
            else //multi-screen or picture-in-picture
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

        private void InitializeManagers(Integer2 screenResolution, ScreenUtility.ScreenType screenType, bool isMouseVisible)
        {
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
            this.physicsManager = new PhysicsManager(this, this.eventDispatcher, StatusType.Off);
            Components.Add(this.physicsManager);

            //add mouse manager
            this.mouseManager = new MouseManager(this, isMouseVisible, this.physicsManager);
            Components.Add(this.mouseManager);
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
        }

        private void LoadAssets()
        {
            #region Models
            this.modelDictionary.Load("Assets/Models/plane1", "plane1");
            this.modelDictionary.Load("Assets/Models/box2", "box2");
            this.modelDictionary.Load("Assets/Models/torus");
            this.modelDictionary.Load("Assets/Models/sphere");

            //triangle mesh high/low poly demo
            this.modelDictionary.Load("Assets/Models/teapot");
            this.modelDictionary.Load("Assets/Models/teapot_mediumpoly");
            this.modelDictionary.Load("Assets/Models/teapot_lowpoly");
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
        }

#if DEBUG
        private void InitializeDebugTextInfo()
        {
            //add debug info in top left hand corner of the screen
            this.debugDrawer = new DebugDrawer(this, this.screenManager, this.cameraManager, this.objectManager, spriteBatch,
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

            //Non-collidable
            InitializeNonCollidableSkyBox(worldScale);
            InitializeNonCollidableFoliage(worldScale);
            InitializeNonCollidableDriveableObject();
            InitializeNonCollidableDecoratorObjects();

            //Collidable
            InitializeCollidableGround(worldScale);
            //demo high vertex count trianglemesh
            InitializeStaticCollidableTriangleMeshObjects();
            //demo medium and low vertex count trianglemesh
            InitializeStaticCollidableMediumPolyTriangleMeshObjects();
            InitializeStaticCollidableLowPolyTriangleMeshObjects();
            //demo dynamic collidable objects with user-defined collision primitives
            InitializeDynamicCollidableObjects();
        }

        //skybox is a non-collidable series of ModelObjects with no lighting
        private void InitializeNonCollidableSkyBox(int worldScale)
        {
            //first we will create a prototype plane and then simply clone it for each of the skybox decorator elements (e.g. ground, front, top etc). 
            Transform3D transform = new Transform3D(new Vector3(0, 0, 0), new Vector3(worldScale, 1, worldScale));

            ModelObject planePrototypeModelObject = new ModelObject("plane1", ActorType.Decorator, transform, this.unlitModelEffect, ColorParameters.WhiteOpaque,
                this.textureDictionary["grass1"],
                this.modelDictionary["plane1"]);

            //will be re-used for all planes
            ModelObject clonePlane = null;

            #region Skybox
            //we no longer add a simple grass plane - see InitializeCollidableGround()
            //clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            //clonePlane.Texture = this.textureDictionary["grass1"];
            //this.objectManager.Add(clonePlane);

            //add the back skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.Texture = this.textureDictionary["back"];
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
            clonePlane.Texture = this.textureDictionary["left"];
            clonePlane.Transform.Rotation = new Vector3(90, 90, 0);
            clonePlane.Transform.Translation = new Vector3((-2.54f * worldScale) / 2.0f, 0, 0);
            this.objectManager.Add(clonePlane);

            //add the right skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.Texture = this.textureDictionary["right"];
            clonePlane.Transform.Rotation = new Vector3(90, -90, 0);
            clonePlane.Transform.Translation = new Vector3((2.54f * worldScale) / 2.0f, 0, 0);
            this.objectManager.Add(clonePlane);

            //add the top skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.Texture = this.textureDictionary["sky"];
            //notice the combination of rotations to correctly align the sky texture with the sides
            clonePlane.Transform.Rotation = new Vector3(180, -90, 0);
            clonePlane.Transform.Translation = new Vector3(0, ((2.54f * worldScale) / 2.0f), 0);
            this.objectManager.Add(clonePlane);

            //add the front skybox plane
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.Texture = this.textureDictionary["front"];
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

            ModelObject planePrototypeModelObject = new ModelObject("plane1", ActorType.Decorator, transform, this.unlitModelEffect,
                ColorParameters.WhiteAlmostOpaque,
                this.textureDictionary["checkerboard"],
                this.modelDictionary["plane1"]);

            //will be re-used for all planes
            ModelObject clonePlane = null;

            //tree
            clonePlane = (ModelObject)planePrototypeModelObject.Clone();
            clonePlane.Texture = this.textureDictionary["tree2"];
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
            Texture2D texture = null;

            Model model = this.modelDictionary["box2"];
            texture = this.textureDictionary["grass1"];
            transform3D = new Transform3D(Vector3.Zero, Vector3.Zero,
                new Vector3(worldScale, 0.001f, worldScale), Vector3.UnitX, Vector3.UnitY);

            collidableObject = new CollidableObject("ground", ActorType.CollidableGround, transform3D, this.modelEffect, ColorParameters.WhiteOpaque, texture, model);
            collidableObject.AddPrimitive(new JigLibX.Geometry.Plane(transform3D.Up, transform3D.Translation), new MaterialProperties(0.8f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1); //change to false, see what happens.
            this.objectManager.Add(collidableObject);
        }

        //Triangle mesh objects wrap a tight collision surface around complex shapes - the downside is that TriangleMeshObjects CANNOT be moved
        private void InitializeStaticCollidableTriangleMeshObjects()
        {
            CollidableObject collidableObject = null;
            Transform3D transform3D = null;

            transform3D = new Transform3D(new Vector3(-50, 10, 0),
                new Vector3(45, 45, 0), 0.1f * Vector3.One, Vector3.UnitX, Vector3.UnitY);

            collidableObject = new TriangleMeshObject("torus", ActorType.CollidableProp,
            transform3D, this.modelEffect,
            ColorParameters.WhiteOpaque,
            this.textureDictionary["ml"], this.modelDictionary["torus"],
                  new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        //Demos use of a low-polygon model to generate the triangle mesh collision skin - saving CPU cycles on CDCR checking
        private void InitializeStaticCollidableMediumPolyTriangleMeshObjects()
        {

            CollidableObject collidableObject = null;
            Transform3D transform3D = null;

            transform3D = new Transform3D(new Vector3(-30, 3, 0),
                new Vector3(0, 0, 0), 0.08f * Vector3.One, Vector3.UnitX, Vector3.UnitY);

            collidableObject = new TriangleMeshObject("teapot", ActorType.CollidableProp,
            transform3D, this.modelEffect,
            new ColorParameters(Color.Yellow, 1),
            this.textureDictionary["checkerboard"], this.modelDictionary["teapot"],
            this.modelDictionary["teapot_mediumpoly"],
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        //Demos use of a low-polygon model to generate the triangle mesh collision skin - saving CPU cycles on CDCR checking
        private void InitializeStaticCollidableLowPolyTriangleMeshObjects()
        {

            CollidableObject collidableObject = null;
            Transform3D transform3D = null;

            transform3D = new Transform3D(new Vector3(-10, 3, 0),
                new Vector3(0, 0, 0), 0.08f * Vector3.One, Vector3.UnitX, Vector3.UnitY);

            collidableObject = new TriangleMeshObject("teapot", ActorType.CollidableProp,
            transform3D, this.modelEffect,
            new ColorParameters(Color.Blue, 1),
            this.textureDictionary["checkerboard"], this.modelDictionary["teapot"],
            this.modelDictionary["teapot_lowpoly"],
                new MaterialProperties(0.2f, 0.8f, 0.7f));
            collidableObject.Enable(true, 1);
            this.objectManager.Add(collidableObject);
        }

        //if you want objects to be collidable AND moveable then you must attach either a box, sphere, or capsule primitives to the object
        private void InitializeDynamicCollidableObjects()
        {
            CollidableObject collidableObject, sphereArchetype = null;
            Transform3D transform3D = null;
            Texture2D texture = null;
            Model model = null;

            #region Spheres
            //these boxes, spheres and cylinders are all centered around (0,0,0) in 3DS Max
            model = this.modelDictionary["sphere"];
            texture = this.textureDictionary["checkerboard"];

            //make once then clone
            sphereArchetype = new CollidableObject("sphere ",
                 ActorType.CollidablePickup, Transform3D.Zero, this.modelEffect,
                 ColorParameters.WhiteOpaque,
                 texture, model);

            for (int i = 0; i < 10; i++)
            {
                collidableObject = (CollidableObject)sphereArchetype.Clone();

                collidableObject.ID += " - " + i;
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
            texture = this.textureDictionary["crate2"];

            for (int i = 0; i < 5; i++)
            {
                transform3D = new Transform3D(
                        new Vector3(15, 15 + 10 * i, i * 2),
                        new Vector3(0, 0, 0),
                        new Vector3(2, 4, 1),
                        Vector3.UnitX, Vector3.UnitY);

                collidableObject = new CollidableObject("box - " + i,
                    ActorType.CollidablePickup, transform3D, this.modelEffect,
                    ColorParameters.WhiteOpaque,
                    texture, model);

                collidableObject.AddPrimitive(
                    new Box(transform3D.Translation, Matrix.Identity, /*important do not change - cm to inch*/2.54f * transform3D.Scale),
                    new MaterialProperties(0.2f, 0.8f, 0.7f));

                //increase the mass of the boxes in the demo to see how collidable first person camera interacts vs. spheres (at mass = 1)
                collidableObject.Enable(false, 100);
                this.objectManager.Add(collidableObject);
            }

            #endregion
        }

        //demo of a non-collidable ModelObject with attached third person controller
        private void InitializeNonCollidableDriveableObject()
        {
            //place the drivable model to the left of the existing models and specify that forward movement is along the -ve z-axis
            Transform3D transform = new Transform3D(new Vector3(-10, 5, 25), -Vector3.UnitZ, Vector3.UnitY);

            //initialise the drivable model object - we've made this variable a field to allow it to be visible to the rail camera controller - see InitializeCameras()
            this.drivableBoxObject = new ModelObject("drivable box1", ActorType.Player, transform, this.modelEffect, new ColorParameters(Color.Gold, 1),
                this.textureDictionary["crate1"],
                this.modelDictionary["box2"]);

            //attach a DriveController
            drivableBoxObject.AttachController(new DriveController("driveController1", ControllerType.Drive,
                AppData.PlayerMoveKeys, AppData.PlayerMoveSpeed, AppData.PlayerStrafeSpeed, AppData.PlayerRotationSpeed, this.mouseManager, this.keyboardManager));

            //add to the objectManager so that it will be drawn and updated
            this.objectManager.Add(drivableBoxObject);
        }

        //demo of some semi-transparent non-collidable ModelObjects
        private void InitializeNonCollidableDecoratorObjects()
        {
            //position the object
            Transform3D transform = new Transform3D(new Vector3(0, 5, 0), Vector3.Zero, Vector3.One, Vector3.UnitX, Vector3.UnitY);

            //loading model, texture

            //initialise the boxObject
            ModelObject boxObject = new ModelObject("some box 1", ActorType.Decorator, transform, this.modelEffect,
                new ColorParameters(Color.White, 0.5f),
                this.textureDictionary["crate1"], this.modelDictionary["box2"]);
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
            clone.Color = Color.Red;
            this.objectManager.Add(clone);

            //add more clones here...
        }
        #endregion

        #region Initialize Cameras
        private void InitializeCamera(Integer2 screenResolution, string id, Viewport viewPort, Transform3D transform, IController controller)
        {
            Camera3D camera = new Camera3D(id, ActorType.Camera, transform, ProjectionParameters.StandardMediumFiveThree, viewPort, 1, StatusType.Update);

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
                this.mouseManager, this.keyboardManager, this.cameraManager, this.screenManager);
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller);

            //security camera 1
            id = "non-collidable security 1";
            viewportDictionaryKey = "column0 row0";
            transform = new Transform3D(new Vector3(0, cameraHeight, 20), -Vector3.UnitZ, Vector3.UnitY);
            controller = new SecurityCameraController(id + " controller", ControllerType.Security, 60, AppData.SecurityCameraRotationSpeedSlow, AppData.SecurityCameraRotationAxisYaw);
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller);

            //security camera 2
            id = "non-collidable security 2";
            viewportDictionaryKey = "column0 row1";
            transform = new Transform3D(new Vector3(0, cameraHeight, 20), -Vector3.UnitZ, Vector3.UnitY);
            controller = new SecurityCameraController(id + " controller", ControllerType.Security, 45, AppData.SecurityCameraRotationSpeedMedium, new Vector3(1, 1, 0));
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller);

            //security camera 3
            id = "non-collidable security 3";
            viewportDictionaryKey = "column0 row2";
            transform = new Transform3D(new Vector3(0, cameraHeight, 20), -Vector3.UnitZ, Vector3.UnitY);
            controller = new SecurityCameraController(id + " controller", ControllerType.Security, 30, AppData.SecurityCameraRotationSpeedFast, new Vector3(4, 1, 0));
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller);

            //track camera 1
            id = "non-collidable track 1";
            viewportDictionaryKey = "column0 row3";
            transform = new Transform3D(new Vector3(0, cameraHeight, 20), -Vector3.UnitZ, Vector3.UnitY);
            controller = new CurveController(id + " controller", ControllerType.Track, this.curveDictionary["unique curve name 1"], PlayStatusType.Play);
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller);

            //rail camera 1
            id = "non-collidable rail 1";
            viewportDictionaryKey = "column0 row4";
            //since the camera will be set on a rail it doesnt matter what the initial transform is set to
            transform = Transform3D.Zero;
            controller = new RailController(id + " controller", ControllerType.Rail, this.drivableBoxObject, this.railDictionary["rail1 - parallel to x-axis"]);
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller);
        }

        private void InitializeSingleScreenCameraDemo(Integer2 screenResolution)
        {
            Transform3D transform = null;
            string id = "";
            string viewportDictionaryKey = "";

            id = "collidable first person camera";
            viewportDictionaryKey = "full viewport";
            transform = new Transform3D(new Vector3(0, 10, 60), -Vector3.UnitZ, Vector3.UnitY);

            Camera3D camera = new Camera3D(id, ActorType.Camera, transform,
                    ProjectionParameters.StandardDeepSixteenNine, this.viewPortDictionary[viewportDictionaryKey], 1, StatusType.Update);

            //attach a CollidableFirstPersonController
            camera.AttachController(new CollidableFirstPersonCameraController(
                    camera + " controller",
                    ControllerType.CollidableFirstPerson,
                    AppData.CameraMoveKeys,
                    AppData.CollidableCameraMoveSpeed, AppData.CollidableCameraStrafeSpeed, AppData.CameraRotationSpeed,
                    this.mouseManager, this.keyboardManager, this.cameraManager, this.screenManager,
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

            InitializeSingleScreenCameraDemo(screenResolution);

            Transform3D transform = null;
            IController controller = null;
            string id = "";
            string viewportDictionaryKey = "full viewport";

            //track camera 1
            id = "non-collidable track 1";
            transform = new Transform3D(new Vector3(0, 0, 20), -Vector3.UnitZ, Vector3.UnitY);
            controller = new CurveController(id + " controller", ControllerType.Track, this.curveDictionary["unique curve name 1"], PlayStatusType.Play);
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller);

            //rail camera 1
            id = "non-collidable rail 1";
            //since the camera will be set on a rail it doesnt matter what the initial transform is set to
            transform = Transform3D.Zero;
            controller = new RailController(id + " controller", ControllerType.Rail, this.drivableBoxObject, this.railDictionary["rail1 - parallel to x-axis"]);
            InitializeCamera(screenResolution, id, this.viewPortDictionary[viewportDictionaryKey], transform, controller);

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
            EventDispatcher.Publish(new EventData("this doesnt matter", this, EventActionType.OnPause, EventCategoryType.MainMenu));

            //publish an event to set the camera
            object[] additionalEventParamsB = { "collidable first person camera 1"};
            EventDispatcher.Publish(new EventData("set camera bla", this, EventActionType.OnCameraSetActive, EventCategoryType.Camera, additionalEventParamsB));
            //we could also just use the line below, but why not use our event dispatcher?
            //this.cameraManager.SetActiveCamera(x => x.ID.Equals("collidable first person camera 1"));

        }
        #endregion

        #region Menu
        private void InitializeMenu()
        {
            this.menuManager = new MyAppMenuManager(this, this.mouseManager, this.keyboardManager, this.cameraManager,
                spriteBatch, this.eventDispatcher, StatusType.Off);
            //set the main menu to be the active menu scene
            this.menuManager.SetActiveList("mainmenu");
            Components.Add(this.menuManager);
        }

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
                transform, ColorParameters.WhiteOpaque, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));

            //add start button
            buttonID = "startbtn";
            buttonText = "Start";
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f, 200);
            texture = this.textureDictionary["genericbtn"];
            transform = new Transform2D(position,
                0, new Vector2(1.5f, 0.6f),
                new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Integer2(texture.Width, texture.Height));

            uiButtonObject = new UIButtonObject(buttonID, ActorType.UIButton, StatusType.Update | StatusType.Drawn,
                transform, new ColorParameters(Color.LightPink, 1), SpriteEffects.None, 0.1f, texture, buttonText,
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
            clone.ColorParameters.Color = Color.LightGreen;
            this.menuManager.Add(sceneID, clone);

            //add controls button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "controlsbtn";
            clone.Text = "Controls";
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 2 * verticalBtnSeparation);
            //change the texture blend color
            clone.ColorParameters.Color = Color.LightBlue;
            this.menuManager.Add(sceneID, clone);

            //add exit button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "exitbtn";
            clone.Text = "Exit";
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 3 * verticalBtnSeparation);
            //change the texture blend color
            clone.ColorParameters.Color = Color.LightYellow;
            //store the original color since if we modify with a controller and need to reset
            clone.ColorParameters.OriginalColorParameters.Color = clone.ColorParameters.Color;
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
                transform, ColorParameters.WhiteOpaque, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));


            //add volume up button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            clone.ID = "volumeUpbtn";
            clone.Text = "Volume Up";
            //change the texture blend color
            clone.ColorParameters.Color = Color.LightPink;
            this.menuManager.Add(sceneID, clone);

            //add volume down button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, verticalBtnSeparation);
            clone.ID = "volumeDownbtn";
            clone.Text = "Volume Down";
            //change the texture blend color
            clone.ColorParameters.Color = Color.LightGreen;
            this.menuManager.Add(sceneID, clone);

            //add volume mute button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 2 * verticalBtnSeparation);
            clone.ID = "volumeMutebtn";
            clone.Text = "Volume Mute";
            //change the texture blend color
            clone.ColorParameters.Color = Color.LightBlue;
            this.menuManager.Add(sceneID, clone);

            //add back button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 3 * verticalBtnSeparation);
            clone.ID = "backbtn";
            clone.Text = "Back";
            //change the texture blend color
            clone.ColorParameters.Color = Color.LightYellow;
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
                transform, ColorParameters.WhiteOpaque, SpriteEffects.None,
                1, //depth is 1 so its always sorted to the back of other menu elements
                texture));

            //add back button - clone the audio button then just reset texture, ids etc in all the clones
            clone = (UIButtonObject)uiButtonObject.Clone();
            //move down on Y-axis for next button
            clone.Transform.Translation += new Vector2(0, 9 * verticalBtnSeparation);
            clone.ID = "backbtn";
            clone.Text = "Back";
            //change the texture blend color
            clone.ColorParameters.Color = Color.LightYellow;
            this.menuManager.Add(sceneID, clone);
            #endregion
        }
        #endregion

        #region UI
        private void InitializeUI()
        {
            //holds UI elements (e.g. reticule, inventory, progress)
            this.uiManager = new UIManager(this, this.spriteBatch, this.eventDispatcher, 10, StatusType.Off);
            Components.Add(this.uiManager);
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

            UITextureObject texture2DObject = new UIMouseObject("mouseObject",
                ActorType.UIDynamicTexture,
                StatusType.Drawn | StatusType.Update,
                new Transform2D(Vector2.One),
                new ColorParameters(Color.Yellow, 0.5f),
                SpriteEffects.None,
                this.fontDictionary["mouse"],
                "hello world!",
                new Vector2(0, -40),
                Color.White,
                1,
                texture,
                sourceRectangle,
                new Vector2(sourceRectangle.Width / 2.0f, sourceRectangle.Height / 2.0f),
                this.mouseManager,
                this.cameraManager,
                AppData.PickStartDistance,
                AppData.PickEndDistance);
            this.uiManager.Add(texture2DObject);
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

            texture = this.textureDictionary["progress_gradient"];
            scale = new Vector2(1, 0.75f);

            #region Player 1 Progress Bar
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f - texture.Width * scale.X - separation, verticalOffset);
            transform = new Transform2D(position, 0, scale, Vector2.Zero, new Integer2(texture.Width, texture.Height));

            textureObject = new UITextureObject(AppData.PlayerOneProgressID,
                    ActorType.UIDynamicTexture,
                    StatusType.Drawn | StatusType.Update,
                    transform, new ColorParameters(Color.Green, 1),
                    SpriteEffects.None,
                    0,
                    texture);

            //add a controller which listens for pickupeventdata send when the player (or red box) collects the box on the left
            textureObject.AttachController(new UIProgressController(AppData.PlayerOneProgressControllerID, ControllerType.UIProgress, 2, 10, this.eventDispatcher));
            this.uiManager.Add(textureObject);
            #endregion


            #region Player 2 Progress Bar
            position = new Vector2(graphics.PreferredBackBufferWidth / 2.0f + separation, verticalOffset);
            transform = new Transform2D(position, 0, scale, Vector2.Zero, new Integer2(texture.Width, texture.Height));

            textureObject = new UITextureObject(AppData.PlayerTwoProgressID,
                    ActorType.UIDynamicTexture,
                    StatusType.Drawn | StatusType.Update,
                    transform, 
                    new ColorParameters(Color.Red, 1),
                    SpriteEffects.None,
                    0,
                    texture);

            //add a controller which listens for pickupeventdata send when the player (or red box) collects the box on the left
            textureObject.AttachController(new UIProgressController(AppData.PlayerTwoProgressControllerID, ControllerType.UIProgress, 8, 10, this.eventDispatcher));
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
            this.modelEffect = new BasicEffect(graphics.GraphicsDevice);
            //enable the use of a texture on a model
            this.modelEffect.TextureEnabled = true;
            //setup the effect to have a single default light source which will be used to calculate N.L and N.H lighting
            this.modelEffect.EnableDefaultLighting();


            //used by the skybox which doesn't respond to lighting within the scene i.e. the skybox is effectively unlit
            this.unlitModelEffect = new BasicEffect(graphics.GraphicsDevice);
            //enable the use of a texture on a model
            this.unlitModelEffect.TextureEnabled = true;
        }
        #endregion

        #region Content, Update, Draw        
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            #region Add Menu & UI
            InitializeMenu();
            AddMenuElements();
            InitializeUI();
            AddUIElements();
            #endregion

#if DEBUG
            InitializeDebugTextInfo();
#endif

        }

        protected override void UnloadContent()
        {
            //formally call garbage collection to de-allocate resources from RAM
            this.modelDictionary.Dispose();
            this.textureDictionary.Dispose();
            this.fontDictionary.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {

            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            #region DEMO
            //only single in single screen layout since cycling in multi-screen is meaningless
            if (this.screenManager.ScreenType == ScreenUtility.ScreenType.SingleScreen && this.keyboardManager.IsFirstKeyPress(Keys.F1))
            {
                EventDispatcher.Publish(new EventData("set camera please", this, EventActionType.OnCameraCycle, EventCategoryType.Camera));
            }

            //testing event generation on opacity change - see DrawnActor3D::Alpha setter
            if (this.keyboardManager.IsFirstKeyPress(Keys.F5))
            {
                this.drivableBoxObject.Alpha -= 0.1f;
            }
            else if (this.keyboardManager.IsFirstKeyPress(Keys.F6))
            {
                this.drivableBoxObject.Alpha += 0.1f;
            }

            //testing event generation for UIProgressController
            if (this.keyboardManager.IsFirstKeyPress(Keys.F9))
            {
                //increase the left progress controller by 2
                object[] additionalEventParams = { AppData.PlayerOneProgressControllerID, (Integer)(-1)/*need brackets around number because of sign*/};
                EventDispatcher.Publish(new EventData("bla", this, EventActionType.OnHealthChange, EventCategoryType.Player, additionalEventParams));
            }
            else if (this.keyboardManager.IsFirstKeyPress(Keys.F10))
            {
                //increase the left progress controller by 2
                object[] additionalEventParams = { AppData.PlayerOneProgressControllerID, (Integer)1};
                EventDispatcher.Publish(new EventData("bla", this, EventActionType.OnHealthChange, EventCategoryType.Player, additionalEventParams));
            }

            if (this.keyboardManager.IsFirstKeyPress(Keys.F11))
            {
                //increase the left progress controller by 2
                object[] additionalEventParams = { AppData.PlayerTwoProgressControllerID, (Integer)(-1)/*need brackets around number because of sign*/};
                EventDispatcher.Publish(new EventData("bla", this, EventActionType.OnHealthChange, EventCategoryType.Player, additionalEventParams));
            }
            else if (this.keyboardManager.IsFirstKeyPress(Keys.F12))
            {
                //increase the left progress controller by 2
                object[] additionalEventParams = { AppData.PlayerTwoProgressControllerID, (Integer)1 };
                EventDispatcher.Publish(new EventData("bla", this, EventActionType.OnHealthChange, EventCategoryType.Player, additionalEventParams));
            }
            #endregion

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
