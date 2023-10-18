using System;

namespace ZEngine
{
    public interface IDatableHandle : IDisposable
    {
        string name { get; }
    }
}