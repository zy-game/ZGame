using System.IO;

namespace ZGame.Networking
{
    public class RoomMsg : MsgBasic
    {
        public uint rid;
        public string rName;

        public override void Encode(BinaryWriter writer)
        {
            base.Encode(writer);
            writer.Write(rid);
            writer.Write(rName);
        }

        public override void Decode(BinaryReader reader)
        {
            base.Decode(reader);
            rid = reader.ReadUInt32();
            rName = reader.ReadString();
        }
    }
}