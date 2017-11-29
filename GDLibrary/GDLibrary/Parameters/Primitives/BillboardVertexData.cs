/*
Function: 		This child class for drawing billboard primitives - these are primitives that automatically orient themselves in a specific way
                in relation to the camera (i.e. cylindrical, spherical) or point in a user-defined direction.
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
    public class BillboardVertexData<T> : VertexData<T> where T : struct, IVertexType
    {
        #region Variables
        private DynamicVertexBuffer vertexBuffer;
        private GraphicsDevice graphicsDevice;
        #endregion

        #region Properties

        public DynamicVertexBuffer VertexBuffer
        {
            get
            {
                return vertexBuffer;
            }
            set
            {
                vertexBuffer = value;

            }
        }
        #endregion

        public BillboardVertexData(GraphicsDevice graphicsDevice, T[] vertices, 
            PrimitiveType primitiveType, int primitiveCount)
            : base(vertices, primitiveType, primitiveCount)
        {
            this.graphicsDevice = graphicsDevice;

            //reserve space on gfx will be CHANGED frequently
            this.vertexBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(T), vertices.Length, BufferUsage.None);
            //set data on the reserved space
            this.vertexBuffer.SetData<T>(this.Vertices);
        }

        public override void Draw(GameTime gameTime, Effect effect)
        {
            //this is what we want GFX to draw
            effect.GraphicsDevice.SetVertexBuffer(this.vertexBuffer);

            //draw!
            effect.GraphicsDevice.DrawPrimitives(this.PrimitiveType, 0, this.PrimitiveCount);
        }

        public new object Clone()
        {
            return new BillboardVertexData<T>(this.graphicsDevice, this.Vertices, this.PrimitiveType, this.PrimitiveCount);
        }
    }
}
