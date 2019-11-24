/*
Function: 		A factory to generate common primitives used by your game.

Author: 		NMCG
Version:		1.0
Date Updated:	27/11/17
Bugs:			
Fixes:			None
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    public static class VertexFactory
    {
        //constant
        public static int ROUND_PRECISION_FLOAT = 3;

        /******************************************** Wireframe - Origin, Line, Circle, Quad, Cube & Billboard ********************************************/

        public static VertexPositionColor[] GetVerticesPositionColorLine(int sidelength,
            out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.LineList;
            primitiveCount = 1;

            var vertices = new VertexPositionColor[2];

            var halfSideLength = sidelength / 2.0f;

            var left = new Vector3(-halfSideLength, 0, 0);
            var right = new Vector3(halfSideLength, 0, 0);

            vertices[0] = new VertexPositionColor(left, Color.White);
            vertices[1] = new VertexPositionColor(right, Color.White);

            return vertices;
        }

        public static VertexPositionColor[] GetVerticesPositionColorOriginHelper(out PrimitiveType primitiveType,
            out int primitiveCount)
        {
            primitiveType = PrimitiveType.LineList;
            primitiveCount = 10;

            var vertices = new VertexPositionColor[20];

            //x-axis
            vertices[0] = new VertexPositionColor(-Vector3.UnitX, Color.DarkRed);
            vertices[1] = new VertexPositionColor(Vector3.UnitX, Color.DarkRed);

            //y-axis
            vertices[2] = new VertexPositionColor(-Vector3.UnitY, Color.DarkGreen);
            vertices[3] = new VertexPositionColor(Vector3.UnitY, Color.DarkGreen);

            //z-axis
            vertices[4] = new VertexPositionColor(-Vector3.UnitZ, Color.DarkBlue);
            vertices[5] = new VertexPositionColor(Vector3.UnitZ, Color.DarkBlue);

            //to do - x-text , y-text, z-text
            //x label
            vertices[6] = new VertexPositionColor(new Vector3(1.1f, 0.1f, 0), Color.DarkRed);
            vertices[7] = new VertexPositionColor(new Vector3(1.3f, -0.1f, 0), Color.DarkRed);
            vertices[8] = new VertexPositionColor(new Vector3(1.3f, 0.1f, 0), Color.DarkRed);
            vertices[9] = new VertexPositionColor(new Vector3(1.1f, -0.1f, 0), Color.DarkRed);


            //y label
            vertices[10] = new VertexPositionColor(new Vector3(-0.1f, 1.3f, 0), Color.DarkGreen);
            vertices[11] = new VertexPositionColor(new Vector3(0, 1.2f, 0), Color.DarkGreen);
            vertices[12] = new VertexPositionColor(new Vector3(0.1f, 1.3f, 0), Color.DarkGreen);
            vertices[13] = new VertexPositionColor(new Vector3(-0.1f, 1.1f, 0), Color.DarkGreen);

            //z label
            vertices[14] = new VertexPositionColor(new Vector3(0, 0.1f, 1.1f), Color.DarkBlue);
            vertices[15] = new VertexPositionColor(new Vector3(0, 0.1f, 1.3f), Color.DarkBlue);
            vertices[16] = new VertexPositionColor(new Vector3(0, 0.1f, 1.1f), Color.DarkBlue);
            vertices[17] = new VertexPositionColor(new Vector3(0, -0.1f, 1.3f), Color.DarkBlue);
            vertices[18] = new VertexPositionColor(new Vector3(0, -0.1f, 1.3f), Color.DarkBlue);
            vertices[19] = new VertexPositionColor(new Vector3(0, -0.1f, 1.1f), Color.DarkBlue);


            return vertices;
        }

        //returns the vertices for a simple sphere (i.e. 3 circles) with a user-defined radius and sweep angle
        public static VertexPositionColor[] GetVerticesPositionColorSphere(int radius, int sweepAngleInDegrees,
            out PrimitiveType primitiveType, out int primitiveCount)
        {
            var vertexList = new List<VertexPositionColor>();

            //get vertices for each plane e.g. XY, XZ
            var verticesSinglePlane =
                GetVerticesPositionColorCircle(radius, sweepAngleInDegrees, OrientationType.XYAxis);
            AddArrayElementsToList(verticesSinglePlane, vertexList);

            verticesSinglePlane = GetVerticesPositionColorCircle(radius, sweepAngleInDegrees, OrientationType.XZAxis);
            AddArrayElementsToList(verticesSinglePlane, vertexList);

            primitiveType = PrimitiveType.LineStrip;
            primitiveCount = vertexList.Count - 1;

            return vertexList.ToArray();
        }

        //Adds the contents of a list to an array
        public static void AddArrayElementsToList<T>(T[] array, List<T> list)
        {
            foreach (var obj in array)
                list.Add(obj);
        }

        //returns the vertices for a circle with a user-defined radius, sweep angle, and orientation
        public static VertexPositionColor[] GetVerticesPositionColorCircle(int radius, int sweepAngleInDegrees,
            OrientationType orientationType)
        {
            //sweep angle should also be >= 1 and a multiple of 360
            //if angle is not a multiple of 360 the circle will not close - remember we are drawing with a LineStrip
            if (sweepAngleInDegrees < 1 || 360 % sweepAngleInDegrees != 0)
                return null;

            //number of segments forming the circle (e.g. for sweepAngleInDegrees == 90 we have 4 segments)
            var segmentCount = 360 / sweepAngleInDegrees;

            //segment angle
            var rads = MathHelper.ToRadians(sweepAngleInDegrees);

            //we need one more vertex to close the circle (e.g. 4 + 1 vertices to draw four lines)
            var vertices = new VertexPositionColor[segmentCount + 1];

            float a, b;

            for (var i = 0; i < vertices.Length; i++)
            {
                //round the values so we dont end up with the two oordinate values very close to but not equals to 0
                a = (float) (radius * Math.Round(Math.Cos(rads * i), ROUND_PRECISION_FLOAT));
                b = (float) (radius * Math.Round(Math.Sin(rads * i), ROUND_PRECISION_FLOAT));

                if (orientationType == OrientationType.XYAxis)
                    vertices[i] = new VertexPositionColor(new Vector3(a, b, 0), Color.White);
                else if (orientationType == OrientationType.XZAxis)
                    vertices[i] = new VertexPositionColor(new Vector3(a, 0, b), Color.White);
                else
                    vertices[i] = new VertexPositionColor(new Vector3(0, a, b), Color.White);
            }

            return vertices;
        }

        public static VertexPositionColor[] GetVerticesPositionColorQuad(int sidelength,
            out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 2;

            var vertices = new VertexPositionColor[4];

            var halfSideLength = sidelength / 2.0f;

            var topLeft = new Vector3(-halfSideLength, halfSideLength, 0);
            var topRight = new Vector3(halfSideLength, halfSideLength, 0);
            var bottomLeft = new Vector3(-halfSideLength, -halfSideLength, 0);
            var bottomRight = new Vector3(halfSideLength, -halfSideLength, 0);

            //quad coplanar with the XY-plane (i.e. forward facing normal along UnitZ)
            vertices[0] = new VertexPositionColor(topLeft, Color.White);
            vertices[1] = new VertexPositionColor(topRight, Color.White);
            vertices[2] = new VertexPositionColor(bottomLeft, Color.White);
            vertices[3] = new VertexPositionColor(bottomRight, Color.White);

            return vertices;
        }

        //returns the vertices for a billboard which has a custom vertex declaration
        public static VertexBillboard[] GetVertexBillboard(int sidelength, out PrimitiveType primitiveType,
            out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 2;

            var vertices = new VertexBillboard[4];
            var halfSideLength = sidelength / 2.0f;

            var uvTopLeft = new Vector2(0, 0);
            var uvTopRight = new Vector2(1, 0);
            var uvBottomLeft = new Vector2(0, 1);
            var uvBottomRight = new Vector2(1, 1);

            //quad coplanar with the XY-plane (i.e. forward facing normal along UnitZ)
            vertices[0] = new VertexBillboard(Vector3.Zero, new Vector4(uvTopLeft, -halfSideLength, halfSideLength));
            vertices[1] = new VertexBillboard(Vector3.Zero, new Vector4(uvTopRight, halfSideLength, halfSideLength));
            vertices[2] =
                new VertexBillboard(Vector3.Zero, new Vector4(uvBottomLeft, -halfSideLength, -halfSideLength));
            vertices[3] =
                new VertexBillboard(Vector3.Zero, new Vector4(uvBottomRight, halfSideLength, -halfSideLength));

            return vertices;
        }

        public static VertexPositionColor[] GetVerticesPositionColorCube(int sidelength)
        {
            var vertices = new VertexPositionColor[36];

            var halfSideLength = sidelength / 2.0f;

            var topLeftFront = new Vector3(-halfSideLength, halfSideLength, halfSideLength);
            var topLeftBack = new Vector3(-halfSideLength, halfSideLength, -halfSideLength);
            var topRightFront = new Vector3(halfSideLength, halfSideLength, halfSideLength);
            var topRightBack = new Vector3(halfSideLength, halfSideLength, -halfSideLength);

            var bottomLeftFront = new Vector3(-halfSideLength, -halfSideLength, halfSideLength);
            var bottomLeftBack = new Vector3(-halfSideLength, -halfSideLength, -halfSideLength);
            var bottomRightFront = new Vector3(halfSideLength, -halfSideLength, halfSideLength);
            var bottomRightBack = new Vector3(halfSideLength, -halfSideLength, -halfSideLength);

            //top - 1 polygon for the top
            vertices[0] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[1] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[2] = new VertexPositionColor(topRightBack, Color.White);

            vertices[3] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[4] = new VertexPositionColor(topRightBack, Color.White);
            vertices[5] = new VertexPositionColor(topRightFront, Color.White);

            //front
            vertices[6] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[7] = new VertexPositionColor(topRightFront, Color.White);
            vertices[8] = new VertexPositionColor(bottomLeftFront, Color.White);

            vertices[9] = new VertexPositionColor(bottomLeftFront, Color.White);
            vertices[10] = new VertexPositionColor(topRightFront, Color.White);
            vertices[11] = new VertexPositionColor(bottomRightFront, Color.White);

            //back
            vertices[12] = new VertexPositionColor(bottomRightBack, Color.White);
            vertices[13] = new VertexPositionColor(topRightBack, Color.White);
            vertices[14] = new VertexPositionColor(topLeftBack, Color.White);

            vertices[15] = new VertexPositionColor(bottomRightBack, Color.White);
            vertices[16] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[17] = new VertexPositionColor(bottomLeftBack, Color.White);

            //left 
            vertices[18] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[19] = new VertexPositionColor(topLeftFront, Color.White);
            vertices[20] = new VertexPositionColor(bottomLeftFront, Color.White);

            vertices[21] = new VertexPositionColor(bottomLeftBack, Color.White);
            vertices[22] = new VertexPositionColor(topLeftBack, Color.White);
            vertices[23] = new VertexPositionColor(bottomLeftFront, Color.White);

            //right
            vertices[24] = new VertexPositionColor(bottomRightFront, Color.White);
            vertices[25] = new VertexPositionColor(topRightFront, Color.White);
            vertices[26] = new VertexPositionColor(bottomRightBack, Color.White);

            vertices[27] = new VertexPositionColor(topRightFront, Color.White);
            vertices[28] = new VertexPositionColor(topRightBack, Color.White);
            vertices[29] = new VertexPositionColor(bottomRightBack, Color.White);

            //bottom
            vertices[30] = new VertexPositionColor(bottomLeftFront, Color.White);
            vertices[31] = new VertexPositionColor(bottomRightFront, Color.White);
            vertices[32] = new VertexPositionColor(bottomRightBack, Color.White);

            vertices[33] = new VertexPositionColor(bottomLeftFront, Color.White);
            vertices[34] = new VertexPositionColor(bottomRightBack, Color.White);
            vertices[35] = new VertexPositionColor(bottomLeftBack, Color.White);

            return vertices;
        }

        //defined vertices for a new shape in our game
        public static VertexPositionColor[] GetColoredTriangle(out PrimitiveType primitiveType, out int primitiveCount)
        {
            var vertices = new VertexPositionColor[3];
            vertices[0] = new VertexPositionColor(new Vector3(0, 1, 0), Color.White); //T
            vertices[1] = new VertexPositionColor(new Vector3(1, 0, 0), Color.White); //R
            vertices[2] = new VertexPositionColor(new Vector3(-1, 0, 0), Color.White); //L

            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 1;

            return vertices;
        }

        //TriangleStrip
        public static VertexPositionColorTexture[] GetTextureQuadVertices(out PrimitiveType primitiveType,
            out int primitiveCount)
        {
            var halfLength = 0.5f;

            var topLeft = new Vector3(-halfLength, halfLength, 0);
            var topRight = new Vector3(halfLength, halfLength, 0);
            var bottomLeft = new Vector3(-halfLength, -halfLength, 0);
            var bottomRight = new Vector3(halfLength, -halfLength, 0);

            //quad coplanar with the XY-plane (i.e. forward facing normal along UnitZ)
            var vertices = new VertexPositionColorTexture[4];
            vertices[0] = new VertexPositionColorTexture(topLeft, Color.White, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(topRight, Color.White, Vector2.UnitX);
            vertices[2] = new VertexPositionColorTexture(bottomLeft, Color.White, Vector2.UnitY);
            vertices[3] = new VertexPositionColorTexture(bottomRight, Color.White, Vector2.One);

            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 2;

            return vertices;
        }

        public static VertexPositionColor[] GetSpiralVertices(int radius, int angleInDegrees, float verticalIncrement,
            out int primitiveCount)
        {
            var vertices = GetCircleVertices(radius, angleInDegrees, out primitiveCount,
                OrientationType.XZAxis);

            for (var i = 0; i < vertices.Length; i++) vertices[i].Position.Y = verticalIncrement * i;

            return vertices;
        }

        public static VertexPositionColor[] GetSphereVertices(int radius, int angleInDegrees, out int primitiveCount)
        {
            var vertList = new List<VertexPositionColor>();

            vertList.AddRange(GetCircleVertices(radius, angleInDegrees, out primitiveCount, OrientationType.XYAxis));
            vertList.AddRange(GetCircleVertices(radius, angleInDegrees, out primitiveCount, OrientationType.YZAxis));
            vertList.AddRange(GetCircleVertices(radius, angleInDegrees, out primitiveCount, OrientationType.XZAxis));
            primitiveCount = vertList.Count - 1;
            return vertList.ToArray();
        }

        public static VertexPositionColor[] GetCircleVertices(int radius, int angleInDegrees, out int primitiveCount,
            OrientationType orientationType)
        {
            primitiveCount = 360 / angleInDegrees;
            var vertices = new VertexPositionColor[primitiveCount + 1];

            var position = Vector3.Zero;
            var angleInRadians = MathHelper.ToRadians(angleInDegrees);

            for (var i = 0; i <= primitiveCount; i++)
            {
                if (orientationType == OrientationType.XYAxis)
                {
                    position.X = (float) (radius * Math.Cos(i * angleInRadians));
                    position.Y = (float) (radius * Math.Sin(i * angleInRadians));
                }
                else if (orientationType == OrientationType.XZAxis)
                {
                    position.X = (float) (radius * Math.Cos(i * angleInRadians));
                    position.Z = (float) (radius * Math.Sin(i * angleInRadians));
                }
                else
                {
                    position.Y = (float) (radius * Math.Cos(i * angleInRadians));
                    position.Z = (float) (radius * Math.Sin(i * angleInRadians));
                }

                vertices[i] = new VertexPositionColor(position, Color.White);
            }

            return vertices;
        }

        /******************************************** Textured - Quad, Cube & Pyramid ********************************************/

        public static VertexPositionColorTexture[] GetVerticesPositionColorTextureQuad(int sidelength,
            out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleStrip;
            primitiveCount = 2;

            var vertices = new VertexPositionColorTexture[4];
            var halfSideLength = sidelength / 2.0f;

            var topLeft = new Vector3(-halfSideLength, halfSideLength, 0);
            var topRight = new Vector3(halfSideLength, halfSideLength, 0);
            var bottomLeft = new Vector3(-halfSideLength, -halfSideLength, 0);
            var bottomRight = new Vector3(halfSideLength, -halfSideLength, 0);

            //quad coplanar with the XY-plane (i.e. forward facing normal along UnitZ)
            vertices[0] = new VertexPositionColorTexture(topLeft, Color.White, Vector2.Zero);
            vertices[1] = new VertexPositionColorTexture(topRight, Color.White, Vector2.UnitX);
            vertices[2] = new VertexPositionColorTexture(bottomLeft, Color.White, Vector2.UnitY);
            vertices[3] = new VertexPositionColorTexture(bottomRight, Color.White, Vector2.One);

            return vertices;
        }

        public static VertexPositionColorTexture[] GetVerticesPositionTexturedCube(int sidelength,
            out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList;
            primitiveCount = 12;

            var vertices = new VertexPositionColorTexture[36];

            var halfSideLength = sidelength / 2.0f;

            var topLeftFront = new Vector3(-halfSideLength, halfSideLength, halfSideLength);
            var topLeftBack = new Vector3(-halfSideLength, halfSideLength, -halfSideLength);
            var topRightFront = new Vector3(halfSideLength, halfSideLength, halfSideLength);
            var topRightBack = new Vector3(halfSideLength, halfSideLength, -halfSideLength);

            var bottomLeftFront = new Vector3(-halfSideLength, -halfSideLength, halfSideLength);
            var bottomLeftBack = new Vector3(-halfSideLength, -halfSideLength, -halfSideLength);
            var bottomRightFront = new Vector3(halfSideLength, -halfSideLength, halfSideLength);
            var bottomRightBack = new Vector3(halfSideLength, -halfSideLength, -halfSideLength);

            //uv coordinates
            var uvTopLeft = new Vector2(0, 0);
            var uvTopRight = new Vector2(1, 0);
            var uvBottomLeft = new Vector2(0, 1);
            var uvBottomRight = new Vector2(1, 1);


            //top - 1 polygon for the top
            vertices[0] = new VertexPositionColorTexture(topLeftFront, Color.White, uvBottomLeft);
            vertices[1] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[2] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);

            vertices[3] = new VertexPositionColorTexture(topLeftFront, Color.White, uvBottomLeft);
            vertices[4] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);
            vertices[5] = new VertexPositionColorTexture(topRightFront, Color.White, uvBottomRight);

            //front
            vertices[6] = new VertexPositionColorTexture(topLeftFront, Color.White, uvBottomLeft);
            vertices[7] = new VertexPositionColorTexture(topRightFront, Color.White, uvBottomRight);
            vertices[8] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);

            vertices[9] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);
            vertices[10] = new VertexPositionColorTexture(topRightFront, Color.White, uvBottomRight);
            vertices[11] = new VertexPositionColorTexture(bottomRightFront, Color.White, uvTopRight);

            //back
            vertices[12] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);
            vertices[13] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);
            vertices[14] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);

            vertices[15] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);
            vertices[16] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[17] = new VertexPositionColorTexture(bottomLeftBack, Color.White, uvBottomLeft);

            //left 
            vertices[18] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[19] = new VertexPositionColorTexture(topLeftFront, Color.White, uvTopRight);
            vertices[20] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvBottomRight);

            vertices[21] = new VertexPositionColorTexture(bottomLeftBack, Color.White, uvBottomLeft);
            vertices[22] = new VertexPositionColorTexture(topLeftBack, Color.White, uvTopLeft);
            vertices[23] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvBottomRight);

            //right
            vertices[24] = new VertexPositionColorTexture(bottomRightFront, Color.White, uvBottomLeft);
            vertices[25] = new VertexPositionColorTexture(topRightFront, Color.White, uvTopLeft);
            vertices[26] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);

            vertices[27] = new VertexPositionColorTexture(topRightFront, Color.White, uvTopLeft);
            vertices[28] = new VertexPositionColorTexture(topRightBack, Color.White, uvTopRight);
            vertices[29] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);

            //bottom
            vertices[30] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);
            vertices[31] = new VertexPositionColorTexture(bottomRightFront, Color.White, uvTopRight);
            vertices[32] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);

            vertices[33] = new VertexPositionColorTexture(bottomLeftFront, Color.White, uvTopLeft);
            vertices[34] = new VertexPositionColorTexture(bottomRightBack, Color.White, uvBottomRight);
            vertices[35] = new VertexPositionColorTexture(bottomLeftBack, Color.White, uvBottomLeft);

            return vertices;
        }

        public static VertexPositionColorTexture[] GetVerticesPositionTexturedPyramidSquare(int sidelength,
            out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList;
            primitiveCount = 6;

            var vertices = new VertexPositionColorTexture[18];
            var halfSideLength = sidelength / 2.0f;

            var topCentre =
                new Vector3(0, 0.71f * sidelength,
                    0); //multiplier gives a pyramid where the length of the rising edges == length of the base edges
            var frontLeft = new Vector3(-halfSideLength, 0, halfSideLength);
            var frontRight = new Vector3(halfSideLength, 0, halfSideLength);
            var backLeft = new Vector3(-halfSideLength, 0, -halfSideLength);
            var backRight = new Vector3(halfSideLength, 0, -halfSideLength);

            var uvTopCentre = new Vector2(0.5f, 0);
            var uvTopLeft = new Vector2(0, 0);
            var uvTopRight = new Vector2(1, 0);
            var uvBottomLeft = new Vector2(0, 1);
            var uvBottomRight = new Vector2(1, 1);

            //front 
            vertices[0] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[1] = new VertexPositionColorTexture(frontRight, Color.White, uvBottomRight);
            vertices[2] = new VertexPositionColorTexture(frontLeft, Color.White, uvBottomLeft);

            //left 
            vertices[3] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[4] = new VertexPositionColorTexture(frontLeft, Color.White, uvBottomRight);
            vertices[5] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomLeft);

            //right 
            vertices[6] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[7] = new VertexPositionColorTexture(backRight, Color.White, uvBottomRight);
            vertices[8] = new VertexPositionColorTexture(frontRight, Color.White, uvBottomLeft);

            //back 
            vertices[9] = new VertexPositionColorTexture(topCentre, Color.White, uvTopCentre);
            vertices[10] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomRight);
            vertices[11] = new VertexPositionColorTexture(backRight, Color.White, uvBottomLeft);

            //bottom 
            vertices[12] = new VertexPositionColorTexture(frontLeft, Color.White, uvTopLeft);
            vertices[13] = new VertexPositionColorTexture(frontRight, Color.White, uvTopRight);
            vertices[14] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomLeft);

            vertices[15] = new VertexPositionColorTexture(frontRight, Color.White, uvTopRight);
            vertices[16] = new VertexPositionColorTexture(backRight, Color.White, uvBottomRight);
            vertices[17] = new VertexPositionColorTexture(backLeft, Color.White, uvBottomLeft);

            return vertices;
        }

        /******************************************** Textured & Normal - Cube ********************************************/

        //adding normals - step 1 - add the vertices for the object shape
        public static VertexPositionNormalTexture[] GetVerticesPositionNormalTexturedCube(int sidelength,
            out PrimitiveType primitiveType, out int primitiveCount)
        {
            primitiveType = PrimitiveType.TriangleList;
            primitiveCount = 12;

            var vertices = new VertexPositionNormalTexture[36];

            var halfSideLength = sidelength / 2.0f;

            var topLeftFront = new Vector3(-halfSideLength, halfSideLength, halfSideLength);
            var topLeftBack = new Vector3(-halfSideLength, halfSideLength, -halfSideLength);
            var topRightFront = new Vector3(halfSideLength, halfSideLength, halfSideLength);
            var topRightBack = new Vector3(halfSideLength, halfSideLength, -halfSideLength);

            var bottomLeftFront = new Vector3(-halfSideLength, -halfSideLength, halfSideLength);
            var bottomLeftBack = new Vector3(-halfSideLength, -halfSideLength, -halfSideLength);
            var bottomRightFront = new Vector3(halfSideLength, -halfSideLength, halfSideLength);
            var bottomRightBack = new Vector3(halfSideLength, -halfSideLength, -halfSideLength);

            //uv coordinates
            var uvTopLeft = new Vector2(0, 0);
            var uvTopRight = new Vector2(1, 0);
            var uvBottomLeft = new Vector2(0, 1);
            var uvBottomRight = new Vector2(1, 1);


            //top - 1 polygon for the top
            vertices[0] = new VertexPositionNormalTexture(topLeftFront, Vector3.UnitY, uvBottomLeft);
            vertices[1] = new VertexPositionNormalTexture(topLeftBack, Vector3.UnitY, uvTopLeft);
            vertices[2] = new VertexPositionNormalTexture(topRightBack, Vector3.UnitY, uvTopRight);

            vertices[3] = new VertexPositionNormalTexture(topLeftFront, Vector3.UnitY, uvBottomLeft);
            vertices[4] = new VertexPositionNormalTexture(topRightBack, Vector3.UnitY, uvTopRight);
            vertices[5] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitY, uvBottomRight);

            //front
            vertices[6] = new VertexPositionNormalTexture(topLeftFront, Vector3.UnitZ, uvBottomLeft);
            vertices[7] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitZ, uvBottomRight);
            vertices[8] = new VertexPositionNormalTexture(bottomLeftFront, Vector3.UnitZ, uvTopLeft);

            vertices[9] = new VertexPositionNormalTexture(bottomLeftFront, Vector3.UnitZ, uvTopLeft);
            vertices[10] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitZ, uvBottomRight);
            vertices[11] = new VertexPositionNormalTexture(bottomRightFront, Vector3.UnitZ, uvTopRight);

            //back
            vertices[12] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitZ, uvBottomRight);
            vertices[13] = new VertexPositionNormalTexture(topRightBack, -Vector3.UnitZ, uvTopRight);
            vertices[14] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitZ, uvTopLeft);

            vertices[15] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitZ, uvBottomRight);
            vertices[16] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitZ, uvTopLeft);
            vertices[17] = new VertexPositionNormalTexture(bottomLeftBack, -Vector3.UnitZ, uvBottomLeft);

            //left 
            vertices[18] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitX, uvTopLeft);
            vertices[19] = new VertexPositionNormalTexture(topLeftFront, -Vector3.UnitX, uvTopRight);
            vertices[20] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitX, uvBottomRight);

            vertices[21] = new VertexPositionNormalTexture(bottomLeftBack, -Vector3.UnitX, uvBottomLeft);
            vertices[22] = new VertexPositionNormalTexture(topLeftBack, -Vector3.UnitX, uvTopLeft);
            vertices[23] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitX, uvBottomRight);

            //right
            vertices[24] = new VertexPositionNormalTexture(bottomRightFront, Vector3.UnitX, uvBottomLeft);
            vertices[25] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitX, uvTopLeft);
            vertices[26] = new VertexPositionNormalTexture(bottomRightBack, Vector3.UnitX, uvBottomRight);

            vertices[27] = new VertexPositionNormalTexture(topRightFront, Vector3.UnitX, uvTopLeft);
            vertices[28] = new VertexPositionNormalTexture(topRightBack, Vector3.UnitX, uvTopRight);
            vertices[29] = new VertexPositionNormalTexture(bottomRightBack, Vector3.UnitX, uvBottomRight);

            //bottom
            vertices[30] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitY, uvTopLeft);
            vertices[31] = new VertexPositionNormalTexture(bottomRightFront, -Vector3.UnitY, uvTopRight);
            vertices[32] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitY, uvBottomRight);

            vertices[33] = new VertexPositionNormalTexture(bottomLeftFront, -Vector3.UnitY, uvTopLeft);
            vertices[34] = new VertexPositionNormalTexture(bottomRightBack, -Vector3.UnitY, uvBottomRight);
            vertices[35] = new VertexPositionNormalTexture(bottomLeftBack, -Vector3.UnitY, uvBottomLeft);

            return vertices;
        }
    }
}