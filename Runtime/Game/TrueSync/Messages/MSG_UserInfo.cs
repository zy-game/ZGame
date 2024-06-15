using System.IO;
using ZGame.Networking;

namespace ZGame.Game.LockStep
{
    public class MSG_UserInfo : MSGRoom
    {
        public uint uid { get; set; }
        public string name { get; set; }
        public string avatar { get; set; }

        public override void Release()
        {
            base.Release();
            uid = 0;
            name = null;
            avatar = null;
        }

        public override void Decode(BinaryReader reader)
        {
            base.Decode(reader);
            uid = reader.ReadUInt32();
            name = reader.ReadString();
            avatar = reader.ReadString();
        }

        public override void Encode(BinaryWriter writer)
        {
            base.Encode(writer);
            writer.Write(uid);
            writer.Write(name);
            writer.Write(avatar);
        }
    }
}