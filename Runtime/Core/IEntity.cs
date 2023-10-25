using UnityEngine;
using System;

namespace ZGame
{
    /// <summary>
    /// 实体对象
    /// </summary>
    public interface IEntity : IDisposable
    {
        string guid { get; }
    }
}