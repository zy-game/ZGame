using System;
using System.Collections.Generic;

namespace ZGame
{
    /// <summary>
    /// 数据接口
    /// </summary>
    public interface IConfigDatable : IReferenceObject
    {
        bool Equals(string field, object value);
    }
}