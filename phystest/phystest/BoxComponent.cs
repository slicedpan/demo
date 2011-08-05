using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.EntityStateManagement;

namespace phystest
{
    class BoxComponent : Component
    {
        Vector3 selfcolor;
        Vector3[] vertcolors;
        float mass;
        public BoxComponent(Vector3 p_position, Vector3 p_scale, float p_mass) :
            base(new LPPMesh(Game1.lppbox), p_position, Quaternion.Identity, p_scale)
        {
            initscale = p_scale;
            mass = p_mass;
            selfcolor = new Vector3((float)Game1.rand.NextDouble(), (float)Game1.rand.NextDouble(), (float)Game1.rand.NextDouble());
            vertcolors = new Vector3[8];
            for (int i = 0; i < 8; i++)
            {
                vertcolors[i].X = (float)Game1.rand.NextDouble() * 0.1f;
                vertcolors[i].Y = (float)Game1.rand.NextDouble() * 0.1f;
                vertcolors[i].Z = (float)Game1.rand.NextDouble() * 0.1f;
            }
            SetCollision();
        }

        public void SetCollision()
        {
            var motionState = new MotionState();
            motionState.Position = Position;
            motionState.Orientation = Quaternion.Identity;
            Entity = new Box(motionState, initscale.X, initscale.Y, initscale.Z, mass);
        }
        public static void Spawn(Vector3 position, Vector3 velocity)
        {
            BoxComponent bc = new BoxComponent(position, Vector3.One, 1.0f);
            bc.Entity.ApplyImpulse(position, velocity);
            Game1.Actors.Add(new Actor(bc));
        }     
    }
}
