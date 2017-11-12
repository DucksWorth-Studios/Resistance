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
        private List<Actor3D> removeList, opaqueDrawList, transparentDrawList;
        private RasterizerState rasterizerStateOpaque;
        private RasterizerState rasterizerStateTransparent;
        #endregion

        #region Properties   
        public List<Actor3D> OpaqueDrawList
        {
            get
            {
                return this.opaqueDrawList;
            }
        }
        public List<Actor3D> TransparentDrawList
        {
            get
            {
                return this.transparentDrawList;
            }
        }
        #endregion


#if DEBUG
        //count the number of drawn objects to show frustum culling is happening - see DebugDrawer::Draw()
        private int debugDrawCount;

        public int DebugDrawCount
        {
            get
            {
                return this.debugDrawCount;
            }
        }
#endif

        public ObjectManager(Game game, CameraManager cameraManager, EventDispatcher eventDispatcher, int initialSize)
        {
            this.game = game;
            this.cameraManager = cameraManager;

            //create two lists - opaque and transparent
            this.opaqueDrawList = new List<Actor3D>(initialSize);
            this.transparentDrawList = new List<Actor3D>(initialSize);
            //create list to store objects to be removed at start of each update
            this.removeList = new List<Actor3D>(initialSize);

            //set up graphic settings
            InitializeGraphics();

            //register with the event dispatcher for the events of interest
            RegisterForEventHandling(eventDispatcher);
        }

        #region Event Handling
        protected virtual void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.OpacityChanged += EventDispatcher_OpacityChanged;
            eventDispatcher.RemoveActorChanged += EventDispatcher_RemoveActorChanged;
        }

        private void EventDispatcher_RemoveActorChanged(EventData eventData)
        {
            if (eventData.EventType == EventActionType.Remove)
            {
                this.Remove(eventData.Sender as Actor3D);
            }
        }

        private void EventDispatcher_OpacityChanged(EventData eventData)
        {
            Actor3D actor = eventData.Sender as Actor3D;

            if (eventData.EventType == EventActionType.OnOpaqueToTransparent)
            {
                //remove from opaque and add to transparent
                this.opaqueDrawList.Remove(actor);
                this.transparentDrawList.Add(actor);
            }
            else if (eventData.EventType == EventActionType.OnTransparentToOpaque)
            {
                //remove from transparent and add to opaque
                this.transparentDrawList.Remove(actor);
                this.opaqueDrawList.Add(actor);
            }
        }

        #endregion

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

        public void Add(Actor3D actor)
        {
            if (actor.GetAlpha() == 1)
                this.opaqueDrawList.Add(actor);
            else
                this.transparentDrawList.Add(actor);
        }

        //call when we want to remove a drawn object from the scene
        public void Remove(Actor3D actor)
        {
            this.removeList.Add(actor);
        }

        public int Remove(Predicate<Actor3D> predicate)
        {
            List<Actor3D> resultList = null;

            resultList = this.opaqueDrawList.FindAll(predicate);
            if ((resultList != null) && (resultList.Count != 0)) //the actor(s) were found in the opaque list
            {
                foreach (Actor3D actor in resultList)
                    this.removeList.Add(actor);
            }
            else //the actor(s) were found in the transparent list
            {
                resultList = this.transparentDrawList.FindAll(predicate);

                if ((resultList != null) && (resultList.Count != 0))
                    foreach (Actor3D actor in resultList)
                        this.removeList.Add(actor);
            }

            return resultList != null ? resultList.Count : 0;

        }

        //batch remove on all objects that were requested to be removed
        protected virtual void ApplyRemove()
        {
            foreach (Actor3D actor in this.removeList)
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
            foreach (Actor3D actor in this.opaqueDrawList)
            {
                if ((actor.GetStatusType() & StatusType.Update) != 0) //if update flag is set
                    actor.Update(gameTime);
            }

            //update all your transparent objects
            foreach (Actor3D actor in this.transparentDrawList)
            {
                if ((actor.GetStatusType() & StatusType.Update) != 0) //if update flag is set
                {
                    actor.Update(gameTime);
                    //used to sort objects by distance from the camera so that proper depth representation will be shown
                    MathUtility.SetDistanceFromCamera(actor as Actor3D, this.cameraManager.ActiveCamera);
                }
            }

            //sort so that the transparent objects closest to the camera are the LAST transparent objects drawn
            SortTransparentByDistance();
        }

        private void SortTransparentByDistance()
        {
            //sorting in descending order
            this.transparentDrawList.Sort((a, b) => (b.Transform.DistanceToCamera.CompareTo(a.Transform.DistanceToCamera)));
        }

        public void Draw(GameTime gameTime, Camera3D activeCamera)
        {
            //modify Draw() method to pass in the currently active camera - used to support multiple camera viewports - see ScreenManager::Draw()
            //set the viewport dimensions to the size defined by the active camera
            this.game.GraphicsDevice.Viewport = activeCamera.Viewport;

#if DEBUG
            //count the number of drawn objects to show frustum culling is happening - see DebugDrawer::Draw()
            this.debugDrawCount = 0;
#endif

            SetGraphicsStateObjects(true);
            foreach (Actor3D actor in this.opaqueDrawList)
            {
                DrawByType(gameTime, actor as Actor3D, activeCamera);
            }

            SetGraphicsStateObjects(false);
            foreach (Actor3D actor in this.transparentDrawList)
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
            if (activeCamera.BoundingFrustum.Intersects(modelObject.BoundingSphere))
            {
                if (modelObject.Model != null)
                {
                    BasicEffect effect = modelObject.Effect as BasicEffect;
                    effect.View = activeCamera.View;
                    effect.Projection = activeCamera.ProjectionParameters.Projection;

                    effect.Texture = modelObject.Texture;
                    effect.DiffuseColor = modelObject.Color.ToVector3();
                    effect.Alpha = modelObject.Alpha;

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
#if DEBUG
                debugDrawCount++;
#endif
            }
        }
    }
}
