using System;
using System.IO;
using ProtoBuf.Meta;

namespace ZEngine.Network
{
    public interface IMessageRecvierHandle : IDisposable
    {
        void OnHandleMessage(string address, uint opcode, MemoryStream bodyData);

        public static IMessageRecvierHandle Create(Type type, Action<IMessaged> subscriber)
        {
            InternalMessageRecvieHandle internalMessageRecvieHandle = Activator.CreateInstance<InternalMessageRecvieHandle>();
            internalMessageRecvieHandle.SetSubscribe(type, subscriber);
            return internalMessageRecvieHandle;
        }

        class InternalMessageRecvieHandle : IMessageRecvierHandle
        {
            private uint crc;
            private Type msgType;
            private Action<IMessaged> subscriber;

            public void Dispose()
            {
                crc = 0;
                subscriber = null;
                GC.SuppressFinalize(this);
            }

            public void SetSubscribe(Type type, Action<IMessaged> subscriber)
            {
                crc = Crc32.GetCRC32Str(type.FullName);
                this.subscriber = subscriber;
            }

            public void OnHandleMessage(string address, uint opcode, MemoryStream bodyData)
            {
                if (crc != opcode)
                {
                    return;
                }

                subscriber?.Invoke((IMessaged)RuntimeTypeModel.Default.Deserialize(bodyData, null, msgType));
            }
        }
    }
}