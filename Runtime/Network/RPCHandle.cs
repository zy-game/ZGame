using System;
using System.Collections.Generic;
using System.IO;
using ProtoBuf.Meta;

namespace ZEngine.Network
{
    class RPCHandle : Singleton<RPCHandle>, IMessageHandle
    {
        private List<IMessageHandle> handles = new List<IMessageHandle>();
        private Dictionary<uint, Type> opcode = new Dictionary<uint, Type>();
        private Dictionary<Type, List<IToken>> subscribers = new Dictionary<Type, List<IToken>>();

        public RPCHandle()
        {
            handles.Add(this);
        }

        public override void Dispose()
        {
            handles.Clear();
            opcode.Clear();
            subscribers.Clear();
        }

        public void Handle(IMessaged messaged)
        {
            Type source = messaged.GetType();
            if (subscribers.TryGetValue(source, out List<IToken> tokens) is false)
            {
                return;
            }

            tokens.ForEach(x =>
            {
                x.Complate(messaged);
                x.Dispose();
            });
            tokens.Clear();
            subscribers.Remove(source);
        }

        public void Dispacher(IChannel channel, byte[] bytes)
        {
            uint crc = Crc32.GetCRC32Byte(bytes, 0, sizeof(uint));
            if (!opcode.TryGetValue(crc, out Type msgType))
            {
                Engine.Console.Log("位置的消息类型 crc：", crc);
                return;
            }

            MemoryStream memoryStream = new MemoryStream(bytes, sizeof(uint), bytes.Length - sizeof(uint));
            IMessaged message = (IMessaged)RuntimeTypeModel.Default.Deserialize(memoryStream, null, msgType);
            for (int i = handles.Count - 1; i >= 0; i--)
            {
                handles[i].Handle(message);
            }
        }

        public void Subscribe(Type type, IToken source)
        {
            if (subscribers.TryGetValue(type, out List<IToken> tokens) is false)
            {
                subscribers.Add(type, tokens = new List<IToken>());
            }

            tokens.Add(source);
        }

        public void Subscribe(Type type)
        {
            if (typeof(IMessageHandle).IsAssignableFrom(type))
            {
                return;
            }

            handles.Add((IMessageHandle)Activator.CreateInstance(type));
        }

        public void Unsubscribe(Type type)
        {
            IMessageHandle handle = handles.Find(x => x.GetType() == type);
            if (handle is null)
            {
                return;
            }

            handles.Remove(handle);
            handle.Dispose();
        }

        public void RegisterMessageType(params Type[] types)
        {
            for (int i = 0; i < types.Length; i++)
            {
                uint crc = Crc32.GetCRC32Str(types[i].Name);
                if (opcode.ContainsKey(crc))
                {
                    continue;
                }

                opcode.Add(crc, types[i]);
            }
        }
    }
}