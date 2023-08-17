namespace ZEngine.Network
{
    public interface IChannel : IReference
    {
        string address { get; }

        bool connected { get; }
        void Close();
        void Connect(string address);
        void WriteAndFlush(byte[] bytes);
    }
}