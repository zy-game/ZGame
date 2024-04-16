using System.IO;
using ZGame.Networking;

namespace ZGame.Game
{
    public class UserJoin : RoomMsg
    {
        public uint uid;
        public string path;
        public BEPUutilities.Vector3 position;
        public BEPUutilities.Quaternion rotation;

        public void Release()
        {
            uid = 0;
            path = null;
            position = BEPUutilities.Vector3.Zero;
            rotation = BEPUutilities.Quaternion.Identity;
        }


        public static byte[] Create(uint uid, string path, BEPUutilities.Vector3 position, BEPUutilities.Quaternion rotation)
        {
            UserJoin data = RefPooled.Spawner<UserJoin>();
            data.uid = uid;
            data.path = path;
            data.position = position;
            data.rotation = rotation;
            return IMsg.Encode(data);
        }

        public override void Decode(BinaryReader reader)
        {
            base.Decode(reader);
            uid = reader.ReadUInt32();
            path = reader.ReadString();
            position = new BEPUutilities.Vector3(reader.ReadInt64(), reader.ReadInt64(), reader.ReadInt64());
            rotation = new BEPUutilities.Quaternion(reader.ReadInt64(), reader.ReadInt64(), reader.ReadInt64(), reader.ReadInt64());
        }

        public override void Encode(BinaryWriter writer)
        {
            base.Encode(writer);
            writer.Write(uid);
            writer.Write(path);
            writer.Write(position.X.RawValue);
            writer.Write(position.Y.RawValue);
            writer.Write(position.Z.RawValue);
            writer.Write(rotation.X.RawValue);
            writer.Write(rotation.Y.RawValue);
            writer.Write(rotation.Z.RawValue);
            writer.Write(rotation.W.RawValue);
        }
    }
}