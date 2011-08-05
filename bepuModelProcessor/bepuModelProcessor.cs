using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using bepuData;
using System.ComponentModel;


namespace physicspipeline
{
    /// <summary>
    /// This class will be instantiated by the XNA Framework Content Pipeline
    /// to apply custom processing to content data, converting an object of
    /// type TInput to TOutput. The input and output types may be the same if
    /// the processor wishes to alter data without changing its type.
    ///
    /// This should be part of a Content Pipeline Extension Library project.
    ///
    /// TODO: change the ContentProcessor attribute to specify the correct
    /// display name for this processor.
    /// </summary>
    [ContentProcessor(DisplayName = "BEPU Physics Model Processor")]
    public class ContentProcessor1 : ContentProcessor<NodeContent, PhysicsInfo>
    {
        PhysicsInfo retData;

        private float _density = 1.0f;
        private float _bounciness = 1.0f;
        private float _dynamicFriction = 1.0f;
        private float _staticFriction = 1.0f;

        [DefaultValue(1f)]
        [DisplayName("Density")]
        [Description("The density of the object, used when calculating mass properties.")]
        public float DefaultDensity { get { return _density; } set { _density = value; } }

        [DefaultValue(1f)]
        [DisplayName("Dynamic Friction")]
        [Description("The dynamic friction of the object, refers to how difficult it is to move once it has started moving")]
        public float DynamicFriction { get { return _dynamicFriction; } set { _dynamicFriction = value; } }

        [DefaultValue(1f)]
        [DisplayName("Static Friction")]
        [Description("The static friction of this object, refers to how difficult it is to move the object from stationary")]
        public float StaticFriction { get { return _staticFriction; } set { _staticFriction= value; } }

        [DefaultValue(1f)]
        [DisplayName("Bounciness")]
        [Description("The bounciness of the object, measures how much energy it retains after a collision")]
        public float Bounciness { get { return _bounciness; } set { _bounciness = value;} }

        public override PhysicsInfo Process(NodeContent input, ContentProcessorContext context)
        {
            retData = new PhysicsInfo(StaticFriction, DynamicFriction, Bounciness);
            retData.Density = _density;
            ProcessNode(input);
            return retData;
        }
        private void PreRotate(ref Vector3 point)
        {

        }
        private void ProcessNode(NodeContent node)
        {
            MeshContent mesh = node as MeshContent;
            retData.nodes += 1;
            if (mesh != null)
            {
                string primtype = mesh.OpaqueData.GetValue<string>("UDP3DSMAX", "nostring");
                if (primtype.ToUpper() == "BOX")
                {
                    Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
                    Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

                    Quaternion rot;
                    Vector3 pos, scale, translation;

                    mesh.AbsoluteTransform.Decompose(out scale, out rot, out translation);

                    var rotMat = Matrix.CreateFromQuaternion(rot);
                    rotMat = Matrix.CreateRotationX(MathHelper.ToRadians(90.0f)) * rotMat;
                    Vector3 junk1, junk2;
                    rotMat.Decompose(out junk1, out rot, out junk2);

                    foreach (Vector3 vec in mesh.Positions)
                    {
                        pos = Vector3.Transform(vec, Matrix.CreateScale(scale));
                        if (pos.X < min.X)
                            min.X = pos.X;
                        if (pos.Y < min.Y)
                            min.Y = pos.Y;
                        if (pos.Z < min.Z)
                            min.Z = pos.Z;

                        if (pos.X > max.X)
                            max.X = pos.X;
                        if (pos.Y > max.Y)
                            max.Y = pos.Y;
                        if (pos.Z > max.Z)
                            max.Z = pos.Z;
                    }                                        
                    
                    Vector3 width = new Vector3(max.X - min.X, max.Z - min.Z, max.Y - min.Y);
                    Vector3 position = new Vector3(translation.X, translation.Y, translation.Z);
                    Vector3 avg = new Vector3(max.X + min.X, max.Z + min.Z, max.Y + min.Y);
                    //position += avg / 2.0f;
                    retData.nodes = width.ToString() + " " + position.ToString() + " " + avg.ToString();
                    retData.CreateBox(position, rot, width);
                }
                else if (primtype.ToUpper() == "BOX2")
                {
                    Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                    Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
                    Vector3 width;
                    Vector3 scale, translation;
                    Quaternion rotation;
                    Vector3 centre = Vector3.Zero;
                    int count = 0;

                    mesh.AbsoluteTransform.Decompose(out scale, out rotation, out translation);                    

                    foreach (Vector3 pos in mesh.Positions)
                    {                        
                        centre += pos;
                        ++count;
                        if (pos.X < min.X)
                            min.X = pos.X;
                        if (pos.Y < min.Y)
                            min.Y = pos.Y;
                        if (pos.Z < min.Z)
                            min.Z = pos.Z;

                        if (pos.X > max.X)
                            max.X = pos.X;
                        if (pos.Y > max.Y)
                            max.Y = pos.Y;
                        if (pos.Z > max.Z)
                            max.Z = pos.Z;
                    }

                    width = max - min;

                    centre /= count; //No. of points in the cube
                    translation += centre;

                    //Vector3.Transform(translation, Matrix.CreateScale(scale));
                    //Vector3.Transform(width, Matrix.CreateScale(scale));

                    retData.CreateBox(translation, rotation, width);

                }
                else if (primtype.ToUpper() == "BOXTEST")
                {
                    Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                    Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
                    Vector3 pos, width;
                    Vector3 scale, translation;
                    Quaternion rotation;
                    Matrix rotationmatrix;

                    mesh.AbsoluteTransform.Decompose(out scale, out rotation, out translation);
                    rotationmatrix = Matrix.CreateFromQuaternion(rotation);

                    foreach (Vector3 vec in mesh.Positions)
                    {
                        pos = Vector3.Transform(vec, Matrix.CreateScale(scale)); //bake in the scale 

                        if (pos.X < min.X)
                            min.X = pos.X;
                        if (pos.Y < min.Y)
                            min.Y = pos.Y;
                        if (pos.Z < min.Z)
                            min.Z = pos.Z;

                        if (pos.X > max.X)
                            max.X = pos.X;
                        if (pos.Y > max.Y)
                            max.Y = pos.Y;
                        if (pos.Z > max.Z)
                            max.Z = pos.Z;
                    }

                    width = max - min;  //this will be the length of the sides

                    Vector3 offs = width / 2;
                    offs.Z = 0;
                    Vector3 diff = Vector3.Transform(offs, rotationmatrix); //since the rotation will be about one of the corner points, we can subtract this to


                    retData.CreateBox(translation - diff, rotation, width);

                }
                else if (primtype.ToUpper() == "SPHERE")
                {
                    Vector3 centre = Vector3.Zero;
                    foreach (Vector3 vec in mesh.Positions)
                    {
                        centre += vec;
                    }

                    centre /= mesh.Positions.Count;

                    centre = Vector3.Transform(centre, mesh.AbsoluteTransform);

                    Vector3 radial = centre - Vector3.Transform(mesh.Positions[0], mesh.AbsoluteTransform);
                    Vector3 transform = new Vector3(centre.X, centre.Z, centre.Y);
                    retData.CreateSphere(centre, radial.Length());
                }
                else if (primtype.ToUpper() == "CAPSULE")
                {
                    Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
                    Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
                    Vector3 pos, scale, translation;
                    Quaternion rot;

                    mesh.AbsoluteTransform.Decompose(out scale, out rot, out translation);

                    var rotMat = Matrix.CreateFromQuaternion(rot);
                    rotMat = Matrix.CreateRotationX(MathHelper.ToRadians(90.0f)) * rotMat;
                    Vector3 junk1, junk2;
                    rotMat.Decompose(out junk1, out rot, out junk2);

                    foreach (Vector3 vec in mesh.Positions)
                    {
                        pos = Vector3.Transform(vec, Matrix.CreateScale(scale)); //bake in the scale 

                        if (pos.X < min.X)
                            min.X = pos.X;
                        if (pos.Y < min.Y)
                            min.Y = pos.Y;
                        if (pos.Z < min.Z)
                            min.Z = pos.Z;

                        if (pos.X > max.X)
                            max.X = pos.X;
                        if (pos.Y > max.Y)
                            max.Y = pos.Y;
                        if (pos.Z > max.Z)
                            max.Z = pos.Z;
                    }

                    float radius = (max.X - min.X) / 2;
                    float height = max.Z - min.Z - (radius * 2);
                    Vector3 pos2 = new Vector3(translation.X, translation.Y + (height / 2) + radius, translation.Z);
                    retData.CreateCapsule(pos2, rot, radius, height);
                }
                else if (primtype.ToUpper() == "CYLINDER")
                {
                    Vector3 max = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
                    Vector3 min = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);

                    float radius, height;
                    Quaternion rot;
                    Vector3 pos, scale, translation;

                    mesh.AbsoluteTransform.Decompose(out scale, out rot, out translation);

                    var rotMat = Matrix.CreateFromQuaternion(rot);
                    rotMat = Matrix.CreateRotationX(MathHelper.ToRadians(90.0f)) * rotMat;
                    Vector3 junk1, junk2;
                    rotMat.Decompose(out junk1, out rot, out junk2);
                    
                    foreach (Vector3 vec in mesh.Positions)
                    {
                        pos = Vector3.Transform(vec, Matrix.CreateScale(scale));
                        if (pos.X < min.X)
                            min.X = pos.X;
                        if (pos.Y < min.Y)
                            min.Y = pos.Y;
                        if (pos.Z < min.Z)
                            min.Z = pos.Z;

                        if (pos.X > max.X)
                            max.X = pos.X;
                        if (pos.Y > max.Y)
                            max.Y = pos.Y;
                        if (pos.Z > max.Z)
                            max.Z = pos.Z;
                    }
                    radius = (max.X - min.X) / 2.0f;
                    height = max.Z - min.Z;
                    Vector3 pos2 = new Vector3(translation.X, translation.Y + (height / 2), translation.Z);
                    retData.CreateCylinder(pos2, rot, radius, height);
                }
            }
            foreach (NodeContent child in node.Children)
            {
                ProcessNode(child);
            }
        }
    }
}