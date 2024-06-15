using System.Collections.Generic;
using System.IO;
using ZGame.Networking;

namespace ZGame.Game.LockStep
{
    public class MSG_RoomInfo : MSGRoom
    {
        public string rName;

        /// <summary>
        /// 当前房间的随机数种子
        /// </summary>
        public uint seed;

        public override void Release()
        {
            base.Release();
            rName = null;
            seed = 0;
        }

        public override void Decode(BinaryReader reader)
        {
            base.Decode(reader);
            rName = reader.ReadString();
            seed = reader.ReadUInt32();
        }

        public override void Encode(BinaryWriter writer)
        {
            base.Encode(writer);
            writer.Write(rName);
            writer.Write(seed);
        }
    }
}