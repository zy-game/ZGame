using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Networking
{
    public interface IChannel : IDisposable
    {
        string address { get; }
        bool connected { get; }
        ISerialize serialize { get; }

        UniTask Connect(string address);
        UniTask Close();
        void WriteAndFlush(IMessage message);
        UniTask<T> WriteAndFlushAsync<T>(IMessage message) where T : IMessage;
    }
}