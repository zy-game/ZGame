using System;
using System.Collections;

namespace ZEngine.Network
{
    public interface IChannelConnectExecuteHandle : IExecuteHandle<IChannelConnectExecuteHandle>
    {
        int id { get; }
        string address { get; }
        IChannel channel { get; }
    }
}