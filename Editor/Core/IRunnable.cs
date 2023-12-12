using System;

namespace ZGame.Editor
{
    public interface IRunnable : IDisposable
    {
        void Execute(params object[] args);
    }
}