using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace phystest
{    
    public class LightDrawer
    {    
        private List<Light> Lights;
        private Game Game;
        public LightDrawer(Game game)
        {
            Lights = new List<Light>();
            Game = game;
        }
        public void Add(Light light)
        {
            Lights.Add(light);
        }
        public void Remove(Light light)
        {
            Lights.Remove(light);
        }
        public void Draw(Matrix view, Matrix projection)
        {
            BasicEffect effect = Game1.spheremodel.Meshes[0].Effects[0] as BasicEffect;
            foreach (Light light in Lights)
            {                
                effect.World = Matrix.CreateScale(0.1f) * Matrix.CreateTranslation(light.Transform.Translation);
                effect.View = view;
                effect.Projection = projection;
                effect.DiffuseColor = light.Color.ToVector3();
                effect.EnableDefaultLighting();
                effect.CurrentTechnique.Passes[0].Apply();
                Game1.spheremodel.Meshes[0].Draw();
            }
        }
    }
}

