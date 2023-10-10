using System;
using System.IO;
using ProtoBuf.Meta;

namespace ZEngine.Network
{
    public interface IMessageRecvierHandle : IDisposable
    {
        void OnHandleMessage(string address, uint opcode, MemoryStream bodyData);

        public static IMessageRecvierHandle Create(Type type, ISubscriber subscriber)
        {
            InternalMessageRecvieHandle internalMessageRecvieHandle = Activator.CreateInstance<InternalMessageRecvieHandle>();
            internalMessageRecvieHandle.SetSubscribe(type, subscriber);
            return internalMessageRecvieHandle;
        }

        class InternalMessageRecvieHandle : IMessageRecvierHandle
        {
            private uint crc;
            private Type msgType;
            private ISubscriber subscriber;

            public void Dispose()
            {
                crc = 0;
                subscriber.Dispose();
                subscriber = null;
                GC.SuppressFinalize(this);
            }

            public void SetSubscribe(Type type, ISubscriber subscriber)
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

                IMessaged message = (IMessaged)RuntimeTypeModel.Default.Deserialize(bodyData, null, msgType);
                subscriber.Execute(message);
            }
        }
    }
}