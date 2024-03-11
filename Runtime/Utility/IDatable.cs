using System;
using System.Collections.Generic;

namespace ZGame
{
    /// <summary>
    /// 数据接口
    /// </summary>
    public interface IDatable : IDisposable
    {
    }

    public interface IDatableTemplete : IDatable
    {
        bool Equals(string field, object value);
    }

    public interface IDatableTemplete<T> : IDatableTemplete
    {
    }
}