using System;

namespace ZEngine
{
    public abstract class GameEventSubscribe<T> : ISubscribe where T : IEventArgs
    {
        public virtual void Release()
        {
            GC.SuppressFinalize(this);
        }

        protected abstract void Execute(T eventArgs);

        public void Execute(params object[] args)
        {
            this.Execute((T)args[0]);
        }
    }
}