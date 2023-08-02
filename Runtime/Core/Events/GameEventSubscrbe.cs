using System;

namespace ZEngine
{
    public abstract class GameEventSubscrbe<T> : ISubscribe<T> where T : GameEventArgs<T>
    {
        public abstract void Execute(T args);

        public abstract void Release();

        public void Execute(params object[] args)
        {
            throw new NotImplementedException();
        }
    }
}