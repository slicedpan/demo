using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using bepuData;

namespace phystest
{
    public class LiftButton : Actor
    {
        string _message;
        Vector3 _position, _scale;
        Model _buttonModel;
        PhysicsInfo PI;

        public LiftButton(Vector3 position, Vector3 scale, string message)
        {            
            _message = message;
            _position = position;
            _scale = scale;
        }
        public override void LoadContent(Microsoft.Xna.Framework.Content.ContentManager Content)
        {
            _buttonModel = Content.Load<Model>("room/buttonup");
            PI = Content.Load<PhysicsInfo>("room/buttonphys");
            base.LoadContent(Content);
        }
        public override void Use()
        {
            messages.Add(_message);
        }
        public override void InitializeComponents()
        {
            Components.Add(new Component(new LPPMesh(_buttonModel), _position, Quaternion.Identity, _scale));
            Components[0].SetCollision(PI, true);
            Components[0].Entity.Tag = new EntityTag(this, Components[0]);
            (Components[0].Entity.Tag as EntityTag).useType = EntityTag.UseType.Use;
        }
    }
}
