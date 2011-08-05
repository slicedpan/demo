using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace phystest
{
    class Helpers
    {
        static float[] cos;
        static float[] sin;
        static GraphicsDevice gd;
        static Random rand;
        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }
        static public List<BoundingSphere> GetBoundingSpheres(Model p_model, Matrix p_basetransform)
        {
            List<BoundingSphere> tmp = new List<BoundingSphere>();
            Matrix[] m = new Matrix[p_model.Bones.Count];
            p_model.CopyAbsoluteBoneTransformsTo(m);

            foreach (ModelMesh mesh in p_model.Meshes)
            {
                tmp.Add(Helpers.TransformSphere(mesh.BoundingSphere, m[mesh.ParentBone.Index]));
            }            
            return TransformSpheres(tmp, p_basetransform);
        }
        static public List<BoundingSphere> TransformSpheres(List<BoundingSphere> p_list, Matrix transform)
        {
            List<BoundingSphere> retspheres = new List<BoundingSphere>();
            foreach (BoundingSphere sphere in p_list)
            {
                retspheres.Add(TransformSphere(sphere, transform));
            }
            return retspheres;
        }
        static public BoundingSphere TransformSphere(BoundingSphere p_sphere, Matrix transform)
        {
            Vector3 centre = Vector3.Transform(p_sphere.Center, transform);
            Vector3 point = p_sphere.Center;
            point.X += p_sphere.Radius;
            point = Vector3.Transform(point, transform);
            point -= centre;
            return new BoundingSphere(centre, point.Length());
        }
        static public Vector3[] GetCirclePoints(BoundingSphere p_sphere)
        {
            Vector3[] retvec = new Vector3[16];
            for (int i = 0; i < 16; i++)
            {
                retvec[i].X = p_sphere.Center.X + (sin[i] * p_sphere.Radius);
                retvec[i].Z = p_sphere.Center.Z + (cos[i] * p_sphere.Radius);
                retvec[i].Y = p_sphere.Center.Y;
            }
            return retvec;
        }
        static Helpers()
        {
            cos = new float[16];
            sin = new float[16];
            for (int i = 0; i < 16; i++)
            {
                double PiOver8 = Math.PI / 8;
                cos[i] = (float)Math.Cos(PiOver8 * i);
                sin[i] = (float)Math.Sin(PiOver8 * i);
            }
        }
        public static void DrawCircle(Vector3[] points)
        {
            short[] indices = new short[points.Length + 1];
            VertexPositionColor[] vpc = new VertexPositionColor[points.Length];
            for (short i = 0; i < points.Length; i++)
            {
                indices[i] = i;
                vpc[i].Position = points[i];
                vpc[i].Color = Color.White;
            }
            indices[points.Length] = 0;
            
            
            gd.DrawUserIndexedPrimitives<VertexPositionColor>(PrimitiveType.LineStrip, vpc, 0, 16, indices, 0, 16);
        }
        static public void Init(GraphicsDevice p_gd) 
        { 
            gd = p_gd;
            rand = new Random();
        }
        static public Vector3 GetRandomVector()
        {
            return new Vector3((float)rand.NextDouble() - 0.5f, (float)rand.NextDouble() - 0.5f, (float)rand.NextDouble() - 0.5f);
        }
        static public int DebrisType()
        {
            return rand.Next(3) + 1;
        }
        public static BoundingSphere GetContainingSphere(List<BoundingSphere> collisionspheres)
        {
            if (collisionspheres.Count == 0) 
                return new BoundingSphere(Vector3.Zero, 0.0f);

            BoundingSphere original = new BoundingSphere(collisionspheres[0].Center,0);
            foreach (BoundingSphere sphere in collisionspheres)
            {
                original = BoundingSphere.CreateMerged(original, sphere);
            }
            return original;
        }
        public static void RemapEffects(Model model, EffectMaterial effect)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart mmp in mesh.MeshParts)
                {
                    mmp.Effect = effect;                    
                }
            }
        }
    }
}
