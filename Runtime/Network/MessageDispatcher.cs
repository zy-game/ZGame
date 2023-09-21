using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf.Meta;

namespace ZEngine.Network
{
    class MessageDispatcher : Single<MessageDispatcher>
    {
        private Dictionary<uint, Type> opcode = new Dictionary<uint, Type>();
        private Dictionary<Type, ISubscribeHandle> messageSubscribeList = new Dictionary<Type, ISubscribeHandle>();

        public void Subscribe(Type type, ISubscribeHandle subscribe)
        {
            uint crc = Crc32.GetCRC32Str(type.Name);
            if (opcode.TryGetValue(crc, out Type _) is false)
            {
                opcode.Add(crc, type);
            }

            if (messageSubscribeList.TryGetValue(type, out ISubscribeHandle _subscribe) is false)
            {
                messageSubscribeList.Add(type, subscribe);
            }

            _subscribe.Merge(subscribe);
        }

        public void Unsubscribe(Type type, ISubscribeHandle subscribe)
        {
            if (messageSubscribeList.TryGetValue(type, out ISubscribeHandle _subscribe))
            {
                _subscribe.Unmerge(subscribe);
            }
        }

        public void Enqueue(IChannel channel, byte[] bytes)
        {
            uint crc = Crc32.GetCRC32Byte(bytes, 0, sizeof(uint));
            if (!opcode.TryGetValue(crc, out Type msgType))
            {
                Engine.Console.Log("位置的消息类型 crc：", crc);
                return;
            }

            MemoryStream memoryStream = new MemoryStream(bytes, sizeof(uint), bytes.Length - sizeof(uint));
            IMessaged message = (IMessaged)RuntimeTypeModel.Default.Deserialize(memoryStream, null, msgType);

            if (messageSubscribeList.TryGetValue(msgType, out ISubscribeHandle subscribeHandle) is false)
            {
                Engine.Console.Error("未找到订阅：", msgType.Name);
                return;
            }

            subscribeHandle.Execute(message);
        }
    }
}