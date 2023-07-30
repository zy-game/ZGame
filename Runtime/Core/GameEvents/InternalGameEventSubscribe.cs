using System;

namespace ZEngine
{
    class InternalGameEventSubscribe<T> : ISubscribe<T> where T : GameEventArgs<T>
    {
        public Action<T> callback;


        public void Execute(params object[] args)
        {
        }

        public void Execute(T args)
        {
            callback?.Invoke(args);
        }

        public void Release()
        {
            callback = null;
        }
    }
}