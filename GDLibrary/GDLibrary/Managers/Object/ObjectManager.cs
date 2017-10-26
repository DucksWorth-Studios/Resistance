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
    public class ObjectManager 
    {
        #region Fields
        private Game game;
        private CameraManager cameraManager;
        private List<IActor> removeList, opaqueDrawList, transparentDrawList;
        private RasterizerState rasterizerStateOpaque;
        private RasterizerState rasterizerStateTransparent;
        private PhysicsDebugDrawer physicsDebugDrawer;
        #endregion

        #region Properties   
        public List<IActor> OpaqueDrawList
        {
            get
            {
                return this.opaqueDrawList;
            }
        }
        public List<IActor> TransparentDrawList
        {
            get
            {
                return this.transparentDrawList;
            }
        }
        #endregion

        public ObjectManager(Game game, CameraManager cameraManager, int initialSize)
        {
            this.game = game;
            this.cameraManager = cameraManager;

            //create two lists - opaque and transparent
            this.opaqueDrawList = new List<IActor>(initialSize);
            this.transparentDrawList = new List<IActor>(initialSize);
            this.removeList = new List<IActor>(initialSize);

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
            if (actor.GetAlpha() == 1)
                this.opaqueDrawList.Add(actor);
            else
                this.transparentDrawList.Add(actor);
        }

        //call when we want to remove a drawn object from the scene
        public void Remove(Actor actor)
        {
            this.removeList.Add(actor);
        }

        public int Remove(Predicate<IActor> predicate)
        {
            List<IActor> resultList = null;

            resultList = this.opaqueDrawList.FindAll(predicate);
            if ((resultList != null) && (resultList.Count != 0)) //the actor(s) were found in the opaque list
            {
                foreach (Actor actor in resultList)
                    this.removeList.Add(actor);
            }
            else //the actor(s) were found in the transparent list
            {
                resultList = this.transparentDrawList.FindAll(predicate);

                if ((resultList != null) && (resultList.Count != 0))
                    foreach (Actor actor in resultList)
                        this.removeList.Add(actor);
            }

            return resultList != null ? resultList.Count : 0;

        }

        //batch remove on all objects that were requested to be removed
        protected virtual void ApplyRemove()
        {
            foreach (Actor actor in this.removeList)
            {
                if (actor.GetAlpha() == 1)
                    this.opaqueDrawList.Remove(actor);
                else
                    this.transparentDrawList.Remove(actor);
            }

            this.removeList.Clear();
        }

        public void Update(GameTime gameTime)
        {
            //remove any outstanding objects since the last update
            ApplyRemove();

            //update all your opaque objects
            foreach (IActor actor in this.opaqueDrawList)
            {
                if ((actor.GetStatusType() & StatusType.Update) != 0) //if update flag is set
                    actor.Update(gameTime);
            }

            //update all your transparent objects
            foreach (IActor actor in this.transparentDrawList)
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
            foreach (IActor actor in this.opaqueDrawList)
            {
                DrawByType(gameTime, actor as Actor3D, activeCamera);
            }

            SetGraphicsStateObjects(false);
            foreach (IActor actor in this.transparentDrawList)
            {
                DrawByType(gameTime, actor as Actor3D, activeCamera);
            }
        }

        //calls the correct DrawObject() based on underlying object type
        private void DrawByType(GameTime gameTime, Actor3D actor, Camera3D activeCamera)
        {
            //was the drawn enum value set?
            if ((actor.StatusType & StatusType.Drawn) == StatusType.Drawn)
            {
                if (actor is ModelObject)
                {
                    DrawObject(gameTime, actor as ModelObject, activeCamera);
                }
                //we will add additional else...if statements here to render other object types (e.g model, animated, billboard etc)
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
    }
}
