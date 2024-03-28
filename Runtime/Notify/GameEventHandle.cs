using System;

namespace ZGame.Notify
{
    class GameEventHandle<T> : IGameEventHandle<T> where T : IGameEventArgs
    {
        private Action<T> _handle;

        public static GameEventHandle<T2> Create<T2>(Action<T2> action) where T2 : IGameEventArgs
        {
            GameEventHandle<T2> handle = GameFrameworkFactory.Spawner<GameEventHandle<T2>>();
            handle._handle = action;
            return handle;
        }

        public void Release()
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