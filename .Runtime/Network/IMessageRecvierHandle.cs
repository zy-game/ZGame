using System;
using System.IO;
using System.Linq;
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

        public static IMessageRecvierHandle Create(Type recvieHandleType)
        {
            if (typeof(IMessageRecvierHandle).IsAssignableFrom(recvieHandleType) is false)
            {
                ZGame.Console.Error(new NotImplementedException(recvieHandleType.FullName));
                return default;
            }

            IMessageRecvierHandle messageRecvierHandle = ZGame.Datable.GetDatable<IMessageRecvierHandle>(x => x.GetType().Equals(recvieHandleType));
            if (messageRecvierHandle is not null)
            {
                ZGame.Console.Error("已存在消息处理管道", recvieHandleType);
                return default;
            }

            messageRecvierHandle = Activator.CreateInstance(recvieHandleType) as IMessageRecvierHandle;
            ZGame.Datable.Add(messageRecvierHandle);
            return messageRecvierHandle;
        }

        public static void Release(Type recvieHandleType)
        {
            if (typeof(IMessageRecvierHandle).IsAssignableFrom(recvieHandleType) is false)
            {
                ZGame.Console.Error(new NotImplementedException(recvieHandleType.FullName));
                return;
            }

            IMessageRecvierHandle messageRecvierHandle = ZGame.Datable.GetDatable<IMessageRecvierHandle>(x => x.GetType().Equals(recvieHandleType));
            if (messageRecvierHandle is null)
            {
                ZGame.Console.Error("不存在消息处理管道", recvieHandleType);
                return;
            }

            ZGame.Datable.Release(messageRecvierHandle);
        }

        public static void PublishMessage(IChannel channel, byte[] bytes)
        {
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