/*
Function: 		Allows us to draw models objects. These are the FBX files we export from 3DS Max. 
Author: 		NMCG
Version:		1.0
Date Updated:	17/8/17
Bugs:			None
Fixes:			None
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class ModelObject : DrawnActor3D, ICloneable
    {
        //default draw and update settings for statusType
        public ModelObject(string id, ActorType actorType,
            Transform3D transform, EffectParameters effectParameters, Model model)
            : this(id, actorType, transform, effectParameters, model, StatusType.Update | StatusType.Drawn)
        {
        }

        public ModelObject(string id, ActorType actorType,
            Transform3D transform, EffectParameters effectParameters, Model model, StatusType statusType)
            : base(id, actorType, transform, effectParameters, statusType)
        {
            Model = model;

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

        //default draw and update settings for statusType
        public ModelObject(string id, ActorType actorType,
            Transform3D transform, EffectParameters effectParameters, Model model, float boundingSphereMultiplier)
            : this(id, actorType, transform, effectParameters, model, StatusType.Update | StatusType.Drawn)
        {
            this.boundingSphereMultiplier = boundingSphereMultiplier;
        }

        public ModelObject(string id, ActorType actorType,
            Transform3D transform, EffectParameters effectParameters, Model model, StatusType statusType,
            float boundingSphereMultiplier)
            : base(id, actorType, transform, effectParameters, statusType)
        {
            Model = model;
            this.boundingSphereMultiplier = boundingSphereMultiplier;

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

        public new object Clone()
        {
            var actor = new ModelObject("clone - " + ID, //deep
                ActorType, //deep
                (Transform3D) Transform.Clone(), //deep
                EffectParameters.GetDeepCopy(), //hybrid - shallow (texture and effect) and deep (all other fields) 
                Model); //shallow i.e. a reference

            if (ControllerList != null)
                //clone each of the (behavioural) controllers
                foreach (var controller in ControllerList)
                    actor.AttachController((IController) controller.Clone());

            return actor;
        }

        private void InitializeBoneTransforms()
        {
            //load bone transforms and copy transfroms to transform array (transforms)
            if (Model != null)
            {
                BoneTransforms = new Matrix[Model.Bones.Count];
                Model.CopyAbsoluteBoneTransformsTo(BoneTransforms);
            }
        }

        public override bool Equals(object obj)
        {
            var other = obj as ModelObject;

            if (other == null)
                return false;
            if (this == other)
                return true;

            return Model.Equals(other.Model) && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hash = 1;
            hash = hash * 11 + Model.GetHashCode();
            hash = hash * 17 + base.GetHashCode();
            return hash;
        }

        public override bool Remove()
        {
            //tag for garbage collection
            BoneTransforms = null;
            //notice how the base Remove() is called. What will happen when this is called? See DrawnActor3D::Remove().
            return base.Remove();
        }

        #region Fields

        private float boundingSphereMultiplier = 1.1f;

        #endregion

        #region Properties

        public Model Model { get; set; }

        public Matrix[] BoneTransforms { get; set; }

        public float BoundingSphereMultiplier
        {
            get => boundingSphereMultiplier;
            set
            {
                if (value > 0)
                    boundingSphereMultiplier = value;
            }
        }

        public BoundingSphere BoundingSphere =>
            Model.Meshes[Model.Root.Index].BoundingSphere.Transform(Matrix.CreateScale(boundingSphereMultiplier)
                                                                    * GetWorldMatrix());

        #endregion
    }
}