using System;
using System.Collections.Generic;
using UnityEngine;
using ZEngine.Network;

namespace ZEngine.Game
{
    

    /// <summary>
    /// 游戏实体对象
    /// </summary>
    public interface IGameEntity : IDisposable
    {
        int guid { get; }

        /// <summary>
        /// 网络同步句柄
        /// </summary>
        ISynchro synchor { get; }

        /// <summary>
        /// 角色持有的状态机
        /// </summary>
        IStateMachine stateMachine { get; }
    }
}