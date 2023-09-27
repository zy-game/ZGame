using System;

namespace ZEngine.Network
{
    public interface IChannel : IDisposable
    {
        string address { get; }

        bool connected { get; }
        void Close();
        void Connect(string address, int id = 0);
        void WriteAndFlush(byte[] bytes);
    }
}