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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public class BufferedVertexData<T> : VertexData<T> where T : struct, IVertexType
    {
        //allows developer to pass in vertices AND buffer - more efficient since buffer is defined ONCE outside of the object instead of a new VertexBuffer for EACH instance of the class
        public BufferedVertexData(GraphicsDevice graphicsDevice, T[] vertices, VertexBuffer vertexBuffer,
            PrimitiveType primitiveType, int primitiveCount)
            : base(vertices, primitiveType, primitiveCount)
        {
            GraphicsDevice = graphicsDevice;
            VertexBuffer = vertexBuffer;

            //set data on the reserved space
            VertexBuffer.SetData(Vertices);
        }

        //buffer is created INSIDE the class so each class has a buffer - not efficient
        public BufferedVertexData(GraphicsDevice graphicsDevice, T[] vertices, PrimitiveType primitiveType,
            int primitiveCount)
            : base(vertices, primitiveType, primitiveCount)
        {
            GraphicsDevice = graphicsDevice;
            VertexBuffer = new VertexBuffer(graphicsDevice, typeof(T), vertices.Length, BufferUsage.None);

            //set data on the reserved space
            VertexBuffer.SetData(Vertices);
        }


        public void SetData(T[] vertices)
        {
            Vertices = vertices;
            //set data on the reserved space
            VertexBuffer.SetData(Vertices);
        }

        public override void Draw(GameTime gameTime, Effect effect)
        {
            //this is what we want GFX to draw
            effect.GraphicsDevice.SetVertexBuffer(VertexBuffer);

            //draw!
            effect.GraphicsDevice.DrawPrimitives(PrimitiveType, 0, PrimitiveCount);
        }

        public new object Clone()
        {
            return new BufferedVertexData<T>(GraphicsDevice, //shallow - reference
                Vertices, //shallow - reference
                PrimitiveType, //struct - deep
                PrimitiveCount); //deep - primitive
        }

        #region Variables

        #endregion

        #region Properties

        public VertexBuffer VertexBuffer { get; set; }

        public GraphicsDevice GraphicsDevice { get; }

        #endregion
    }
}