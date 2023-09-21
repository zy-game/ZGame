using System;

namespace ZEngine.Network
{
    public interface IChannel : IDisposable
    {
        string address { get; }

        bool connected { get; }
        IChannelClosedExecuteHandle Close();
        IChannelConnectExecuteHandle Connect(string address, int id = 0);
        IChannelWriteExecuteHandle WriteAndFlush(byte[] bytes);
    }
}