using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace phystest
{
    public class FollowLight : Component
    {
        Camera _camera;
        public FollowLight(float radius, float intensity, Color color, Camera cam)
            : base(Vector3.Zero, Quaternion.Identity, Vector3.One)            
        {
            _camera = cam;
            Light light;
            light = new Light();
            light.Color = color;
            light.Radius = radius;
            light.Intensity = intensity;
            light.LocalTransform = Matrix.Identity;
            light.Shadow = false;
            Lights.Add(light);
        }
        public override void Update(GameTime gameTime)
        {
            Position = _camera.Transform.Translation;
            base.Update(gameTime);
        }
    }
}
