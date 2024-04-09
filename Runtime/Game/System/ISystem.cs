﻿namespace ZGame.Game
{
    /// <summary>
    /// 逻辑系统
    /// </summary>
    public interface ISystem : IReferenceObject
    {
        /// <summary>
        /// 系统轮询优先级
        /// </summary>
        uint priority { get; }
    }
}