using System;

namespace ZGame.Notify
{
    class DefaultActionEventNotifyHandle : IEventNotifyHandler
    {
        private Action<INotifyArgs> _handler;
        public string eventName { get; private set; }

        public DefaultActionEventNotifyHandle(string name, Action<INotifyArgs> handler)
        {
            this._handler = handler;
            this.eventName = name;
        }

        public void Dispose()
        {
            _handler = null;
            eventName = default;
            GC.SuppressFinalize(this);
        }


        public void Notify(INotifyArgs args)
        {
            _handler?.Invoke(args);
        }

        public override bool Equals(object obj)
        {
            if (obj is DefaultActionEventNotifyHandle eventNotifyHandler)
            {
                return eventNotifyHandler._handler.Equals(_handler);
            }

            if (obj is Type type)
            {
                return this.GetType().Equals(type);
            }

            return _handler.Equals(obj);
        }
    }

    class DefaultActionEventNotifyHandle<T> : IEventNotifyHandler<T> where T : INotifyArgs
    {
        private Action<T> _handler;
        public string eventName { get; private set; }

        public DefaultActionEventNotifyHandle(string name, Action<T> handler)
        {
            this._handler = handler;
            this.eventName = name;
        }

        public void Dispose()
        {
            _handler = null;
            eventName = default;
            GC.SuppressFinalize(this);
        }

        public void Notify(INotifyArgs args)
        {
            Notify((T)args);
        }

        public void Notify(T args)
        {
            _handler?.Invoke(args);
        }

        public override bool Equals(object obj)
        {
            if (obj is DefaultActionEventNotifyHandle<T> eventNotifyHandler)
            {
                return eventNotifyHandler._handler.Equals(_handler);
            }

            if (obj is Type type)
            {
                return this.GetType().Equals(type);
            }

            return _handler.Equals(obj);
        }
    }
}