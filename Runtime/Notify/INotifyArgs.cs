using System;

namespace ZGame.Notify
{
    public interface INotifyArgs : IDisposable
    {
    }

    public sealed class NormalNotifyArgs : INotifyArgs
    {
        public void Dispose()
        {
        }
    }
}