using System;

namespace ZGame.Runnable
{
    public abstract class RunnableHandle : IDisposable
    {
        public virtual void OnStart(params object[] args)
        {
        }

        public virtual void OnUpdate()
        {
        }

        public abstract bool IsCompletion();

        public virtual void Dispose()
        {
        }
    }
}