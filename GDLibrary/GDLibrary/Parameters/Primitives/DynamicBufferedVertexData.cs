/*
Function: 		This child class for drawing primitives where the vertex data can change over time (e.g. where you use a controller to modify the position of vertices).
                You can use this class to create a wall of vertices that undulate over time (i.e. like a animated polygon sea as in something like Monument Valley).
                Note: 
                - The class is generic and can be used to draw VertexPositionColor, VertexPositionColorTexture, VertexPositionColorNormal types etc.
                - For each draw call the vertex data is sent from RAM to VRAM. You an imagine that this is expensive - See BufferedVertexData.

Author: 		NMCG
Version:		1.0
Date Updated:	27/11/17
Bugs:			None
Fixes:			None
*/

using Microsoft.Xna.Framework.Graphics;
using System;
using Microsoft.Xna.Framework;

namespace GDLibrary
{
    public class DynamicBufferedVertexData<T> : VertexData<T> where T : struct, IVertexType
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

        public DynamicBufferedVertexData(GraphicsDevice graphicsDevice, T[] vertices, PrimitiveType primitiveType, int primitiveCount)
            : base(vertices, primitiveType, primitiveCount)
        {
            this.graphicsDevice = graphicsDevice;

            //reserve space on gfx will be CHANGED frequently
            this.vertexBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(T), vertices.Length, BufferUsage.None);
            //add an event listener to reset the data if another game object access the graphics device and (potentially) resets buffer contents
            this.vertexBuffer.ContentLost += vertexBuffer_ContentLost;
            UpdateData();
        }

        public void SetData(T[] vertices)
        {
            this.Vertices = vertices;
            //set data on the reserved space
            this.vertexBuffer.SetData<T>(this.Vertices);
        }

        public void UpdateData()
        {
            //set data on the reserved space
            this.vertexBuffer.SetData<T>(this.Vertices);
        }

        void vertexBuffer_ContentLost(object sender, EventArgs e)
        {
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
            return new DynamicBufferedVertexData<T>(this.graphicsDevice, this.Vertices, this.PrimitiveType, this.PrimitiveCount);
        }
    }
}
