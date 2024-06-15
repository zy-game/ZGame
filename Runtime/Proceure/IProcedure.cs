using System;
using System.Reflection;
using Cysharp.Threading.Tasks;

namespace ZGame
{
    public interface IProcedure : IProcedure<object>
    {
    }

    public interface IProcedure<T> : IReference
    {
        T Execute(params object[] args);
    }
}