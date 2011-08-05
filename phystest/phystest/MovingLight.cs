using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace phystest
{
    public class MovingLight : Component
    {
        Vector3 _centre;
        float _radius;
        public MovingLight(Vector3 centrePoint, float radius, float intensity = 1.0f) :
            base(Vector3.Zero, Quaternion.Identity, Vector3.One)
        {
            Light light = new Light();
            light.Radius = 15.0f;
            light.Intensity = intensity;
            _centre = centrePoint;
            _radius = radius;
            Lights.Add(light);
        }
        public MovingLight(Vector3 centrePoint, float radius, Color color, float intensity = 1.0f) :
            base(Vector3.Zero, Quaternion.Identity, Vector3.One)
        {
            Light light = new Light();
            light.Radius = 25.0f;
            light.Intensity = intensity;
            light.Color = color;
            light.Shadow = true;
            _centre = centrePoint;
            _radius = radius;
            Lights.Add(light);
        }
        public override void Update(GameTime gameTime)
        {
            //Position = _centre + Vector3.UnitZ * _radius;
            Position = _centre + ((float)Math.Sin(gameTime.TotalGameTime.TotalSeconds / 10.0f) * Vector3.UnitZ * _radius) + ((float)Math.Cos(gameTime.TotalGameTime.TotalSeconds / 10.0f) * Vector3.UnitX * _radius);
            base.Update(gameTime);
        }
    }
}
