using System;
using System.Collections;
using System.IO;
using ProtoBuf;

namespace ZEngine.Network
{
    public interface IChannelWriteExecuteHandle : IExecuteHandle<IChannelWriteExecuteHandle>
    {
        IChannel channel { get; }
        byte[] bytes { get; }
        Status status { get; }
    }
}