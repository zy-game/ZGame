using System;
using System.Collections;

namespace ZEngine.Network
{
    public interface IChannelClosedExecuteHandle : IExecuteHandle<IChannelClosedExecuteHandle>
    {
        IChannel channel { get; }
    }
}