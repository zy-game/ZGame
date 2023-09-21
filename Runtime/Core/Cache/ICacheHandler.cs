using System;

namespace ZEngine.Cache
{
    public interface ICacheHandler : IDisposable
    {
        int count { get; }
        Type cacheType { get; }
        void Release(string key, object value);
        object Dequeue(string key);
    }
}