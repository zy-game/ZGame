using System.IO;
using ZGame.Networking;

namespace ZGame.Game.LockStep
{
    public class MSG_UserJoin : MSGRoom
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        public uint uid;

        public override void Release()
        {
            base.Release();
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
            AppCore.Logger.Log("MSG_UserJoin:" + uid);
        }
    }
}