using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.Entities;
using BEPUphysics.Collidables;
using BEPUphysics.MathExtensions;
using BEPUphysics.DataStructures;
using BEPUphysics;

namespace phystest
{
    public class TriangleMeshComponent : Component
    {
        private Model _model;
        private StaticMesh _physmesh;
        public StaticMesh StaticMesh
        {
            get
            {
                return _physmesh;
            }
        }
        public TriangleMeshComponent(Model model, Vector3 position, Vector3 scale) :
            base(new LPPMesh(model), position, Quaternion.Identity, scale)
        {
            _model = model;
            SetCollision();
            
        }
        public override void CleanUp()
        {
            Game1.space.Remove(_physmesh);
            Game1.thegame.ModelDrawer.Remove(_physmesh.Mesh);
        }
        public void SetCollision()
        {
            AffineTransform transform = new AffineTransform(initscale, initorient, Position);
            Vector3[] staticTriangleVertices;
            int[] staticTriangleIndices;
            TriangleMesh.GetVerticesAndIndicesFromModel(_model, out staticTriangleVertices, out staticTriangleIndices);
            _physmesh = new StaticMesh(staticTriangleVertices, staticTriangleIndices, transform);
            Game1.space.Add(_physmesh);
            Game1.thegame.ModelDrawer.Add(_physmesh.Mesh);
        }
        /* meshfunctions (old)
        #region meshfunctions
        public static void ExtractData(List<Vector3> vertices, List<TriangleVertexIndices> indices, Model model, Vector3 scale)
        {
            Matrix[] bones_ = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bones_);
            foreach (ModelMesh mm in model.Meshes)
            {
                int offset = vertices.Count;
                Matrix xform = bones_[mm.ParentBone.Index] * Matrix.CreateScale(scale);
                foreach (ModelMeshPart mmp in mm.MeshParts)
                {
                    Vector3[] a = new Vector3[mmp.NumVertices];
                    int stride = mmp.VertexBuffer.VertexDeclaration.VertexStride;
                    //mm.VertexBuffer.GetData<Vector3>(mmp.StreamOffset + mmp.BaseVertex * mmp.VertexStride, a, 0, mmp.NumVertices, mmp.VertexStride);  //XNA 4.0 change
                    mmp.VertexBuffer.GetData(mmp.VertexOffset * stride, a, 0, mmp.NumVertices, stride);

                    for (int i = 0; i != a.Length; ++i)
                        Vector3.Transform(ref a[i], ref xform, out a[i]);
                    vertices.AddRange(a);

                    //if (mm.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)      //XNA 4.0 change
                    if (mmp.IndexBuffer.IndexElementSize != IndexElementSize.SixteenBits)
                        throw new Exception(String.Format("Model uses 32-bit indices, which are not supported."));

                    short[] s = new short[mmp.PrimitiveCount * 3];
                    //mm.IndexBuffer.GetData<short>(mmp.StartIndex * 2, s, 0, mmp.PrimitiveCount * 3);      //XNA 4.0 change
                    mmp.IndexBuffer.GetData(mmp.StartIndex * 2, s, 0, mmp.PrimitiveCount * 3);

                    JigLibX.Geometry.TriangleVertexIndices[] tvi = new JigLibX.Geometry.TriangleVertexIndices[mmp.PrimitiveCount];
                    for (int i = 0; i != tvi.Length; ++i)
                    {
                        tvi[i].I0 = s[i * 3 + 2] + offset;
                        tvi[i].I1 = s[i * 3 + 1] + offset;
                        tvi[i].I2 = s[i * 3 + 0] + offset;
                    }
                    indices.AddRange(tvi);
                }
            }
        }
        public static TriangleMesh CreateMesh(Model model, Vector3 scale)
        {
            TriangleMesh retmesh = new TriangleMesh();
            List<Vector3> vertices = new List<Vector3>();
            List<TriangleVertexIndices> indices = new List<TriangleVertexIndices>();
            ExtractData(vertices, indices, model, scale);
            retmesh.CreateMesh(vertices, indices, 4, 1.0f);
            return retmesh;
        }
        #endregion
         */ 
    }
}
