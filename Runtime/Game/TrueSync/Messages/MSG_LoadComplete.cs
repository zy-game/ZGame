using System.IO;
using ZGame.Networking;

namespace ZGame.Game.LockStep
{
    public class MSG_LoadComplete : MSGRoom
    {
        public uint uid;

        public override void Decode(BinaryReader reader)
        {
            base.Decode(reader);
            uid = reader.ReadUInt32();
        }

        public override void Encode(BinaryWriter writer)
        {
            base.Encode(writer);
            writer.Write(uid);
        }

        public override void Release()
        {
            base.Release();
            uid = 0;
        }
    }
}