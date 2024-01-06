using System;
using System.Collections.Generic;

namespace ZGame.Game
{
    public class StateManager : IDisposable
    {
        private Stateable _current;
        public Stateable current => _current;
        private List<Stateable> _states = new List<Stateable>();


        public StateManager(Actorable actor)
        {
        }

        public Stateable GetState(string stateName)
        {
            return _states.Find(x => x.name.Equals(stateName));
        }

        public T GetState<T>() where T : Stateable
        {
            return (T)_states.Find(x => x is T);
        }

        public void Switch(string stateName)
        {
            Switch(GetState(stateName));
        }

        public void Switch<T>() where T : Stateable
        {
            Switch(GetState<T>());
        }

        public void Switch(Stateable state)
        {
            if (_current != null)
            {
                _current.Inactive();
            }

            _current = state;
            if (_current != null)
            {
                _current.Active();
            }
        }

        public void OnUpdate()
        {
            if (_current is null)
            {
                return;
            }

            _current.OnUpdate();
        }

        public void Dispose()
        {
            if (_current != null)
            {
                _current.Dispose();
                _current = null;
            }

            for (int i = _states.Count - 1; i >= 0; i--)
            {
                _states[i].Dispose();
            }

            _states.Clear();
            GC.SuppressFinalize(this);
        }
    }
}