/*
Function: 		Store, update, and draw all visible objects
Author: 		NMCG
Version:		1.2
Date Updated:	21/11/17
Bugs:			None
Fixes:			None
Mods:           Removed DrawableGameComponent to support ScreenManager, added support for EffectParameters
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class ObjectManager
    {
        public ObjectManager(Game game, CameraManager cameraManager, EventDispatcher eventDispatcher, int initialSize)
        {
            this.game = game;
            this.cameraManager = cameraManager;

            //create two lists - opaque and transparent
            OpaqueDrawList = new List<Actor3D>(initialSize);
            TransparentDrawList = new List<Actor3D>(initialSize);
            //create list to store objects to be removed at start of each update
            removeList = new List<Actor3D>(initialSize);

            //set up graphic settings
            InitializeGraphics();

            //register with the event dispatcher for the events of interest
            RegisterForEventHandling(eventDispatcher);
        }

        private void InitializeGraphics()
        {
            //set the graphics card to repeat the end pixel value for any UV value outside 0-1
            //See http://what-when-how.com/xna-game-studio-4-0-programmingdeveloping-for-windows-phone-7-and-xbox-360/samplerstates-xna-game-studio-4-0-programming/
            var samplerState = new SamplerState();
            samplerState.AddressU = TextureAddressMode.Mirror;
            samplerState.AddressV = TextureAddressMode.Mirror;
            game.GraphicsDevice.SamplerStates[0] = samplerState;

            //opaque objects
            rasterizerStateOpaque = new RasterizerState();
            rasterizerStateOpaque.CullMode = CullMode.CullCounterClockwiseFace;

            //transparent objects
            rasterizerStateTransparent = new RasterizerState();
            rasterizerStateTransparent.CullMode = CullMode.None;
        }

        private void SetGraphicsStateObjects(bool isOpaque)
        {
            //Remember this code from our initial aliasing problems with the Sky box?
            //enable anti-aliasing along the edges of the quad i.e. to remove jagged edges to the primitive
            game.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;

            if (isOpaque)
            {
                //set the appropriate state for opaque objects
                game.GraphicsDevice.RasterizerState = rasterizerStateOpaque;

                //disable to see what happens when we disable depth buffering - look at the boxes
                game.GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
            else
            {
                //set the appropriate state for transparent objects
                game.GraphicsDevice.RasterizerState = rasterizerStateTransparent;

                //enable alpha blending for transparent objects i.e. trees
                game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

                //disable to see what happens when we disable depth buffering - look at the boxes
                game.GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;
            }
        }

        public void Add(Actor3D actor)
        {
            if (actor.GetAlpha() == 1)
                OpaqueDrawList.Add(actor);
            else
                TransparentDrawList.Add(actor);
        }

        public Actor3D Find(Predicate<Actor3D> predicate)
        {
            var actor = OpaqueDrawList.Find(predicate);

            return actor;
        }

        //call when we want to remove a drawn object from the scene
        public void Remove(Actor3D actor)
        {
            removeList.Add(actor);
        }

        public int Remove(Predicate<Actor3D> predicate)
        {
            List<Actor3D> resultList = null;

            resultList = OpaqueDrawList.FindAll(predicate);
            if (resultList != null && resultList.Count != 0) //the actor(s) were found in the opaque list
            {
                foreach (var actor in resultList)
                    removeList.Add(actor);
            }
            else //the actor(s) were found in the transparent list
            {
                resultList = TransparentDrawList.FindAll(predicate);

                if (resultList != null && resultList.Count != 0)
                    foreach (var actor in resultList)
                        removeList.Add(actor);
            }

            return resultList != null ? resultList.Count : 0;
        }

        //batch remove on all objects that were requested to be removed
        protected virtual void ApplyRemove()
        {
            foreach (var actor in removeList)
                if (actor.GetAlpha() == 1)
                    OpaqueDrawList.Remove(actor);
                else
                    TransparentDrawList.Remove(actor);

            removeList.Clear();
        }

        public void Update(GameTime gameTime)
        {
            //remove any outstanding objects since the last update
            ApplyRemove();

            //update all your opaque objects
            foreach (var actor in OpaqueDrawList)
                if ((actor.GetStatusType() & StatusType.Update) == StatusType.Update) //if update flag is set
                    actor.Update(gameTime);

            //update all your transparent objects
            foreach (var actor in TransparentDrawList)
                if ((actor.GetStatusType() & StatusType.Update) == StatusType.Update) //if update flag is set
                {
                    actor.Update(gameTime);
                    //used to sort objects by distance from the camera so that proper depth representation will be shown
                    MathUtility.SetDistanceFromCamera(actor, cameraManager.ActiveCamera);
                }

            //sort so that the transparent objects closest to the camera are the LAST transparent objects drawn
            SortTransparentByDistance();
        }

        private void SortTransparentByDistance()
        {
            //sorting in descending order
            TransparentDrawList.Sort((a, b) => b.Transform.DistanceToCamera.CompareTo(a.Transform.DistanceToCamera));
        }

        public void Draw(GameTime gameTime, Camera3D activeCamera)
        {
            //modify Draw() method to pass in the currently active camera - used to support multiple camera viewports - see ScreenManager::Draw()
            //set the viewport dimensions to the size defined by the active camera
            game.GraphicsDevice.Viewport = activeCamera.Viewport;

#if DEBUG
            //count the number of drawn objects to show frustum culling is happening - see DebugDrawer::Draw()
            DebugDrawCount = 0;
#endif

            SetGraphicsStateObjects(true);
            foreach (var actor in OpaqueDrawList) DrawByType(gameTime, actor, activeCamera);

            SetGraphicsStateObjects(false);
            foreach (var actor in TransparentDrawList) DrawByType(gameTime, actor, activeCamera);
        }

        //calls the correct DrawObject() based on underlying object type
        private void DrawByType(GameTime gameTime, Actor3D actor, Camera3D activeCamera)
        {
            //was the drawn enum value set?
            if ((actor.StatusType & StatusType.Drawn) == StatusType.Drawn)
            {
                if (actor is AnimatedPlayerObject)
                    DrawObject(gameTime, actor as AnimatedPlayerObject, activeCamera);
                else if (actor is ModelObject)
                    DrawObject(gameTime, actor as ModelObject, activeCamera);
                else if (actor is BillboardPrimitiveObject)
                    DrawObject(gameTime, actor as BillboardPrimitiveObject, activeCamera);
                else if (actor is PrimitiveObject) DrawObject(gameTime, actor as PrimitiveObject, activeCamera);
                //we will add additional else...if statements here to render other object types (e.g model, animated, billboard etc)
            }
        }

        //draw a NON-TEXTURED primitive i.e. vertices (and possibly indices) defined by the user
        private void DrawObject(GameTime gameTime, PrimitiveObject primitiveObject, Camera3D activeCamera)
        {
            if (activeCamera.BoundingFrustum.Intersects(primitiveObject.BoundingSphere))
            {
                primitiveObject.EffectParameters.SetParameters(activeCamera);
                primitiveObject.EffectParameters.SetWorld(primitiveObject.GetWorldMatrix());
                primitiveObject.VertexData.Draw(gameTime, primitiveObject.EffectParameters.Effect);

#if DEBUG
                DebugDrawCount++;
#endif
            }
        }

        private void DrawObject(GameTime gameTime, BillboardPrimitiveObject billboardPrimitiveObject,
            Camera3D activeCamera)
        {
            if (activeCamera.BoundingFrustum.Intersects(billboardPrimitiveObject.BoundingSphere))
            {
                billboardPrimitiveObject.EffectParameters.SetParameters(activeCamera,
                    billboardPrimitiveObject.BillboardOrientationParameters);
                billboardPrimitiveObject.EffectParameters.SetWorld(billboardPrimitiveObject.GetWorldMatrix());
                billboardPrimitiveObject.VertexData.Draw(gameTime, billboardPrimitiveObject.EffectParameters.Effect);

#if DEBUG
                DebugDrawCount++;
#endif
            }
        }

        //draw a model object 
        private void DrawObject(GameTime gameTime, ModelObject modelObject, Camera3D activeCamera)
        {
            //if (activeCamera.BoundingFrustum.Intersects(modelObject.BoundingSphere))
            {
                if (modelObject.Model != null)
                {
                    modelObject.EffectParameters.SetParameters(activeCamera);
                    foreach (var mesh in modelObject.Model.Meshes)
                    {
                        foreach (var part in mesh.MeshParts) part.Effect = modelObject.EffectParameters.Effect;
                        modelObject.EffectParameters.SetWorld(
                            modelObject.BoneTransforms[mesh.ParentBone.Index] * modelObject.GetWorldMatrix());
                        mesh.Draw();
                    }
                }
#if DEBUG
                DebugDrawCount++;
#endif
            }
        }

        private void DrawObject(GameTime gameTime, AnimatedPlayerObject animatedPlayerObject, Camera3D activeCamera)
        {
            //an array of the current positions of the model meshes
            var bones = animatedPlayerObject.AnimationPlayer.GetSkinTransforms();
            var world = animatedPlayerObject.GetWorldMatrix();

            for (var i = 0; i < bones.Length; i++) bones[i] *= world;

            foreach (var mesh in animatedPlayerObject.Model.Meshes)
            {
                foreach (SkinnedEffect skinnedEffect in mesh.Effects)
                {
                    skinnedEffect.SetBoneTransforms(bones);
                    skinnedEffect.View = activeCamera.View;
                    skinnedEffect.Projection = activeCamera.Projection;

                    //if you want to overwrite the texture you baked into the animation in 3DS Max then set your own texture
                    if (animatedPlayerObject.EffectParameters.Texture != null)
                        skinnedEffect.Texture = animatedPlayerObject.EffectParameters.Texture;

                    skinnedEffect.DiffuseColor = animatedPlayerObject.EffectParameters.DiffuseColor.ToVector3();
                    skinnedEffect.Alpha = animatedPlayerObject.Alpha;
                    skinnedEffect.EnableDefaultLighting();
                    skinnedEffect.PreferPerPixelLighting = true;
                }

                mesh.Draw();
            }

#if DEBUG
            DebugDrawCount++;
#endif
        }

        #region Fields

        private readonly Game game;
        private readonly CameraManager cameraManager;
        private readonly List<Actor3D> removeList;
        private RasterizerState rasterizerStateOpaque;
        private RasterizerState rasterizerStateTransparent;

        #endregion

        #region Properties   

        public List<Actor3D> OpaqueDrawList { get; }

        public List<Actor3D> TransparentDrawList { get; }

        #endregion


#if DEBUG
        //count the number of drawn objects to show frustum culling is happening - see DebugDrawer::Draw()

        public int DebugDrawCount { get; private set; }
#endif

        #region Event Handling

        protected virtual void RegisterForEventHandling(EventDispatcher eventDispatcher)
        {
            eventDispatcher.OpacityChanged += EventDispatcher_OpacityChanged;
            eventDispatcher.RemoveActorChanged += EventDispatcher_RemoveActorChanged;
            eventDispatcher.AddActorChanged += EventDispatcher_AddActorChanged;
        }

        private void EventDispatcher_AddActorChanged(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnAddActor) Add(eventData.Sender as Actor3D);
        }

        private void EventDispatcher_RemoveActorChanged(EventData eventData)
        {
            if (eventData.EventType == EventActionType.OnRemoveActor) Remove(eventData.Sender as Actor3D);
        }

        private void EventDispatcher_OpacityChanged(EventData eventData)
        {
            var actor = eventData.Sender as Actor3D;

            if (eventData.EventType == EventActionType.OnOpaqueToTransparent)
            {
                //remove from opaque and add to transparent
                OpaqueDrawList.Remove(actor);
                TransparentDrawList.Add(actor);
            }
            else if (eventData.EventType == EventActionType.OnTransparentToOpaque)
            {
                //remove from transparent and add to opaque
                TransparentDrawList.Remove(actor);
                OpaqueDrawList.Add(actor);
            }
        }

        #endregion
    }
}