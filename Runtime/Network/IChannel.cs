using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace ZEngine.Network
{
    /// <summary>
    /// 网络管道
    /// </summary>
    public interface IChannel : IDisposable
    {
        string address { get; }

        bool connected { get; }
        UniTask<IChannel> Close();
        UniTask<IChannel> Connect(string address, int id = 0);
        UniTask WriteAndFlush(byte[] bytes);
    }
}