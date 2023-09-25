using System;

namespace ZEngine.Network
{
    public interface IChannel : IDisposable
    {
        string address { get; }

        bool connected { get; }
        IExecuteHandle Close();
        IExecuteHandle<IChannel> Connect(string address, int id = 0);
        IExecuteHandle WriteAndFlush(byte[] bytes);
    }
}