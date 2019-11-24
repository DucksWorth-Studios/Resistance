using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Math;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class CollidableObject : ModelObject
    {
        public CollidableObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, Model model)
            : base(id, actorType, transform, effectParameters, model)
        {
            Body = new Body();
            Body.ExternalData = this;
            Collision = new CollisionSkin(Body);
            Body.CollisionSkin = Collision;

            //we will only add this event handling in a class that sub-classes CollidableObject e.g. PickupCollidableObject or PlayerCollidableObject
            //this.body.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
        }

        public CollidableObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters,
            Model model, float boundingSphereMultiplier)
            : base(id, actorType, transform, effectParameters, model, boundingSphereMultiplier)
        {
            Body = new Body();
            Body.ExternalData = this;
            Collision = new CollisionSkin(Body);
            Body.CollisionSkin = Collision;

            //we will only add this event handling in a class that sub-classes CollidableObject e.g. PickupCollidableObject or PlayerCollidableObject
            //this.body.CollisionSkin.callbackFn += CollisionSkin_callbackFn;
        }

        //we will only add this method in a class that sub-classes CollidableObject e.g. PickupCollidableObject or PlayerCollidableObject
        //private bool CollisionSkin_callbackFn(CollisionSkin skin0, CollisionSkin skin1)
        //{
        //    return true;
        //}

        public override Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(Transform.Scale) *
                   Collision.GetPrimitiveLocal(0).Transform.Orientation *
                   Body.Orientation *
                   Transform.Orientation *
                   Matrix.CreateTranslation(Body.Position);
        }


        protected Vector3 SetMass(float mass)
        {
            var primitiveProperties = new PrimitiveProperties(PrimitiveProperties.MassDistributionEnum.Solid,
                PrimitiveProperties.MassTypeEnum.Density, mass);

            float junk;
            Vector3 com;
            Matrix it, itCoM;

            Collision.GetMassProperties(primitiveProperties, out junk, out com, out it, out itCoM);
            Body.BodyInertia = itCoM;
            Body.Mass = junk;

            return com;
        }

        public void AddPrimitive(Primitive primitive, MaterialProperties materialProperties)
        {
            Collision.AddPrimitive(primitive, materialProperties);
        }

        public virtual void Enable(bool bImmovable, float mass)
        {
            Mass = mass;

            //set whether the object can move
            Body.Immovable = bImmovable;
            //calculate the centre of mass
            var com = SetMass(mass);
            //adjust skin so that it corresponds to the 3D mesh as drawn on screen
            Body.MoveTo(Transform.Translation, Matrix.Identity);
            //set the centre of mass
            Collision.ApplyLocalTransform(new Transform(-com, Matrix.Identity));
            //enable so that any applied forces (e.g. gravity) will affect the object
            Body.EnableBody();
        }


        public new object Clone()
        {
            return new CollidableObject("clone - " + ID, //deep
                ActorType, //deep
                (Transform3D) Transform.Clone(), //deep
                EffectParameters.GetDeepCopy(), //hybrid - shallow (texture and effect) and deep (all other fields) 
                Model); //shallow i.e. a reference
        }

        public override bool Remove()
        {
            Body = null;
            return base.Remove();
        }

        #region Variables

        #endregion

        #region Properties

        public float Mass { get; set; }

        public CollisionSkin Collision { get; set; }

        public Body Body { get; set; }

        #endregion
    }
}