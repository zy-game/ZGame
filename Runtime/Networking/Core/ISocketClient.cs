using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Networking
{
    public interface ISocketClient : IReference
    {
        int cid { get; }
        string address { get; }

        UniTask Connect(string address, ushort port);
        void Write(byte[] message);
        void DoUpdate();
    }
}