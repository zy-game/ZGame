using System;

namespace ZEngine
{
    public interface IBindPipeline : IDisposable
    {
        string name { get; }
        bool actived { get; }
        void Active();
        void Inactive();
    }

    public interface IEventBindPipeline<T> : IBindPipeline
    {
        void AddListener(Action<T, object> callback);
        void RemoveListener(Action<T, object> callback);
        void Invoke(object args);
    }

    public interface IValueBindPipeline<T> : IBindPipeline
    {
        T value { get; }
        void SetValue(T value);
    }
}