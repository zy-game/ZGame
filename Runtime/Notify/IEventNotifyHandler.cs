using System;

namespace ZGame.Notify
{
    public interface IEventNotifyHandler : IDisposable
    {
        string eventName { get; }
        void Notify(INotifyArgs args);
    }
    
    public interface IEventNotifyHandler<T> : IEventNotifyHandler where T : INotifyArgs
    {
        void Notify(T args);
    }
}