using System;

namespace ZGame.Notify
{
    class GameEventHandle : IGameEventHandle
    {
        private Action<IGameEventArgs> _handle;

        public GameEventHandle(Action<IGameEventArgs> handle)
        {
            _handle = handle;
        }

        public void Notify(IGameEventArgs args)
        {
            this._handle(args);
        }

        public override bool Equals(object obj)
        {
            if (obj is Action<IGameEventArgs> eventHandle)
            {
                return _handle.Equals(eventHandle);
            }

            return base.Equals(obj);
        }

        public virtual void Dispose()
        {
            _handle = null;
        }
    }

    class GameEventHandle<T> : IGameEventHandle<T> where T : IGameEventArgs
    {
        private Action<T> _handle;

        public GameEventHandle(Action<T> handle)
        {
            _handle = handle;
        }

        public void Dispose()
        {
            _handle = null;
        }

        public void Notify(IGameEventArgs args)
        {
            this.Notify((T)args);
        }

        public void Notify(T args)
        {
            this._handle(args);
        }

        public override bool Equals(object obj)
        {
            if (obj is Action<T> eventHandle)
            {
                return _handle.Equals(eventHandle);
            }

            return base.Equals(obj);
        }
    }
}