namespace ZGame.Networking
{
    public interface ISerialize
    {
        IMessage Deserialize(byte[] bytes, out uint opcode);
        byte[] Serialize(IMessage message);
    }
}