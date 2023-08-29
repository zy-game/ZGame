using System.Collections;
using System.IO;
using ProtoBuf;

namespace ZEngine.Network
{
    public interface IWriteMessageExecuteHandle : IExecuteHandle<IWriteMessageExecuteHandle>
    {
        IChannel channel { get; }
        IMessagePackage write { get; }
    }


    public interface IRecvieMessageExecuteHandle<T> : IExecuteHandle<T> where T : IMessagePackage
    {
        IChannel channel { get; }
        T message { get; }

        internal static IRecvieMessageExecuteHandle<T> Create()
        {
            return Engine.Class.Loader<InternalRecvieMessageExecuteHandle>();
        }

        class InternalRecvieMessageExecuteHandle : AbstractExecuteHandle, IRecvieMessageExecuteHandle<T>
        {
            protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
            {
                channel = (IChannel)paramsList[0];
                message = (T)paramsList[1];
                yield break;
            }

            public IChannel channel { get; set; }
            public T message { get; set; }
        }
    }


    class InternalWriteMessageExecuteHandle : AbstractExecuteHandle, IWriteMessageExecuteHandle
    {
        public IChannel channel { get; set; }
        public IMessagePackage write { get; set; }

        protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
        {
            channel = (IChannel)paramsList[0];
            write = (IMessagePackage)paramsList[1];
            MemoryStream memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, write);
            channel.WriteAndFlush(memoryStream.ToArray());
            yield break;
        }
    }
}