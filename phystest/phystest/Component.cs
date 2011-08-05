using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using bepuData;
using BEPUphysics.Entities;
using BEPUphysics.EntityStateManagement;

namespace phystest
{
    public class Component
    {

        #region fields

        public Vector3 Position;
        protected Vector3 initscale;
        protected Quaternion initorient;
        protected Vector3 com = Vector3.Zero;
        protected Matrix currentWorldTransform;
        protected Matrix bodyTransform;
        protected Mesh _mesh;
        public List<Light> Lights;
        Matrix preTransform;

        #endregion

        #region accessors
        public Mesh Mesh
        {
            get{ return _mesh; }
            set 
            { 
                _mesh = value;
                //_mesh.Transform = bodyTransform;
            }
        }

        #endregion

        #region renderers

        #endregion

        #region physfields

        public Entity Entity;

        #endregion

        #region constructors

        public Component(Mesh mesh, Vector3 position, Quaternion Orientation, Vector3 scale)
        {
            Position = position;
            initorient = Orientation;
            initscale = scale;
            Lights = new List<Light>();
            _mesh = mesh;
        }
        public Component(Vector3 position, Quaternion Orientation, Vector3 scale)
        {
            Position = position;
            initorient = Orientation;
            initscale = scale;
            Lights = new List<Light>();
        }
        protected Component()
        {
            Lights = new List<Light>();
        }

        #endregion

        public virtual void CleanUp()
        {

        }

        public virtual void SetCollision(PhysicsInfo PI, bool immovable)
        {            
            Entity = PI.GetEntity(initscale, out preTransform);
            Entity.Position += Position;
            if (immovable)
            {
                Entity.BecomeKinematic();
            }
        }
        private void CalculateWorldTransform()
        {
            if (Entity == null)
            {
                currentWorldTransform = Matrix.CreateFromQuaternion(initorient) * Matrix.CreateTranslation(Position);
            }
            else
            {
                currentWorldTransform = preTransform * Entity.WorldTransform;               
            }
        }        
        public virtual void Update(GameTime gameTime)
        {
            CalculateWorldTransform();
            if (_mesh != null)
            {
                _mesh.Transform = Matrix.CreateScale(initscale) * currentWorldTransform;
            }
            if (Lights.Count > 0)
            {
                foreach (Light light in Lights)
                {
                    light.Transform = light.LocalTransform * currentWorldTransform;
                }
            }
        }
    }
}
