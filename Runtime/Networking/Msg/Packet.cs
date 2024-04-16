using System.IO;
using ZGame;
using ZGame.Networking;

namespace ZGame.Networking
{
    public class Packet : IReference
    {
        public long id { get; private set; }
        public int opcode { get; private set; }
        public byte[] Data { get; private set; }

        public void Release()
        {
            id = 0;
            opcode = 0;
            Data = null;
        }

        public static byte[] Serialized(long id, int opcode, byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    bw.Write(id);
                    bw.Write(opcode);
                    bw.Write(bytes.Length);
                    bw.Write(bytes);
                    return ms.ToArray();
                }
            }
        }

        public static Packet Deserialized(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    Packet msg = RefPooled.Spawner<Packet>();
                    msg.id = br.ReadInt64();
                    msg.opcode = br.ReadInt32();
                    msg.Data = br.ReadBytes(br.ReadInt32());
                    return msg;
                }
            }
        }
    }
}