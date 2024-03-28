using System;

namespace ZGame
{
    public interface IGameCacheObject : IReferenceObject
    {
        string name { get; }
        int refCount { get; }
        void Ref();
        void Unref();
    }
}