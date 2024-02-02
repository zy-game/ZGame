using System;
using System.Collections.Generic;

namespace ZGame
{
    public interface IQuery : IDisposable
    {
        object Query(int key);
        object Query(Func<object, bool> whereFunc);
        List<object> Wheres(Func<object, bool> whereFunc);
    }

    public interface IQuery<T> : IQuery
    {
        T Query(int key);
        T Query(Func<T, bool> func);
        List<T> Wheres(Func<T, bool> func);
    }
}