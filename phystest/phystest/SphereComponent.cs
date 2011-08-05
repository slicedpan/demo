using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.EntityStateManagement;

namespace phystest
{
    public class SphereComponent : Component
    {
        float radius;
        float mass;
        public SphereComponent(Vector3 p_position, float p_radius, float p_mass) :
            base(new LPPMesh(Game1.lppsphere), p_position, Quaternion.Identity, Vector3.One)
        {
            Position = p_position;
            radius = p_radius;
            initscale = Vector3.One * radius;
            mass = p_mass;
            SetCollision();
        }
        public void SetCollision()
        {
            MotionState motionState = new MotionState();
            motionState.Position = Position;
            motionState.Orientation = Quaternion.Identity;
            Entity = new Sphere(motionState, radius, mass);
        }
        public static void Spawn(Vector3 position, Vector3 velocity)
        {
            float mass = 0.1f;
            SphereComponent sc = new SphereComponent(position, 1.0f, mass);
            sc.Entity.ApplyImpulse(position, (velocity * mass) / 2.0f);
            Game1.Actors.Add(new Actor(sc));
        }
    }
}
