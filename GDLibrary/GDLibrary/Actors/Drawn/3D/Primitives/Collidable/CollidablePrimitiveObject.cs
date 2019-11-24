using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class CollidablePrimitiveObject : PrimitiveObject
    {
        //used to draw collidable primitives that have a texture i.e. use VertexPositionColor vertex types only
        public CollidablePrimitiveObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, StatusType statusType, IVertexData vertexData,
            ICollisionPrimitive collisionPrimitive, ObjectManager objectManager)
            : base(id, actorType, transform, effectParameters, statusType, vertexData)
        {
            CollisionPrimitive = collisionPrimitive;
            //unusual to pass this in but we use it to test for collisions - see Update();
            ObjectManager = objectManager;
        }


        public override void Update(GameTime gameTime)
        {
            //reset collidee to prevent colliding with the same object in the next update
            Collidee = null;

            //reset any movements applied in the previous update from move keys
            Transform.TranslateIncrement = Vector3.Zero;
            Transform.RotateIncrement = 0;

            //update collision primitive with new object position
            if (CollisionPrimitive != null)
                CollisionPrimitive.Update(gameTime, Transform);

            base.Update(gameTime);
        }

        //read and store movement suggested by keyboard input
        protected virtual void HandleInput(GameTime gameTime)
        {
        }

        //define what happens when a collision occurs
        protected virtual void HandleCollisionResponse(Actor collidee)
        {
        }

        //test for collision against all opaque and transparent objects
        protected virtual Actor CheckCollisions(GameTime gameTime)
        {
            foreach (IActor actor in ObjectManager.OpaqueDrawList)
            {
                Collidee = CheckCollisionWithActor(gameTime, actor as Actor3D);
                if (Collidee != null)
                    return Collidee;
            }

            foreach (IActor actor in ObjectManager.TransparentDrawList)
            {
                Collidee = CheckCollisionWithActor(gameTime, actor as Actor3D);
                if (Collidee != null)
                    return Collidee;
            }

            return null;
        }

        //test for collision against a specific object
        private Actor CheckCollisionWithActor(GameTime gameTime, Actor3D actor3D)
        {
            //dont test for collision against yourself - remember the player is in the object manager list too!
            if (this != actor3D)
            {
                if (actor3D is CollidablePrimitiveObject)
                {
                    var collidableObject = actor3D as CollidablePrimitiveObject;
                    if (CollisionPrimitive.Intersects(collidableObject.CollisionPrimitive,
                        Transform.TranslateIncrement))
                        return collidableObject;
                }
                else if (actor3D is SimpleZoneObject)
                {
                    var zoneObject = actor3D as SimpleZoneObject;
                    if (CollisionPrimitive.Intersects(zoneObject.CollisionPrimitive, Transform.TranslateIncrement))
                        return zoneObject;
                }
            }

            return null;
        }

        //apply suggested movement since no collision will occur if the player moves to that position
        protected virtual void ApplyInput(GameTime gameTime)
        {
            //was a move/rotate key pressed, if so then these values will be > 0 in dimension
            if (Transform.TranslateIncrement != Vector3.Zero)
                Transform.TranslateBy(Transform.TranslateIncrement);

            if (Transform.RotateIncrement != 0)
                Transform.RotateAroundYBy(Transform.RotateIncrement);
        }

        public new object Clone()
        {
            return new CollidablePrimitiveObject("clone - " + ID, //deep
                ActorType, //deep
                (Transform3D) Transform.Clone(), //deep
                (EffectParameters) EffectParameters.Clone(), //deep
                StatusType, //deep
                VertexData, //shallow - its ok if objects refer to the same vertices
                (ICollisionPrimitive) CollisionPrimitive.Clone(), //deep
                ObjectManager); //shallow - reference
        }

        #region Variables

        //the skin used to wrap the object

        //the object that im colliding with

        #endregion

        #region Properties

        //returns a reference to whatever this object is colliding against
        public Actor Collidee { get; set; }

        public ICollisionPrimitive CollisionPrimitive { get; set; }

        public ObjectManager ObjectManager { get; }

        #endregion
    }
}