using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace phystest
{
    public class Light
    {
        private Matrix _transform = Matrix.Identity;
        private float _radius = 1.0f;
        private Color _color = Color.White;
        private BoundingSphere _sphere;
        private float _intensity = 1.0f;
        public Matrix LocalTransform = Matrix.Identity;
        public bool Shadow = false;
        public Matrix LightView, LightViewProj;
        public string LightType;
        public Texture2D shadowMap;

        #region accessors

        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = value;
                _sphere.Radius = _radius;
            }
        }

        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }

        public Matrix Transform
        {
            get { return _transform; }
            set
            {
                _transform = value;
                _sphere.Center = _transform.Translation;
            }
        }

        public float Intensity
        {
            get { return _intensity; }
            set { _intensity = value; }
        }


        public BoundingSphere BoundingSphere
        {
            get { return _sphere; }
        }

        #endregion

        public Light()
        {
            _sphere = new BoundingSphere(Vector3.Zero, 1.0f);           
        }
        public static Light Dummy()
        {
            Light retlight = new Light();
            retlight.Radius = 0.0001f;
            retlight.Intensity = 0.0001f;
            retlight.Shadow = false;
            retlight.LightType = "dummy";
            return retlight;
        }
    }
}
