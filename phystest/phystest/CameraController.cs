using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace phystest
{
    public class CameraController
    {
        private Camera _camera;
        private Vector3 position;
        private Vector2 orientation;
        public float Speed;
        public bool active;
        public CameraController(Camera camera, Vector3 initialPosition, Vector2 initialOrientation)
        {
            _camera = camera;
            Speed = 0.5f;
            orientation = initialOrientation;
            position = initialPosition;
        }
        public void HandleInput(KeyboardState ks, MouseState ms)
        {

            orientation.Y += (float)(ms.X - 200)  * - 0.001f;
            orientation.X += (float)(ms.Y - 200) * - 0.001f;

            Matrix yrot = Matrix.CreateRotationY(orientation.Y);
            Matrix rot = Matrix.CreateRotationX(orientation.X) * yrot;

            if (orientation.X > MathHelper.ToRadians(89))
                orientation.X = MathHelper.ToRadians(89);
            if (orientation.X < MathHelper.ToRadians(-89))
                orientation.X = MathHelper.ToRadians(-89);                

            if (active)
            {
                if (ks.IsKeyDown(Keys.W))
                {
                    position += rot.Forward * Speed;
                }
                if (ks.IsKeyDown(Keys.S))
                {
                    position -= rot.Forward * Speed;
                }
                if (ks.IsKeyDown(Keys.A))
                {
                    position += rot.Left * Speed;
                }
                if (ks.IsKeyDown(Keys.D))
                {
                    position -= rot.Left * Speed;
                }
                if (ks.IsKeyDown(Keys.Space))
                {
                    position.Y += Speed;
                }
                if (ks.IsKeyDown(Keys.LeftControl))
                {
                    position.Y -= Speed;
                }
            }
            
            _camera.Transform = rot * Matrix.CreateTranslation(position);       

            Mouse.SetPosition(200, 200);

        }
    }
}
