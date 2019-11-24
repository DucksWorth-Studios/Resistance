/*
Function: 		This is the parent class for all primitives drawn where the developer explicitly defines the vertex data (i.e. position, color, normal, UV).
                Note: 
                - The class is generic and can be used to draw VertexPositionColor, VertexPositionColorTexture, VertexPositionColorNormal types etc.
                - For each draw call the vertex data is sent from RAM to VRAM. You an imagine that this is expensive - See BufferedVertexData.

Author: 		NMCG
Version:		1.0
Date Updated:	27/11/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class VertexData<T> : IVertexData where T : struct, IVertexType
    {
        public VertexData(T[] vertices, PrimitiveType primitiveType, int primitiveCount)
        {
            Vertices = vertices;
            PrimitiveType = primitiveType;
            PrimitiveCount = primitiveCount;
        }

        public virtual void Draw(GameTime gameTime, Effect effect)
        {
            effect.GraphicsDevice.DrawUserPrimitives(PrimitiveType, Vertices, 0, PrimitiveCount);
        }

        public object Clone()
        {
            return new VertexData<T>(
                Vertices, //shallow - reference
                PrimitiveType, //struct - deep
                PrimitiveCount); //deep - primitive
        }

        #region Variables

        #endregion

        #region Properties

        public PrimitiveType PrimitiveType { get; }

        public int PrimitiveCount { get; }

        public T[] Vertices { get; set; }

        #endregion
    }
}