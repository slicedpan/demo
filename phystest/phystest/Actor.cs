using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace phystest
{
    public class Actor
    {
        public List<string> messages = new List<string>();
        protected string _name = "noname";
        public string Name
        {
            get { return _name; }
        }
        public virtual void Use()
        {

        }
        public virtual void HandleMessage(string message)
        {

        }
        public List<Component> Components;        
        public Actor()
        {
            Components = new List<Component>();            
        }
        public Actor(Component component)
        {
            Components = new List<Component>();
            Components.Add(component);
            if (component.Entity != null)
                component.Entity.Tag = new EntityTag(this, component);            
        }
        public Actor(Component component, string name)
        {
            Components = new List<Component>();
            Components.Add(component);
            if (component.Entity != null)
                component.Entity.Tag = new EntityTag(this, component);
            _name = name;
        }
        public virtual void Update(GameTime gameTime)
        {
            foreach (Component component in Components)
            {
                component.Update(gameTime);                
            }
        }
        public virtual void LoadContent(ContentManager Content)
        {

        }
        public virtual void InitializeComponents()
        {

        }
        protected Vector3 GetScaleVector(Object scale)
        {
            if (scale is Vector3)
            {
                return(Vector3)scale;
            }
            else if (scale is float || scale is double || scale is int)
            {
                float f = (float)scale;
                return new Vector3(f, f, f);
            }
            else
            {
                throw new ArgumentException("Scale argument must be a Vector3 or a number");
            }
        }
    }
}
