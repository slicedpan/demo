using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BEPUphysics.Entities.Prefabs;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.UpdateableSystems;
using BEPUphysics.Collidables.MobileCollidables;
using BEPUphysics.Entities;

namespace phystest
{
    public class Ladder : Actor
    {
        Box ladderBox;
        public Ladder(Vector3 position, float width, float height, float length)
        {
            Component component;
            ladderBox = new Box(position, width, height, length);
            ladderBox.CollisionInformation.CollisionRules.Personal = CollisionRule.NoNarrowPhaseUpdate;
            ladderBox.IsAffectedByGravity = false;
            component = new DummyComponent(ladderBox);            
            Components.Add(component);
            Game1.thegame.ModelDrawer.Add(ladderBox);
        }
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < ladderBox.CollisionInformation.Pairs.Count; i++)
            {
                var pair = ladderBox.CollisionInformation.Pairs[i];
                EntityCollidable candidate = (pair.BroadPhaseOverlap.EntryA == ladderBox.CollisionInformation ? pair.BroadPhaseOverlap.EntryB : pair.BroadPhaseOverlap.EntryA) as EntityCollidable;
                if (candidate == null)
                    continue;
                Entity grabbedEntity = candidate.Entity;
                if (grabbedEntity.Tag == null)
                {
                    continue;
                }
                else
                {
                    if (grabbedEntity.Tag is CharacterController)
                    {
                        var character = grabbedEntity.Tag as CharacterController;
                        character.ladder = true;
                    }
                }
            }
        }
    }
}
