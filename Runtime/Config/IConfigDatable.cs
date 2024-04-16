using System;
using System.Collections.Generic;

namespace ZGame
{
    /// <summary>
    /// 数据接口
    /// </summary>
    public interface IConfigDatable : IReference
    {
        bool Equals(string field, object value);
    }
}