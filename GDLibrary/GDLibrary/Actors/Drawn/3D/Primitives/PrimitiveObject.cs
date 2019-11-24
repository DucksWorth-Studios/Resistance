/*
Function: 		Allows us to draw primitives by explicitly defining the vertex data.
                Used in you I-CA project.
                 
Author: 		NMCG
Version:		1.0
Date Updated:	27/11/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class PrimitiveObject : DrawnActor3D
    {
        #region Variables

        #endregion

        public PrimitiveObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, StatusType statusType, IVertexData vertexData)
            : base(id, actorType, transform, effectParameters, statusType)
        {
            VertexData = vertexData;
        }

        public new object Clone()
        {
            var actor = new PrimitiveObject("clone - " + ID, //deep
                ActorType, //deep
                (Transform3D) Transform.Clone(), //deep
                (EffectParameters) EffectParameters.Clone(), //deep
                StatusType, //deep
                VertexData); //shallow - its ok if objects refer to the same vertices

            if (ControllerList != null)
                //clone each of the (behavioural) controllers
                foreach (var controller in ControllerList)
                    actor.AttachController((IController) controller.Clone());

            return actor;
        }

        #region Properties

        public IVertexData VertexData { get; set; }

        public BoundingSphere BoundingSphere => new BoundingSphere(Transform.Translation, Transform.Scale.Length());

        #endregion
    }
}