using System;
using System.Collections.Generic;

namespace ZGame.State
{
    public class StateMachine : IDisposable
    {
        public string name { get; }
        public StateHandle current { get; private set; }
        private Dictionary<Type, StateHandle> _handles = new Dictionary<Type, StateHandle>();

        public StateMachine(string name)
        {
            this.name = name;
        }

        public void AddState(Type type)
        {
            if (_handles.ContainsKey(type))
            {
                return;
            }

            StateHandle handle = Activator.CreateInstance(type) as StateHandle;
            _handles.Add(type, handle);
        }

        public void Switch(Type type)
        {
            if (_handles.TryGetValue(type, out StateHandle handle) is false)
            {
                handle = Activator.CreateInstance(type) as StateHandle;
                handle.OnAwake();
                _handles.Add(type, handle);
            }

            current?.OnExit();
            current = handle;
            current?.OnEntry();
        }

        public void OnUpdate()
        {
            if (current is null)
            {
                return;
            }

            current.OnUpdate();
        }

        public void Dispose()
        {
            current?.OnExit();
            current = null;
            foreach (StateHandle handle in _handles.Values)
            {
                handle.Dispose();
            }

            _handles.Clear();
        }
    }
}