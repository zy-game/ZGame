using System;

namespace ZGame.Notify
{
    public interface INotifyHandler : IDisposable
    {
        string eventName { get; }
        void Notify(INotifyDatable datable);
    }
    
    public interface INotifyHandler<T> : INotifyHandler where T : INotifyDatable
    {
        void Notify(T args);
    }
}