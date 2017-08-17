/*
Function: 		Allows us to draw models objects. These are the FBX files we export from 3DS Max. 
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace GDLibrary
{
    public class ModelObject : DrawnActor3D, ICloneable
    {
        #region Variables
        private Texture2D texture;
        private Model model;
        private Matrix[] boneTransforms;
        #endregion

        #region Properties
        public Texture2D Texture
        {
            get
            {
                return this.texture;
            }
            set
            {
                this.texture = value;
            }
        }
        public Model Model
        {
            get
            {
                return this.model;
            }
            set
            {
                this.model = value;
            }
        }
        public Matrix[] BoneTransforms
        {
            get
            {
                return this.boneTransforms;
            }
            set
            {
                this.boneTransforms = value;
            }
        }
        #endregion

        public ModelObject(string id, ActorType actorType, 
            Transform3D transform, Effect effect, Color color, float alpha,
            Texture2D texture, Model model)
            : base(id, actorType, transform, effect, color, alpha, StatusType.Drawn | StatusType.Update)
        {
            this.texture = texture;
            this.model = model;

            /* 3DS Max models contain meshes (e.g. a table might have 5 meshes i.e. a top and 4 legs) and each mesh contains a bone.
            *  A bone holds the transform that says "move this mesh to this position". Without 5 bones in a table all the meshes would collapse down to be centred on the origin.
            *  Our table, wouldnt look very much like a table!
            *  
            *  Take a look at the ObjectManager::DrawObject(GameTime gameTime, ModelObject modelObject) method and see if you can figure out what the line below is doing:
            *  
            *  effect.World = modelObject.BoneTransforms[mesh.ParentBone.Index] * modelObject.GetWorldMatrix();
            */
            InitializeBoneTransforms();
        }

        private void InitializeBoneTransforms()
        {
            //load bone transforms and copy transfroms to transform array (transforms)
            if (this.model != null)
            {
                this.boneTransforms = new Matrix[this.model.Bones.Count];
                model.CopyAbsoluteBoneTransformsTo(this.boneTransforms);
            }
        }


        //See ObjectManager::Draw()
        //public override void Draw(GameTime gameTime)
        //{
        //    BasicEffect basicEffect = this.Effect as BasicEffect;

        //    basicEffect.View = cameraManager.ActiveCamera.View;
        //    basicEffect.Projection = cameraManager.ActiveCamera.Projection;
        //    basicEffect.World = this.Transform3D.World;
        //    basicEffect.DiffuseColor = this.Color.ToVector3();
        //    basicEffect.Alpha = this.Alpha;
        //    basicEffect.Texture = this.texture;
        //    basicEffect.CurrentTechnique.Passes[0].Apply();

        //    foreach (ModelMesh mesh in this.Model.Meshes)
        //    {
        //        foreach (ModelMeshPart part in mesh.MeshParts)
        //        {
        //            part.Effect = this.Effect;
        //        }

        //        basicEffect.World
        //            = this.BoneTransforms[mesh.ParentBone.Index]
        //                                * GetWorldMatrix(); //CD-CR support
        //        *this.Transform3D.World;
        //        mesh.Draw();
        //    }

        //    base.Draw(gameTime);
        //}


        public new object Clone()
        {
            return new ModelObject("clone - " + ID, //deep
                this.ActorType,   //deep
                (Transform3D)this.Transform3D.Clone(),  //deep
                this.Effect, //shallow i.e. a reference
                this.Color,  //deep
                this.Alpha,  //deep
                this.texture, //shallow i.e. a reference
                this.model); //shallow i.e. a reference
        }

        public override bool Remove()
        {
            //tag for garbage collection
            this.boneTransforms = null;
            //notice how the base Remove() is called. What will happen when this is called? See Actor3D::Remove().
            return base.Remove();
        }
    }
}
