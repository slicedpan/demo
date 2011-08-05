using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace phystest
{
    public class PulsingLight : Actor
    {
        Light _light;
        double _period;
        float _halfmax;
        float _minintensity;
        public PulsingLight(Vector3 position, Color color, float radius, float maxintensity, float minintensity, double period)
        {
            Components.Add(new Component(position, Quaternion.Identity, Vector3.One));
            _period = period;
            _light = new Light();
            _light.LightType = "BakedShadow";
            _light.Color = color;
            _light.Radius = radius;
            _light.Intensity = maxintensity;
            _light.LocalTransform = Matrix.Identity;
            Components[0].Lights.Add(_light);
            _minintensity = minintensity;
            _halfmax = (maxintensity - minintensity) / 2;
        }
        public override void Update(GameTime gameTime)
        {
            _light.Intensity = _halfmax + ((float)Math.Sin((gameTime.TotalGameTime.TotalSeconds / _period) * MathHelper.TwoPi) * _halfmax) + _minintensity;
            base.Update(gameTime);
        }
    }
}
