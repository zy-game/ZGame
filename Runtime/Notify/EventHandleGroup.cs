using System;
using System.Collections.Generic;

namespace ZGame.Notify
{
    class EventHandleGroup : IDisposable
    {
        public string eventName { get; private set; }

        private List<IEventNotifyHandler> _handlers = new();

        public EventHandleGroup(string name)
        {
            this.eventName = name;
        }

        public void Add(IEventNotifyHandler handler)
        {
            _handlers.Add(handler);
        }

        public void Remove(IEventNotifyHandler handler)
        {
            _handlers.RemoveAll(x => x.Equals(handler));
        }

        public void Remove(Action<INotifyArgs> handler)
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

        public void Notify(INotifyArgs args)
        {
            for (int i = _handlers.Count - 1; i >= 0; i--)
            {
                _handlers[i].Notify(args);
            }
        }

        public bool Contains<T>(Action<T> handler) where T : INotifyArgs
        {
            return _handlers.Exists(x => x.Equals(handler));
        }

        public bool Contains(Action<INotifyArgs> handler)
        {
            return _handlers.Exists(x => x.Equals(handler));
        }

        public bool Contains(Type type)
        {
            return _handlers.Exists(x => x.GetType() == type);
        }

        public bool Contains(IEventNotifyHandler handler)
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