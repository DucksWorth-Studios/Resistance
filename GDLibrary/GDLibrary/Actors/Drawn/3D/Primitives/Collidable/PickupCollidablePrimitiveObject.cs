namespace GDLibrary
{
    public class PickupCollidablePrimitiveObject : CollidablePrimitiveObject
    {
        #region Variables

        #endregion

        //used to draw collidable primitives that a value associated with them e.g. health
        public PickupCollidablePrimitiveObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, StatusType statusType, IVertexData vertexData,
            ICollisionPrimitive collisionPrimitive, ObjectManager objectManager, PickupParameters pickupParameters)
            : base(id, actorType, transform, effectParameters, statusType, vertexData, collisionPrimitive,
                objectManager)
        {
            PickupParameters = pickupParameters;
        }

        #region Properties

        public PickupParameters PickupParameters { get; set; }

        #endregion

        public new object Clone()
        {
            return new PickupCollidablePrimitiveObject("clone - " + ID, //deep
                ActorType, //deep
                (Transform3D) Transform.Clone(), //deep
                (EffectParameters) EffectParameters.Clone(), //deep
                StatusType, //deep
                VertexData, //shallow - its ok if objects refer to the same vertices
                (ICollisionPrimitive) CollisionPrimitive.Clone(), //deep
                ObjectManager, //shallow - reference
                (PickupParameters) PickupParameters.Clone()); //deep 
        }
    }
}