using System;
using System.Collections.Generic;
using JigLibX.Collision;
using JigLibX.Geometry;
using JigLibX.Math;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace GDLibrary
{
    /// <summary>
    ///     TriangleMeshObjects should be used sparingly. This is because the collision surface is generated
    ///     from the vertex data inside the original model. This makes collision detection relatively more
    ///     expensive than say a CollidableObject with primitive(s) added.
    ///     You should note that ALL triangle mesh objects are STATIC after they are placed.
    ///     That is, they cannot move after initial placement.
    /// </summary>
    public class TriangleMeshObject : CollidableObject
    {
        public TriangleMeshObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, Model model, MaterialProperties materialProperties)
            : this(id, actorType, transform, effectParameters, model, null, materialProperties)
        {
        }

        //allows us to specify a lowPoly collision skin model
        public TriangleMeshObject(string id, ActorType actorType, Transform3D transform,
            EffectParameters effectParameters, Model model, Model lowPolygonModel,
            MaterialProperties materialProperties)
            : base(id, actorType, transform, effectParameters, model)
        {
            //get the primitive mesh which forms the skin - use low poly if it has been provided in the constructor
            TriangleMesh triangleMesh = null;
            if (lowPolygonModel != null)
                triangleMesh = GetTriangleMesh(lowPolygonModel, Transform);
            else
                triangleMesh = GetTriangleMesh(model, Transform);

            //add the primitive mesh to the collision skin
            Body.CollisionSkin.AddPrimitive(triangleMesh, materialProperties);
        }

        public override Matrix GetWorldMatrix()
        {
            return Matrix.CreateScale(Transform.Scale) *
                   Body.CollisionSkin.GetPrimitiveLocal(0).Transform.Orientation *
                   Body.Orientation *
                   Transform.Orientation *
                   Matrix.CreateTranslation(Body.Position) *
                   Matrix.CreateTranslation(Transform.Translation);
        }

        private TriangleMesh GetTriangleMesh(Model model, Transform3D transform)
        {
            var triangleMesh = new TriangleMesh();
            var vertexList = new List<Vector3>();
            var indexList = new List<TriangleVertexIndices>();

            ExtractData(vertexList, indexList, model);

            for (var i = 0; i < vertexList.Count; i++)
                vertexList[i] = Vector3.Transform(vertexList[i], transform.World);

            // create the collision mesh
            triangleMesh.CreateMesh(vertexList, indexList, 1, 1.0f);

            return triangleMesh;
        }

        public override void Enable(bool bImmovable, float mass)
        {
            Mass = mass;

            //set whether the object can move
            Body.Immovable = true;

            //calculate the centre of mass
            var com = SetMass(mass);
            //set the centre of mass
            Body.CollisionSkin.ApplyLocalTransform(new Transform(-com, Matrix.Identity));
            //enable so that any applied forces (e.g. gravity) will affect the object
            Body.EnableBody();
        }

        public void ExtractData(List<Vector3> vertices, List<TriangleVertexIndices> indices, Model model)
        {
            var bones_ = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bones_);
            foreach (var mm in model.Meshes)
            {
                var offset = vertices.Count;
                var xform = bones_[mm.ParentBone.Index];
                foreach (var mmp in mm.MeshParts)
                {
                    var a = new Vector3[mmp.NumVertices];
                    var stride = mmp.VertexBuffer.VertexDeclaration.VertexStride;
                    mmp.VertexBuffer.GetData(mmp.VertexOffset * stride, a, 0, mmp.NumVertices, stride);

                    for (var i = 0; i != a.Length; ++i)
                        Vector3.Transform(ref a[i], ref xform, out a[i]);
                    vertices.AddRange(a);

                    if (mmp.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
                        throw new Exception("Model uses 32-bit indices, which are not supported.");

                    var s = new short[mmp.PrimitiveCount * 3];
                    mmp.IndexBuffer.GetData(mmp.StartIndex * 2, s, 0, mmp.PrimitiveCount * 3);

                    var tvi = new TriangleVertexIndices[mmp.PrimitiveCount];
                    for (var i = 0; i != tvi.Length; ++i)
                    {
                        tvi[i].I0 = s[i * 3 + 2] + offset;
                        tvi[i].I1 = s[i * 3 + 1] + offset;
                        tvi[i].I2 = s[i * 3 + 0] + offset;
                    }

                    indices.AddRange(tvi);
                }
            }
        }

        #region Variables

        #endregion

        #region Properties

        #endregion
    }
}