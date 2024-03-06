using System;
using ZGame.Module;

namespace ZGame.Notify
{
    public interface INotifyArgs : IDisposable
    {
    }

    public sealed class NotifyManager : IModule
    {
        public void Dispose()
        {
        }

        public void OnAwake()
        {
        }
    }
}