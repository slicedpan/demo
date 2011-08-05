using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;
using BEPUphysicsDrawer.Models;
using BEPUphysicsDrawer.Lines;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace phystest
{
    public class ActorContainer
    {

        #region fields
        private ModelDrawer _modelDrawer;
        private LineDrawer _constraintDrawer;
        private LightDrawer _lightDrawer;
        private Space _space;
        private List<Actor> _actors;
        private List<LPPMesh> _meshes;
        private List<Actor> _deleteList;
        private ContentManager Content;
        private Dictionary<string, Actor> _actorDictionary;
        public List<LPPMesh> LPPMeshes
        {
            get
            {
                return _meshes;
            }
        }
        private List<Component> _components;
        public List<Component> Components
        {
            get
            {
                return _components;
            }            
        }
        #endregion        

        public ActorContainer(Space space, ModelDrawer modelDrawer, LineDrawer constraintDrawer, LightDrawer lightDrawer)
        {
            _space = space;
            _meshes = new List<LPPMesh>();
            _deleteList = new List<Actor>();
            _actors = new List<Actor>();
            _components = new List<Component>();
            _modelDrawer = modelDrawer;
            _constraintDrawer = constraintDrawer;
            _lightDrawer = lightDrawer;
            _actorDictionary = new Dictionary<string, Actor>();
            Content = Game1.thegame.Content;
        }
        public void Add(Actor actorToAdd)
        {
            _actors.Add(actorToAdd);

            actorToAdd.LoadContent(Content);
            actorToAdd.InitializeComponents();

            foreach (Component component in actorToAdd.Components)
            {
                if (component is DummyComponent)
                {
                    _space.Add((component as DummyComponent).spaceObject);
                }
                else
                {
                    if (component.Mesh is LPPMesh)
                    {
                        _meshes.Add(component.Mesh as LPPMesh);
                    }
                    _components.Add(component);
                    if (component.Entity != null)
                    {
                        _modelDrawer.Add(component.Entity);
                        _space.Add(component.Entity);
                    }
                }
                foreach (Light light in component.Lights)
                {
                    _lightDrawer.Add(light);
                }
            }
            _constraintDrawer.Clear();
            for (int i = 0; i < _space.Solver.SolverUpdateables.Count; i++)
            {
                //Add the solver updateable and match up the activity setting.                
                LineDisplayObjectBase objectAdded = _constraintDrawer.Add(_space.Solver.SolverUpdateables[i]);
                if (objectAdded != null)
                    objectAdded.IsDrawing = _space.Solver.SolverUpdateables[i].IsActive;
            }
            if (actorToAdd.Name != "noname")
                _actorDictionary.Add(actorToAdd.Name, actorToAdd);
        }
        public void Remove(Actor actorToRemove)
        {
            _deleteList.Add(actorToRemove);
        }
        private void Delete(Actor actorToDelete)
        {
            foreach (Component component in actorToDelete.Components)
            {
                _meshes.Remove(component.Mesh as LPPMesh);
                _components.Remove(component);
                _modelDrawer.Remove(component.Entity);
                component.CleanUp();
                foreach (Light light in component.Lights)
                {
                    _lightDrawer.Remove(light);
                }
            }
            _actors.Remove(actorToDelete);
            if (actorToDelete.Name != "noname")
            {
                _actorDictionary.Remove(actorToDelete.Name);
            }
        }
        public void Update(GameTime gameTime)
        {
            foreach (Actor actor in _actors)
            {
                if (actor.messages.Count > 0)
                {
                    foreach (string msg in actor.messages)
                    {
                        string[] parts = msg.Split(' ', ':', ',');
                        if (_actorDictionary.ContainsKey(parts[0]))
                        {
                            _actorDictionary[parts[0]].HandleMessage(parts[1]);
                        }
                    }
                    actor.messages.Clear();
                }
            }
            foreach (Actor actor in _actors)
            {
                actor.Update(gameTime);
            }
            if (_deleteList.Count > 0)
            {
                foreach (Actor actor in _deleteList)
                {
                    Delete(actor);
                }
                _constraintDrawer.Clear();
                for (int i = 0; i < _space.Solver.SolverUpdateables.Count; i++)
                {
                    //Add the solver updateable and match up the activity setting.
                    LineDisplayObjectBase objectAdded = _constraintDrawer.Add(_space.Solver.SolverUpdateables[i]);
                    if (objectAdded != null)
                        objectAdded.IsDrawing = _space.Solver.SolverUpdateables[i].IsActive;
                }
            }
            _constraintDrawer.Update();
            _modelDrawer.Update();
        }        
    }
}
