/*
Function: 		Represents an area that can detect collisions by using only a simple BoundingSphere or AA BoundingBox. It does 
                NOT have an associated model. We can use this class to create activation zones e.g. for camera switching or event generation

Author: 		NMCG
Version:		1.0
Date Updated:	27/11/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class SimpleZoneObject : Actor3D
    {
        #region Variables

        #endregion

        public SimpleZoneObject(string id, ActorType actorType, Transform3D transform, StatusType statusType,
            ICollisionPrimitive collisionPrimitive)
            : base(id, actorType, transform, statusType)
        {
            CollisionPrimitive = collisionPrimitive;
        }

        #region Properties

        public ICollisionPrimitive CollisionPrimitive { get; set; }

        #endregion

        public override void Update(GameTime gameTime)
        {
            //update collision primitive with new object position
            if (CollisionPrimitive != null)
                CollisionPrimitive.Update(gameTime, Transform);
        }
    }
}