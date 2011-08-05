using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace phystest
{
    public class EntityTag
    {
        public enum UseType
        {
            Grab,
            Use,
            None
        }
        public UseType useType;
        public Actor ParentActor
        {
            get;
            private set;
        }
        public Component ParentComponent
        {
            get;
            private set;
        }
        public EntityTag(Actor parentActor, Component parentComponent)
        {
            ParentActor = parentActor;
            ParentComponent = parentComponent;
            useType = UseType.Grab;
        }
    }
}
