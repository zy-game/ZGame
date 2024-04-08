using System.IO;
using ZGame;
using ZGame.Networking;

namespace ZGame.Networking
{
    public class Packet : IMessaged
    {
        public int opcode { get; set; }
        public byte[] Data { get; set; }

        public void Release()
        {
        }

        public static byte[] Create(int opcode, byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
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
                    Packet msg = GameFrameworkFactory.Spawner<Packet>();
                    msg.opcode = br.ReadInt32();
                    msg.Data = br.ReadBytes(br.ReadInt32());
                    return msg;
                }
            }
        }
    }
}