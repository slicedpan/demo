using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace phystest
{
    public class BareBulb : Actor
    {
        Light _light;
        Vector3 _position, _scale;
        Model _bulbModel;
        public BareBulb(Vector3 position, Vector3 scale, Color color, float radius, float intensity)
        {
            _scale = scale;
            _position = position;
            _light = new Light();
            _light.LightType = "BakedShadow";
            _light.Color = color;
            _light.Radius = radius;
            _light.Intensity = intensity;
            _light.LocalTransform = Matrix.CreateTranslation(-Vector3.UnitY * (100.0f / 64.0f));
        }
        public override void InitializeComponents()
        {
            Components.Add(new Component(new LPPMesh(_bulbModel), _position, Quaternion.Identity, _scale));
            Components[0].Lights.Add(_light);
            base.InitializeComponents();
        }
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            _bulbModel = Content.Load<Model>("bulb/bulb");
            base.LoadContent(Content);
        }
    }
}
