using Cysharp.Threading.Tasks;

namespace ZGame.Networking
{
    public interface INetClient : IReference
    {
        int cid { get; }
        string address { get; }
        bool isConnected { get; }
        UniTask<Status> ConnectAsync(int cid, string address, ushort port, IDispatcher dispatcher);
        UniTask WriteAndFlushAsync(byte[] message);
        
    }
}