using System;

namespace ZEngine
{
    public interface IToken : IDisposable
    {
        object result { get; }
        bool isComplate { get; }
        void Complate(object value);
    }
}