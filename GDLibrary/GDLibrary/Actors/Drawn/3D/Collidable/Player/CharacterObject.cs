using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Math;
using JigLibX.Physics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    /// <summary>
    ///     Provides a parent class representing a player (or non-player) character
    ///     with appropriate collision skin (i.e. capsule) and move rates (e.g. acceleration)
    ///     Inherit from this class to create a specific player (e.g. PlayerObject extends CharacterObject)
    ///     then set Body::velocity and Transform3D::rotation to move and turn the character
    ///     - See PlayerObject
    /// </summary>
    public class CharacterObject : CollidableObject
    {
        public CharacterObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, Model model,
            float radius, float height, float accelerationRate, float decelerationRate)
            : base(id, actorType, transform, effectParameters, model)
        {
            Body = new Character(accelerationRate, decelerationRate);
            Collision = new CollisionSkin(Body);
            Body.ExternalData = this;
            Body.CollisionSkin = Collision;
            var capsule = new Capsule(Vector3.Zero, Matrix.CreateRotationX(MathHelper.PiOver2), radius, height);
            Collision.AddPrimitive(capsule, (int) MaterialTable.MaterialID.NormalSmooth);
        }

        public Character CharacterBody => Body as Character;


        public override void Enable(bool bImmovable, float mass)
        {
            base.Enable(bImmovable, mass);
            Body.SetBodyInvInertia(0.0f, 0.0f, 0.0f);
            //   CharacterBody = Body as Character;
            Body.AllowFreezing = false;
            Body.EnableBody();
        }

        public override void Update(GameTime gameTime)
        {
            //update actual position of the model e.g. used by rail camera controllers
            Transform.Translation = Body.Transform.Position;
            base.Update(gameTime);
        }

        public override Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(Transform.Scale) *
                   Collision.GetPrimitiveLocal(0).Transform.Orientation *
                   Body.Orientation *
                   Transform.Orientation *
                   Matrix.CreateTranslation(Body.Position);
        }


        //add equals, gethashcode, clone, remove...
    }

    internal class ASkinPredicate : CollisionSkinPredicate1
    {
        public override bool ConsiderSkin(CollisionSkin skin0)
        {
            if (!(skin0.Owner is Character))
                return true;
            return false;
        }
    }

    public class Character : Body
    {
        public Character(float accelerationRate, float decelerationRate)
        {
            this.accelerationRate = accelerationRate;
            this.decelerationRate = decelerationRate;
        }

        public void DoJump(float jumpHeight)
        {
            this.jumpHeight = jumpHeight;
            IsJumping = true;
        }

        public override void AddExternalForces(float dt)
        {
            ClearForces();

            if (IsJumping)
                foreach (var info in CollisionSkin.Collisions)
                {
                    var N = info.DirToBody0;
                    if (this == info.SkinInfo.Skin1.Owner)
                        Vector3.Negate(ref N, out N);

                    if (Vector3.Dot(N, Orientation.Up) > 0.7f)
                    {
                        var vel = Velocity;
                        vel.Y = jumpHeight;
                        Velocity = vel;
                        break;
                    }
                }

            var deltaVel = DesiredVelocity - Velocity;

            var running = true;

            if (DesiredVelocity.LengthSquared() < JiggleMath.Epsilon)
                running = false;
            else
                deltaVel.Normalize();

            deltaVel.Y = -2.0f;

            // start fast, slow down slower
            if (running)
                deltaVel *= accelerationRate; //acceleration multiplier
            else
                deltaVel *= decelerationRate; //deceleration multiplier

            var forceFactor = 500.0f;
            AddBodyForce(deltaVel * Mass * dt * forceFactor);
            IsJumping = false;
            AddGravityToExternalForce();
        }

        #region Fields

        private float jumpHeight = 5;

        public float accelerationRate { get; set; }
        public float decelerationRate { get; set; }
        public Vector3 DesiredVelocity { get; set; }

        #endregion

        #region Properties

        public bool IsJumping { get; private set; }

        public bool IsCrouching { get; set; }

        #endregion
    }
}