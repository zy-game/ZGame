using System.IO;

namespace ZGame.Networking
{
    public interface IMessage : IReference
    {
        void Encode(BinaryWriter writer);

        void Decode(BinaryReader reader);
    }
}