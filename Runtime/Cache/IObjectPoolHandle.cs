using System;

namespace ZEngine.Cache
{
    public interface IObjectPoolHandle : IDisposable
    {
        int count { get; }
        Type cacheType { get; }
        void Release(string key, object value);
        object Dequeue(string key);
        void OnUpdate();
    }
}