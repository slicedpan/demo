using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace phystest
{
    public class Mesh
    {
        protected Model _model;
        public Matrix Transform;
        public Matrix Premult = Matrix.Identity;
        protected BoundingSphere _sphere;
        public bool Occlude = true;
        public bool Draw = true;
        public bool Highlight = false;
        public virtual BoundingSphere BoundingSphere
        {
            get { return Helpers.TransformSphere(_sphere, Transform); }                
        }
        public Model Model
        {
            get { return _model; }
            set 
            {
                _model = value;
                GenerateBoundingSphere();
            }
        }
        public Mesh()
        {
            Transform = Matrix.Identity;
            _sphere = new BoundingSphere(Vector3.Zero, 0);
        }
        public Mesh(Model model)
        {
            _sphere = new BoundingSphere();
            _model = model;            
            Transform = Matrix.Identity;
        }
        protected void GenerateBoundingSphere()
        {
            List<BoundingSphere> spheres = new List<BoundingSphere>();
            foreach (ModelMesh mesh in _model.Meshes)
            {
                spheres.Add(mesh.BoundingSphere);
            }
            _sphere = Helpers.GetContainingSphere(spheres);
        }
        public void DrawSphere(SpriteBatch spriteBatch, Camera camera)
        {

        }
    }
}
