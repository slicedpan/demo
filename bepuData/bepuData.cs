using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using BEPUphysics.Entities;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.EntityStateManagement;
using BEPUphysics.Materials;
using BEPUphysics.CollisionShapes;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.MathExtensions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace bepuData
{
    public interface PrimData
    {
        Entity GetEntity(Vector3 scale, float density, Material material, out Matrix preTransform);
        CompoundShapeEntry GetShape(Vector3 scale, float density, Material material);
        PrimData Copy();
    }
    [Serializable]
    public class BoxData : PrimData
    {
        [ContentSerializer]
        Vector3 origin;
        [ContentSerializer]
        Vector3 sidelengths;
        [ContentSerializer]
        Quaternion rotation;
        
        public BoxData(Vector3 p_origin, Quaternion p_rotation, Vector3 p_sidelengths)
        {
            origin = p_origin;
            rotation = p_rotation;
            sidelengths = p_sidelengths;
        }
        private BoxData()
        {

        }
        public Entity GetEntity(Vector3 scale, float density, Material material, out Matrix preTransform)
        {
            Matrix scalemat = Matrix.CreateScale(scale);
            Vector3 transformedorigin = Vector3.Transform(origin, scalemat);
            Vector3 transformedsides = Vector3.Transform(sidelengths, scalemat);
            MotionState motionState = new MotionState();
            motionState.Orientation = rotation;
            rotation.Conjugate();            
            motionState.Position = transformedorigin;
            motionState.AngularVelocity = Vector3.Zero;
            motionState.LinearVelocity = Vector3.Zero;
            preTransform = Matrix.Invert(motionState.WorldTransform);
            float mass = density * (transformedsides.X * transformedsides.Y * transformedsides.Z);
            var box = new Box(motionState, transformedsides.X, transformedsides.Y, transformedsides.Z, mass);            
            box.Material = material;
            return box;
        }
        public CompoundShapeEntry GetShape(Vector3 scale, float density, Material material)
        {
            Matrix scalemat = Matrix.CreateScale(scale);
            Vector3 transformedorigin = Vector3.Transform(origin, scalemat);
            Vector3 transformedsides = Vector3.Transform(sidelengths, scalemat);
            float mass = density * (transformedsides.X * transformedsides.Y * transformedsides.Z);
            RigidTransform transform = new RigidTransform(transformedorigin, rotation);            
            var boxShape = new CompoundShapeEntry(new BoxShape(transformedsides.X, transformedsides.Y, transformedsides.Z), transform, mass);
            return boxShape;
        }
        public PrimData Copy()
        {
            return new BoxData(origin, rotation, sidelengths);
        }
    }
    [Serializable]
    public class SphereData : PrimData
    {
        float sphereConst = 4.1887902f;
        [ContentSerializer]
        Vector3 centre;
        [ContentSerializer]
        float radius;
        public SphereData(Vector3 p_centre, float p_radius)
        {
            centre = p_centre;
            radius = p_radius;
        }
        private SphereData()
        {

        }
        public Entity GetEntity(Vector3 scale, float density, Material material, out Matrix preTransform)
        {
            preTransform = Matrix.Identity;
            Matrix scalemat = Matrix.CreateScale(scale);
            Vector3 transcentre = Vector3.Transform(centre, scalemat);
            float transradius = radius * scale.Length() / Vector3.One.Length();            
            float mass = density * sphereConst * (radius * radius * radius);
            var sphere = new Sphere(transcentre, transradius, 1.0f);
            sphere.Material = material;
            return sphere;
        }
        public CompoundShapeEntry GetShape(Vector3 scale, float density, Material material)
        {
            Matrix scalemat = Matrix.CreateScale(scale);
            Vector3 transcentre = Vector3.Transform(centre, scalemat);
            float transradius = radius * scale.Length() / Vector3.One.Length();
            float mass = density * sphereConst * (radius * radius * radius);
            var transform = new RigidTransform(transcentre);
            var sphereShape = new CompoundShapeEntry(new SphereShape(transradius), transform, mass);
            return sphereShape;
        }
        public PrimData Copy()
        {
            return new SphereData(centre, radius);
        }
    }
    [Serializable]
    public class CapsuleData : PrimData
    {
        [ContentSerializer]
        Vector3 position;
        [ContentSerializer]
        Quaternion rotation;
        [ContentSerializer]
        float radius;
        [ContentSerializer]
        float length;

        public CapsuleData(Vector3 p_position, Quaternion p_rotation, float p_radius, float p_length)
        {
            position = p_position;
            rotation = p_rotation;
            radius = p_radius;
            length = p_length;
        }
        private CapsuleData()
        {
        }
        public Entity GetEntity(Vector3 scale, float density, Material material, out Matrix preTransform)
        {
            preTransform = Matrix.Identity;
            Vector3 transpos = Vector3.Transform(position, Matrix.CreateScale(scale));
            float linearscale = scale.Length() / Vector3.One.Length();
            MotionState motionState = new MotionState();
            motionState.Orientation = rotation;
            rotation.Conjugate();            
            motionState.Position = transpos;
            motionState.LinearVelocity = Vector3.Zero;
            motionState.AngularVelocity = Vector3.Zero; //TODO calculate capsules volume
            preTransform = Matrix.Invert(motionState.WorldTransform);
            var capsule = new Capsule(motionState, length * scale.Y, radius * ((scale.X + scale.Z) / 2), density);

            capsule.Material = material;
            return capsule;
        }
        public CompoundShapeEntry GetShape(Vector3 scale, float density, Material material)
        {
            Vector3 transpos = Vector3.Transform(position, Matrix.CreateScale(scale));
            float linearscale = scale.Length() / Vector3.One.Length();            
            var transform = new RigidTransform(transpos, rotation);
            var shape = new CompoundShapeEntry(new CapsuleShape(length * scale.Y, radius * ((scale.X + scale.Z) / 2)),transform, density);
            return shape;
        }
        public PrimData Copy()
        {
            return new CapsuleData(position, rotation, radius, length);
        }
    }
    [Serializable]
    public class CylinderData: PrimData
    {
        [ContentSerializer]
        Vector3 position;
        [ContentSerializer]
        Quaternion rotation;
        [ContentSerializer]
        float radius;
        [ContentSerializer]
        float length;

        public CylinderData(Vector3 p_position, Quaternion p_rotation, float p_radius, float p_length)
        {
            position = p_position;
            rotation = p_rotation;
            radius = p_radius;
            length = p_length;
        }
        private CylinderData()
        {

        }

        public Entity GetEntity(Vector3 scale, float density, Material material, out Matrix preTransform)
        {
            Vector3 transpos = Vector3.Transform(position, Matrix.CreateScale(scale));            
            float linearscale = scale.Length() / Vector3.One.Length();
            
            MotionState motionState = new MotionState();
            motionState.Orientation = rotation;
            rotation.Conjugate();
            //preTransform = Matrix.CreateTranslation(Vector3.UnitY * -(length / 2) * scale.Y) * Matrix.CreateFromQuaternion(rotation);
            motionState.Position = transpos;           
            motionState.LinearVelocity = Vector3.Zero;
            motionState.AngularVelocity = Vector3.Zero; //TODO calculate capsules volume
            preTransform = Matrix.Invert(motionState.WorldTransform);
            var capsule = new Cylinder(motionState, length * scale.Y, radius * ((scale.X + scale.Z) / 2), density);
            capsule.Material = material;
            return capsule;
        }

        public CompoundShapeEntry GetShape(Vector3 scale, float density, Material material)
        {
            Vector3 transpos = Vector3.Transform(position, Matrix.CreateScale(scale));
            float linearscale = scale.Length() / Vector3.One.Length();
            var transform = new RigidTransform(transpos, rotation);
            var shape = new CompoundShapeEntry(new CylinderShape(length * scale.Y, radius * ((scale.X + scale.Z) / 2)), transform, density);
            return shape;
        }

        public PrimData Copy()
        {
            return new CylinderData(position, rotation, radius, length);
        }
    }

    public class PhysicsInfo
    {
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
        [ContentSerializer]
        public List<PrimData> PrimList = new List<PrimData>();
        [ContentSerializer]
        public string mystr;
        [ContentSerializer]
        public string nodes = "";
        [ContentSerializer]
        public Material Material;
        [ContentSerializer]
        public float Density;

        public PhysicsInfo(float staticFriction, float kineticFriction, float bounciness)
        {
            Material = new Material(staticFriction, kineticFriction, bounciness);
            nodes = String.Format("sf: {0}, kf: {1}, b: {2}", staticFriction, kineticFriction, bounciness);
        }
        public PhysicsInfo(PhysicsInfo PI)
        {
            Density = PI.Density;
            nodes = PI.nodes;
            Material = PI.Material;
            mystr = PI.mystr;
            foreach (PrimData prim in PI.PrimList)
            {
                PrimList.Add(prim.Copy());
            }
        }
        private PhysicsInfo()
        {
        }
        public void CreateBox(Vector3 origin, Quaternion rotation, Vector3 sidelengths)
        {
            PrimList.Add(new BoxData(origin, rotation, sidelengths));
        }
        public void CreateSphere(Vector3 centre, float radius)
        {
            PrimList.Add(new SphereData(centre, radius));            
        }
        public void CreateCapsule(Vector3 position, Quaternion rotation, float radius, float length)
        {
            PrimList.Add(new CapsuleData(position, rotation, radius, length));
        }
        public void CreateCylinder(Vector3 position, Quaternion rotation, float radius, float length)
        {
            PrimList.Add(new CylinderData(position, rotation, radius, length));
        }
        public Entity GetEntity(Vector3 scale, out Matrix preTransform)
        {
            if (PrimList.Count == 1)
            {
                return PrimList[0].GetEntity(scale, Density, Material, out preTransform);                    
            }
            
            var Shapes = new List<CompoundShapeEntry>();
            float totalMass = 0.0f;
            foreach (PrimData prim in PrimList)
            {
                var shape = prim.GetShape(scale, Density, Material);
                totalMass += shape.Weight;
                Shapes.Add(shape);                
            }

            var body = new CompoundBody(Shapes, totalMass);
            preTransform = Matrix.Invert(body.WorldTransform);
            //body.WorldTransform = Matrix.Identity;
            return body;
        }
    }
}
