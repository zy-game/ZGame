using System.IO;

namespace ZGame.Networking
{
    public class MsgBasic : IMsg
    {
        public uint sid;

        public void Release()
        {
        }


        public virtual void Encode(BinaryWriter writer)
        {
            writer.Write(sid);
        }

        public virtual void Decode(BinaryReader reader)
        {
            sid = reader.ReadUInt32();
        }
    }
}