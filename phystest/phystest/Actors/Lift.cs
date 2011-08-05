using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using bepuData;

namespace phystest
{
    public class Lift : Actor
    {
        Model botModel, midModel, topModel;
        Vector3 position;
        Vector3 scalevec;
        float state;
        float liftVelocity;
        PhysicsInfo topPI, midPI, botPI;
        Vector3 topPos, midPos, botPos;
        Texture2D liftTexture, liftNormal;
        EffectMaterial liftEffect;

        public Lift(Vector3 Position, Object scale)
        {
            position = Position;
            scalevec = GetScaleVector(scale);
            Console.AddFloat("liftMinHeight", 3.5f);
            Console.AddFloat("liftMaxVelocity", 0.0001f);
            state = 1.0f;
            liftVelocity = -0.0001f;
            _name = "lift";
        }
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            botModel = Content.Load<Model>("lift/liftbot");
            midModel = Content.Load<Model>("lift/liftmid");
            topModel = Content.Load<Model>("lift/lifttop");
            topPI = Content.Load<PhysicsInfo>("lift/lifttopphys");
            midPI = Content.Load<PhysicsInfo>("lift/liftmidphys");
            botPI = Content.Load<PhysicsInfo>("lift/liftbotphys");
            liftTexture = Content.Load<Texture2D>("lift/SupportliftDoor_Texture");
            liftNormal = Content.Load<Texture2D>("lift/SupportliftDoor_Normal");
            liftEffect = new EffectMaterial(Game1.NormalEffect);            
            base.LoadContent(Content);
        }
        public override void InitializeComponents()
        {
            var bot = new Component(new LPPMesh(botModel), position, Quaternion.Identity, scalevec);
            bot.SetCollision(botPI, true);
            Components.Add(bot); // Component 0 is the bottom
            var mid = new Component(new LPPMesh(midModel), position, Quaternion.Identity, scalevec);
            mid.SetCollision(midPI, true);
            Components.Add(mid); // 1 is the mid section
            var top = new Component(new LPPMesh(topModel), position, Quaternion.Identity, scalevec);
            top.SetCollision(topPI, true);
            Components.Add(top); // and 2 is the top

            foreach (Component component in Components)
            {
                Helpers.RemapEffects(component.Mesh.Model, liftEffect);
                liftEffect.Parameters["DiffuseMap"].SetValue(liftTexture);
                liftEffect.Parameters["NormalMap"].SetValue(liftNormal);
            }

            topPos = top.Entity.Position;
            midPos = mid.Entity.Position;
            botPos = bot.Entity.Position;

            base.InitializeComponents();
        }
        public override void Update(GameTime gameTime)
        {
            if (state > 1.0f)
                state = 1.0f;
            if (state < 0.0f)
                state = 0.0f;

            float minHeight = Console.GetFloat("liftMinHeight");

            float offset = (1.0f - state) * minHeight;

            var bot = Components[0];
            var mid = Components[1];
            var top = Components[2];

            bot.Entity.Position = botPos - (offset * 0.3333f * Vector3.UnitY);
            mid.Entity.Position = midPos - (offset * 0.6666f * Vector3.UnitY);
            top.Entity.Position = topPos - (offset * Vector3.UnitY);

            state += liftVelocity;

            base.Update(gameTime);
        }
        public override void HandleMessage(string message)
        {
            if (message == "up")
                LiftUp();
            else if (message == "down")
                LiftDown();
        }
        public void LiftUp()
        {
            liftVelocity = Console.GetFloat("liftMaxVelocity");
        }
        public void LiftDown()
        {
            liftVelocity = -Console.GetFloat("liftMaxVelocity");
        }
        public void LiftStop()
        {
            liftVelocity = 0.0f;
        }
    }
}
