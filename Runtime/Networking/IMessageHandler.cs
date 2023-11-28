using System;
using System.IO;
using Cysharp.Threading.Tasks;
using ProtoBuf;

namespace ZGame.Networking
{
    public interface IMessageHandler : IDisposable
    {
        void ReceiveHandle(IChannel channel, uint opcode, MemoryStream memoryStream);
    }

    internal class MessageReceiverHandle<T> : IMessageHandler where T : IMessage
    {
        private uint _opcode;
        private Action<T> _callback;

        public void Add(Action<T> callback)
        {
            _callback += callback;
        }

        public void Remove(Action<T> callback)
        {
            _callback -= callback;
        }

        public MessageReceiverHandle()
        {
            _opcode = Crc32.GetCRC32Str(typeof(T).FullName);
        }

        public void Dispose()
        {
            _callback = null;
            GC.SuppressFinalize(this);
        }

        public void ReceiveHandle(IChannel channel, uint opcode, MemoryStream memoryStream)
        {
            if (_opcode != opcode)
            {
                return;
            }

            _callback?.Invoke(Serializer.Deserialize<T>(memoryStream));
        }
    }
}