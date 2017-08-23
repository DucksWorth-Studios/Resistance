/*
Function: 		Store, update, and draw all visible objects
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace GDLibrary
{
    public class ObjectManager : DrawableGameComponent
    {

        #region Variables
        private CameraManager cameraManager;
        private List<IActor> drawList;
        #endregion

        #region Properties   
        #endregion

        public ObjectManager(Game game, CameraManager cameraManager, int initialSize)
            : base(game)
        {
            this.cameraManager = cameraManager;
            this.drawList = new List<IActor>(initialSize);

            InitializeGraphics(game);       
        }

        private void InitializeGraphics(Game game)
        {
            //set the graphics card to repeat the end pixel value for any UV value outside 0-1
            //See http://what-when-how.com/xna-game-studio-4-0-programmingdeveloping-for-windows-phone-7-and-xbox-360/samplerstates-xna-game-studio-4-0-programming/
            SamplerState samplerState = new SamplerState();
            samplerState.AddressU = TextureAddressMode.Clamp;
            samplerState.AddressV = TextureAddressMode.Clamp;
            game.GraphicsDevice.SamplerStates[0] = samplerState;

            //enable alpha blending for transparent objects i.e. trees
            game.GraphicsDevice.BlendState = BlendState.AlphaBlend;

            //draw the forward and backward facing surfaces for all objects in the game - expensive - we will need to refine this approach in time
            RasterizerState rasterizerStateTransparent = new RasterizerState();
            rasterizerStateTransparent.CullMode = CullMode.None;
            game.GraphicsDevice.RasterizerState = rasterizerStateTransparent;
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

        public override void Draw(GameTime gameTime)
        {
            foreach (IActor actor in this.drawList)
            {
                DrawObject(gameTime, actor as ModelObject);
            }
        }

        //draw a model object 
        private void DrawObject(GameTime gameTime, ModelObject modelObject)
        {
            if (modelObject.Model != null)
            {
                //set the viewport dimensions to the size defined by the active camera
               //this.Game.GraphicsDevice.Viewport = cameraManager.ActiveCamera.Viewport;

                BasicEffect effect = modelObject.Effect as BasicEffect;
                effect.View = cameraManager.ActiveCamera.View;
                effect.Projection = cameraManager.ActiveCamera.ProjectionParameters.Projection;

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
        }
    }
}
