using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace phystest
{
    public class SphereDrawer
    {
        private Game1 Game;
        public SphereDrawer(Game1 game)
        {
            Game = game;
        }
        public void Draw(Matrix View, Matrix Projection)
        {
            BasicEffect effect = Game1.spheremodel.Meshes[0].Effects[0] as BasicEffect;
            foreach (LPPMesh mesh in Game1.Actors.LPPMeshes)
            {
                foreach (ModelMesh mm in mesh.Model.Meshes)
                {                    
                    effect.World = Matrix.CreateScale(mm.BoundingSphere.Radius) * Matrix.CreateTranslation(mm.BoundingSphere.Center);
                    effect.View = View;
                    effect.Projection = Projection;
                    Game1.spheremodel.Meshes[0].Draw();
                }
            }
        }
    }
}
