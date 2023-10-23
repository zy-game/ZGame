using System;
using System.IO;

namespace ZEngine.Network
{
    /// <summary>
    /// 游戏消息
    /// </summary>
    public interface IMessaged : IDisposable
    {
        public static IMessaged Deserialize(byte[] bytes)
        {
            return new MessagePacket()
            {
                body = bytes
            };
        }

        public static byte[] Serialize(object message)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);
            writer.Write(Crc32.GetCRC32Str(message.GetType().FullName));
            ProtoBuf.Serializer.Serialize(memoryStream, message);
            return memoryStream.ToArray();
        }

        class MessagePacket : IMessaged
        {
            public ulong id { get; set; }
            public byte[] body { get; set; }

            public byte[] ToBinary()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                id = 0;
                body = Array.Empty<byte>();
                GC.SuppressFinalize(this);
            }
        }
    }
}