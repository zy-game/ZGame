using System;

namespace ZGame.Notify
{
    class CommonNotifyHandle : INotifyHandler
    {
        private Action<INotifyDatable> _handler;
        public string eventName { get; private set; }

        public CommonNotifyHandle(string name, Action<INotifyDatable> handler)
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


        public void Notify(INotifyDatable datable)
        {
            _handler?.Invoke(datable);
        }

        public override bool Equals(object obj)
        {
            if (obj is CommonNotifyHandle eventNotifyHandler)
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

    class CommonNotifyHandle<T> : INotifyHandler<T> where T : INotifyDatable
    {
        private Action<T> _handler;
        public string eventName { get; private set; }

        public CommonNotifyHandle(string name, Action<T> handler)
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

        public void Notify(INotifyDatable datable)
        {
            Notify((T)datable);
        }

        public void Notify(T args)
        {
            _handler?.Invoke(args);
        }

        public override bool Equals(object obj)
        {
            if (obj is CommonNotifyHandle<T> eventNotifyHandler)
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