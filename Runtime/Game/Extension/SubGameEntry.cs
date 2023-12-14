using System;

namespace ZGame.Game
{
    public abstract class SubGameEntry : IDisposable
    {
        public virtual void OnEntry(params object[] args)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}