/*
Function: 		This child class for drawing primitives where the vertex data is buffered on the GFX card in VRAM.
                Note: 
                - The class is generic and can be used to draw VertexPositionColor, VertexPositionColorTexture, VertexPositionColorNormal types etc.
                - For each draw the GFX card refers to vertex data that has already been buffered to VRAM 
                - This is a more efficient approach than either using the VertexData or DynamicBufferedVertexData classes if
                  you wish to draw a large number of primitives on screen.

Author: 		NMCG
Version:		1.0
Date Updated:	27/11/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class BufferedVertexData<T> : VertexData<T> where T : struct, IVertexType
    {
        #region Variables
        private VertexBuffer vertexBuffer;
        private GraphicsDevice graphicsDevice;
        #endregion

        #region Properties
        public VertexBuffer VertexBuffer
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

        public BufferedVertexData(GraphicsDevice graphicsDevice, T[] vertices, PrimitiveType primitiveType, int primitiveCount)
            : base(vertices, primitiveType, primitiveCount)
        {
            this.graphicsDevice = graphicsDevice;

            //reserve space on gfx
            this.VertexBuffer = new VertexBuffer(graphicsDevice, typeof(T), vertices.Length, BufferUsage.None);

            //move data to the space
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
            return new BufferedVertexData<T>(this.graphicsDevice, this.Vertices, this.PrimitiveType, this.PrimitiveCount);
        }
    }
}
