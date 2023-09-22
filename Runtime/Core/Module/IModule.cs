using System;

namespace ZEngine
{
    public abstract class ExecuteCommand : IDisposable
    {
        public abstract void OnComplate();
        public virtual void Dispose()
        {
        }
    }

    public interface IModule : IDisposable
    {
        void Schedule(ExecuteCommand command);
    }
}