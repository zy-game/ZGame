using System;

namespace ZGame.Notify
{
    public interface INotifyArgs : IDisposable
    {
    }

    public sealed class NotifyManager : Singleton<NotifyManager>
    {
        
    }
}