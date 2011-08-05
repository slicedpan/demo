using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.Constraints.TwoEntity.Joints;
using BEPUphysics.Constraints.TwoEntity.Motors;
using bepuData;
using Microsoft.Xna.Framework.Graphics;

namespace phystest
{
    public class CeilingLamp : Actor
    {

        int numSegments = 10;
        float linklength = 0.1f;
        Color lightColor = new Color(231, 239, 216);
        Vector3 scalevec;
        Vector3 Position;
        Model lampModel;
        PhysicsInfo lampPhys;
        Model segmentModel;
        PhysicsInfo segmentPhys;

        public CeilingLamp(Vector3 position, Object scale)
        {
            Position = position;
            scalevec = GetScaleVector(scale);
        }

        public override void InitializeComponents()
        {
            Component shade;
            Light shadeLight;            

            var toplink = new Component(Position, Quaternion.Identity, scalevec / 32.0f);
            toplink.Mesh = new LPPMesh(segmentModel);
            toplink.SetCollision(segmentPhys, true);

            Components.Add(toplink);

            Vector3 yVec = new Vector3(0.0f, scalevec.Y, 0.0f);

            var lastlink = toplink;

            for (int i = 0; i < numSegments; i++)
            {
                var link = new Component(Position - (yVec * (float)i * linklength), Quaternion.Identity, scalevec / 32.0f);
                link.Mesh = new LPPMesh(segmentModel);
                link.SetCollision(segmentPhys, false);
                CollisionRules.AddRule(lastlink.Entity, link.Entity, CollisionRule.NoBroadPhase);
                var joint = new DistanceJoint(lastlink.Entity, link.Entity, link.Position, link.Position);

                joint.SpringSettings.Advanced.UseAdvancedSettings = true;
                joint.SpringSettings.Advanced.ErrorReductionFactor = 0.99f;
                joint.SpringSettings.Advanced.Softness = 0.1f;

                Components.Add(link);
                Components.Add(new DummyComponent(joint));
                lastlink = link;
            }

            shade = new Component(Position - (yVec * ((numSegments * linklength) + 0.25f)), Quaternion.Identity, scalevec / 32.0f);
            shade.Mesh = new LPPMesh(lampModel);

            shadeLight = new Light();
            shadeLight.LocalTransform = Matrix.CreateTranslation(yVec * -0.3f);
            shadeLight.Shadow = true;
            shadeLight.LightType = "BakedShadow";
            shadeLight.Radius = 12.5f;
            shadeLight.Intensity = 0.75f;
            shadeLight.Color = lightColor;
            PhysicsInfo PI = new PhysicsInfo(lampPhys);
            PI.Density /= scalevec.Length() / Vector3.One.Length();
            shade.SetCollision(PI, false);
            shade.Lights.Add(shadeLight);
            shade.Entity.Tag = new EntityTag(this, shade);

            shadeLight = new Light();
            shadeLight.Color = lightColor;
            shadeLight.LocalTransform = Matrix.CreateTranslation(yVec);
            shadeLight.Radius = 11.9f;
            shadeLight.Intensity = 0.75f;
            shadeLight.Shadow = false;
            shade.Lights.Add(shadeLight);

            shadeLight = new Light();
            shadeLight.Intensity = 1.0f;
            shadeLight.Radius = 0.5f;
            shade.Lights.Add(shadeLight);
            Components.Add(shade);

            CollisionRules.AddRule(shade.Entity, lastlink.Entity, CollisionRule.NoBroadPhase);
            var joint1 = new DistanceJoint(lastlink.Entity, shade.Entity, shade.Position, lastlink.Position);
            joint1.SpringSettings.DampingConstant = 1000000f;
            joint1.SpringSettings.StiffnessConstant = 600000f;
            var angularMotor1 = new AngularMotor(lastlink.Entity, shade.Entity);
            angularMotor1.Settings.MaximumForce = 50f;
            Components.Add(new DummyComponent(joint1));
            Components.Add(new DummyComponent(angularMotor1));
            base.InitializeComponents();
        }

        public override void Update(GameTime gameTime)
        {
            for (int n = 0; n < numSegments; n++)
            {
                var link = Components[(n * 2) + 1];
                var joint = (Components[(n * 2) + 2] as DummyComponent).spaceObject as DistanceJoint;
                float lstretch = 1.1f - (joint.Error * 4f);
                link.Mesh.Premult = Matrix.CreateScale(new Vector3(1.0f, lstretch, 1.0f));
                Game1.stretch = lstretch;
            }
            base.Update(gameTime);
        }

        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            lampModel = Content.Load<Model>("lamp/lamp");
            lampPhys = Content.Load<PhysicsInfo>("lamp/lampphys");
            segmentModel = Content.Load<Model>("lamp/segment");
            segmentPhys = Content.Load<PhysicsInfo>("lamp/segmentphys");
        }

    }
}
