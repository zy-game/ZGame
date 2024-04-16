using System.IO;
using ZGame.Networking;

namespace ZGame.Game
{
    public class UserReady : RoomMsg
    {
        /// <summary>
        /// 
        /// </summary>
        public uint uid;

        public void Release()
        {
            uid = 0;
        }

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
    }
}