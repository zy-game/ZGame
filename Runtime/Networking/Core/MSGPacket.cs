using System;
using System.IO;
using System.Text;
using ZGame;
using ZGame.Events;
using ZGame.Networking;

namespace ZGame.Networking
{
    public class MSGPacket : IReference
    {
        /// <summary>
        /// 消息包编号
        /// </summary>
        public long id { get; private set; }

        /// <summary>
        /// 协议号
        /// </summary>
        public int opcode { get; private set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int status { get; private set; }

        /// <summary>
        /// 详情
        /// </summary>
        public string message { get; private set; }

        /// <summary>
        /// 消息数据
        /// </summary>
        public byte[] Content { get; private set; }

        public void Release()
        {
            id = 0;
            opcode = 0;
            status = 0;
            Content = null;
        }

        public T Decode<T>() where T : IMessage
        {
            if (Content is null || Content.Length == 0)
            {
                return default;
            }

            using (MemoryStream ms = new MemoryStream(Content))
            {
                using (BinaryReader br = new BinaryReader(ms))
                {
                    T msg = RefPooled.Alloc<T>();
                    msg.Decode(br);
                    return msg;
                }
            }
        }

        public bool EnsureMessageStatusCode()
        {
            return status is 200;
        }


        private static long packetId = 0;

        public static byte[] Serialize(int opcode, IMessage msg)
        {
            return Serialize(opcode, msg, 200, "null");
        }

        public static byte[] Serialize(int opcode, IMessage msg, int status, string message)
        {
            packetId++;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    using (MemoryStream body = new MemoryStream())
                    {
                        using (BinaryWriter bodyWrite = new BinaryWriter(body))
                        {
                            writer.Write(packetId);
                            writer.Write(opcode);
                            writer.Write(status);
                            writer.Write(message);
                            if (msg is null)
                            {
                                writer.Write(0);
                            }
                            else
                            {
                                msg.Encode(bodyWrite);
                                writer.Write(body.Length);
                                writer.Write(body.ToArray());
                            }

                            return stream.ToArray();
                        }
                    }
                }
            }
        }


        public static MSGPacket Deserialize(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    MSGPacket packet = RefPooled.Alloc<MSGPacket>();
                    packet.id = reader.ReadInt64();
                    packet.opcode = reader.ReadInt32();
                    packet.status = reader.ReadInt32();
                    packet.message = reader.ReadString();
                    packet.Content = reader.ReadBytes((int)reader.ReadInt64());
                    return packet;
                }
            }
        }
    }
}