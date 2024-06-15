using System.IO;

namespace ZGame.Networking
{
    public class MSGRoom : IMessage
    {
        public int rid;

        public virtual void Encode(BinaryWriter writer)
        {
            writer.Write(rid);
        }

        public virtual void Decode(BinaryReader reader)
        {
            rid = reader.ReadInt32();
        }

        public virtual void Release()
        {
            rid = 0;
        }
    }
}