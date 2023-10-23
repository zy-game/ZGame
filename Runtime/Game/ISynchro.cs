using System;
using ZEngine.Network;

namespace ZEngine.Game
{
    public interface ISynchro : IDisposable
    {
        /// <summary>
        /// 玩家网络标识
        /// </summary>
        int guid { get; }

        void Synchro(ISyncMsg msg);
    }

    public interface ISyncMsg : IMessaged
    {
        /// <summary>
        /// 玩家网络标识
        /// </summary>
        int guid { get; }

        /// <summary>
        /// 消息包序号
        /// </summary>
        int index { get; }

        /// <summary>
        /// 操作指令
        /// </summary>
        byte command { get; }
    }
}