using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using ZGame.Notify;

namespace ZGame.Networking
{
    public interface IMsg : IReference
    {
        void Encode(BinaryWriter writer);
        void Decode(BinaryReader reader);

        private static int _msgId = 0;

        public static int GetMsgId()
        {
            return _msgId++;
        }

        public static T Decode<T>(byte[] bytes) where T : IMsg
        {
            return (T)Decode(typeof(T), bytes);
        }

        public static IMsg Decode(Type msgType, byte[] bytes)
        {
            if (typeof(IMsg).IsAssignableFrom(msgType) is false)
            {
                throw new Exception($"{msgType.Name} is not a IMsg type");
            }

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                using (BinaryReader reader = new BinaryReader(ms))
                {
                    IMsg msg = (IMsg)RefPooled.Spawner(msgType);
                    msg.Decode(reader);
                    return msg;
                }
            }
        }

        public static byte[] Encode(IMsg msg)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(ms))
                {
                    msg.Encode(writer);
                    return ms.ToArray();
                }
            }
        }
    }
}