/*
Function: 		Store, update, and draw all visible objects
Author: 		NMCG
Version:		1.1
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
Mods:           Removed DrawableGameComponent to support ScreenManager
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GDLibrary
{
    public class ObjectManager //: DrawableGameComponent
    {

        #region Fields
        private CameraManager cameraManager;
        private List<IActor> drawList;
        private Game game;
        private RasterizerState rasterizerStateOpaque;
        private RasterizerState rasterizerStateTransparent;
        private PhysicsDebugDrawer physicsDebugDrawer;
        #endregion

        #region Properties   
        #endregion

        public ObjectManager(Game game, CameraManager cameraManager, PhysicsDebugDrawer physicsDebugDrawer, int initialSize)
        //   : base(game)
        {
            this.game = game;
            this.cameraManager = cameraManager;
            this.physicsDebugDrawer = physicsDebugDrawer;
            this.drawList = new List<IActor>(initialSize);

            //set up graphic settings
            InitializeGraphics();
        }

        private void InitializeGraphics()
        {
            //set the graphics card to repeat the end pixel value for any UV value outside 0-1
            //See http://what-when-how.com/xna-game-studio-4-0-programmingdeveloping-for-windows-phone-7-and-xbox-360/samplerstates-xna-game-studio-4-0-programming/
            SamplerState samplerState = new SamplerState();
            samplerState.AddressU = TextureAddressMode.Clamp;
            samplerState.AddressV = TextureAddressMode.Clamp;
            game.GraphicsDevice.SamplerStates[0] = samplerState;

            //opaque objects
            this.rasterizerStateOpaque = new RasterizerState();
            this.rasterizerStateOpaque.CullMode = CullMode.CullCounterClockwiseFace;

            //transparent objects
            this.rasterizerStateTransparent = new RasterizerState();
            this.rasterizerStateTransparent.CullMode = CullMode.None;
        }
        private void SetGraphicsStateObjects(bool isOpaque)
        {
            //Remember this code from our initial aliasing problems with the Sky box?
            //enable anti-aliasing along the edges of the quad i.e. to remove jagged edges to the primitive
            game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            if (isOpaque)
            {
                //set the appropriate state for opaque objects
                game.GraphicsDevice.RasterizerState = this.rasterizerStateOpaque;

                //disable to see what happens when we disable depth buffering - look at the boxes
                game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
            else
            {
                //set the appropriate state for transparent objects
                game.GraphicsDevice.RasterizerState = this.rasterizerStateTransparent;

                //enable alpha blending for transparent objects i.e. trees
                game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

                //disable to see what happens when we disable depth buffering - look at the boxes
                game.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            }
        }


        public void Add(IActor actor)
        {
            this.drawList.Add(actor);
        }

        public bool Remove(Predicate<IActor> predicate)
        {
            IActor foundActor = this.drawList.Find(predicate);
            if(foundActor != null)
                return this.drawList.Remove(foundActor);

            return false;
        }

        public int RemoveAll(Predicate<IActor> predicate)
        {
            return this.drawList.RemoveAll(predicate);
        }

        public void Update(GameTime gameTime)
        {
            foreach (IActor actor in this.drawList)
            {
                if ((actor.GetStatusType() & StatusType.Update) != 0) //if update flag is set
                    actor.Update(gameTime);
            }
        }

        public void Draw(GameTime gameTime, Camera3D activeCamera)
        {
            //modify Draw() method to pass in the currently active camera - used to support multiple camera viewports - see ScreenManager::Draw()
            //set the viewport dimensions to the size defined by the active camera
            this.game.GraphicsDevice.Viewport = activeCamera.Viewport;

            SetGraphicsStateObjects(true);

            foreach (IActor actor in this.drawList)
            {
                if ((actor.GetStatusType() & StatusType.Drawn) != 0) //if drawn flag is set
                {
                    DrawObject(gameTime, actor as ModelObject, activeCamera);
                    DrawCollisionSkin(actor);
                }


            }
        }

        //draw a model object 
        private void DrawObject(GameTime gameTime, ModelObject modelObject, Camera3D activeCamera)
        {
            if (modelObject.Model != null)
            {
                BasicEffect effect = modelObject.Effect as BasicEffect;
                effect.View = activeCamera.View;
                effect.Projection = activeCamera.ProjectionParameters.Projection;

                effect.Texture = modelObject.Texture;
                effect.DiffuseColor = modelObject.ColorParameters.Color.ToVector3();
                effect.Alpha = modelObject.ColorParameters.Alpha;

                //apply or serialise the variables above to the GFX card
                effect.CurrentTechnique.Passes[0].Apply();

                foreach (ModelMesh mesh in modelObject.Model.Meshes)
                {
                    foreach (ModelMeshPart part in mesh.MeshParts)
                    {
                        part.Effect = effect;
                    }
                    effect.World = modelObject.BoneTransforms[mesh.ParentBone.Index] * modelObject.GetWorldMatrix();
                    mesh.Draw();
                }
            }
        }

        //debug method to draw collision skins for collidable objects and zone objects
        private void DrawCollisionSkin(IActor actor)
        {
            if (actor is CollidableObject)
            {
                CollidableObject collidableObject = actor as CollidableObject;
                this.physicsDebugDrawer.DrawDebug(actor as CollidableObject);
            }
           
        }
    }
}
