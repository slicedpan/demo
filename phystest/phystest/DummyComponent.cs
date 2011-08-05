using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics.Entities;
using BEPUphysics;

namespace phystest
{
    public class DummyComponent : Component
    {
        public ISpaceObject spaceObject;
        public DummyComponent(ISpaceObject ISO)
        {
            spaceObject = ISO;
        }
        public override void CleanUp()
        {
            Game1.space.Remove(spaceObject);
        }   
    }
}
