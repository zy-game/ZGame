using System;
using Cysharp.Threading.Tasks;
using DotNetty.Buffers;

namespace ZGame.Networking
{
    public interface INetClient : IReferenceObject
    {
        int id { get; }
        string address { get; }
        bool isConnected { get; }
        UniTask<Status> ConnectAsync(string address, ushort port, IMessageHandler handler);
        UniTask WriteAndFlushAsync(byte[] message);
    }

    public interface IMessageHandler : IReferenceObject
    {
        void Active(INetClient client);
        void Inactive(INetClient client);
        void Receive(INetClient client, IByteBuffer message);
        void Exception(INetClient client, Exception exception);
    }
}