namespace ZEngine.Network
{
    public interface ISubscribeMessageExecuteHandle : IReference
    {
        void OnHandleMessage(IMessagePacket messagePacket);
    }
}