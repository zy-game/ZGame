using System;

namespace ZGame.Game
{
    public abstract class GameHandle : IDisposable
    {
        public virtual void OnEntry(params object[] args)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}