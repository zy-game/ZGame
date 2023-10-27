using Cysharp.Threading.Tasks;

namespace ZGame.Networking
{
    public interface IChannel : IEntity
    {
        string address { get; }
        bool connected { get; }
        UniTask Connect(string address);
        UniTask Close();
        void WriteAndFlush(byte[] bytes);
    }
}