using System;
using System.Collections.Generic;

namespace ZGame.Notify
{
    class EventHandleGroup : IDisposable
    {
        public string eventName { get; private set; }

        private List<INotifyHandler> _handlers = new();

        public EventHandleGroup(string name)
        {
            this.eventName = name;
        }

        public void Add(INotifyHandler handler)
        {
            _handlers.Add(handler);
        }

        public void Remove(INotifyHandler handler)
        {
            _handlers.RemoveAll(x => x.Equals(handler));
        }

        public void Remove(Action<INotifyDatable> handler)
        {
            _handlers.RemoveAll(x => x.Equals(handler));
        }

        public void Remove<T>(Action<T> handler)
        {
            _handlers.RemoveAll(x => x.Equals(handler));
        }

        public void Remove(Type type)
        {
            _handlers.RemoveAll(x => x.Equals(type));
        }

        public void Notify(INotifyDatable datable)
        {
            for (int i = _handlers.Count - 1; i >= 0; i--)
            {
                _handlers[i].Notify(datable);
            }
        }

        public bool Contains<T>(Action<T> handler) where T : INotifyDatable
        {
            return _handlers.Exists(x => x.Equals(handler));
        }

        public bool Contains(Action<INotifyDatable> handler)
        {
            return _handlers.Exists(x => x.Equals(handler));
        }

        public bool Contains(Type type)
        {
            return _handlers.Exists(x => x.GetType() == type);
        }

        public bool Contains(INotifyHandler handler)
        {
            return _handlers.Exists(x => x.Equals(handler));
        }

        public void Dispose()
        {
            _handlers.ForEach(x => x.Dispose());
            _handlers.Clear();
        }
    }
}