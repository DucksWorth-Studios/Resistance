using GDLibrary;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace GDApp
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Main : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        private CameraManager cameraManager;
        public ObjectManager objectManager { get; private set; }

        public Main()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            #region Set the screen resolution
            //obviously this will affect the viewport for the camera and does use the same aspect ratio as the camera i.e. 4/3
            int width = 1024, height = 768; //4.3 ratiow
            graphics.PreferredBackBufferWidth = width;
            graphics.PreferredBackBufferHeight = height;
            //if we forget to apply the changes then our resolution wont be set!
            graphics.ApplyChanges();
            #endregion

            #region Initialize the effect (shader) for models
            BasicEffect modelEffect = new BasicEffect(graphics.GraphicsDevice);
            //enable the use of a texture on a model
            modelEffect.TextureEnabled = true;
            //setup the effect to have a single default light source which will be used to calculate N.L and N.H lighting
            modelEffect.EnableDefaultLighting();
            #endregion

            #region Add the CameraManager and ObjectManager
            this.cameraManager = new CameraManager(this, 1);
            Components.Add(this.cameraManager);

            this.objectManager = new ObjectManager(this, cameraManager, 10);
            Components.Add(this.objectManager);
            #endregion

            //we will use this variable for the camera and re-use for the modelobject
            Transform3D transform = null;

            #region Add the camera 
            //add the camera
            transform = new Transform3D(new Vector3(0, 0, 10), -Vector3.UnitZ, Vector3.UnitY);

            //set the camera to occupy the entire viewport
            Viewport viewPort = new Viewport(0, 0, width, height);
            //initialise the camera
            Camera3D simpleCamera = new Camera3D("simple camera", ActorType.Camera, transform, ProjectionParameters.StandardMediumFourThree, viewPort, 0, StatusType.Drawn | StatusType.Update);
            this.cameraManager.Add(simpleCamera);
            #endregion

            #region Add the model object
            //load the texture
            Texture2D texture = Content.Load<Texture2D>("Assets/Textures/Props/Crates/crate1");

            //load the model file i.e. the vertices of the model from the 3DS Max file
            Model boxModel = Content.Load<Model>("Assets/Models/box2");

            //use one of our static defaults to position the object at the origin
            transform = Transform3D.Zero;
            //initialise the boxObject
            ModelObject boxObject = new ModelObject("box1", ActorType.Decorator, transform, modelEffect, Color.White, 1, texture, boxModel);
            //add to the objectManager so that it will be drawn and updated
            this.objectManager.Add(boxObject);

            //a clone variable that we can reuse
            ModelObject clone = null;

            //add a clone of the box model object to test the clone
            clone = (ModelObject)boxObject.Clone();
            clone.Transform3D.Translation = new Vector3(5, 0, 0);
            //scale it to make it look different
            clone.Transform3D.Scale = new Vector3(1, 4, 1);
            //change its color
            clone.Color = Color.Red;
            this.objectManager.Add(clone);

            //add more clones here...


            #endregion

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
