namespace ZGame.Networking
{
    public interface IChannel : IEntity
    {
        string address { get; }
        bool connected { get; }
        void Connect(string address);
        void Close();
        void WriteAndFlush(byte[] bytes);
    }
}