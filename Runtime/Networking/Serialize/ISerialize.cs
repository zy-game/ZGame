namespace ZGame.Networking
{
    public interface ISerialize : IReferenceObject
    {
        IMessage Deserialize(byte[] bytes, out uint opcode);
        byte[] Serialize(IMessage message);
    }
}