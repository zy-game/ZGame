using System;
using System.Collections.Generic;
using UnityEngine;
using ZEngine.Network;

namespace ZEngine.Game
{
    public interface ISyncMsg : IMessaged
    {
        int guid { get; }
        byte command { get; }
    }

    public interface ISynchor : IDisposable
    {
        /// <summary>
        /// 网络ID
        /// </summary>
        int guid { get; }

        IPlayer player { get; }

        void Sync();
    }

    /// <summary>
    /// 角色对象
    /// </summary>
    public interface IPlayer : IDisposable
    {
        int guid { get; }

        /// <summary>
        /// 网络同步句柄
        /// </summary>
        ISynchor synchor { get; }

        /// <summary>
        /// 角色持有的状态机
        /// </summary>
        IStateMachine stateMachine { get; }
    }
}