using Cysharp.Threading.Tasks;

namespace ZGame.Networking
{
    public interface INetClient : IReferenceObject
    {
        int id { get; }
        string address { get; }
        UniTask<Status> ConnectAsync(string address, ushort port);
        UniTask WriteAsync(IMessage message);
        UniTask WriteAndFlushAsync(IMessage message);
    }
}