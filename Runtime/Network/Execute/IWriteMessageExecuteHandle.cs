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

    public interface IWriteMessageExecuteHandle<T> : IWriteMessageExecuteHandle, IExecuteHandle<IWriteMessageExecuteHandle<T>> where T : IMessagePackage
    {
        T response { get; }
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

    class InternalWriteMessageExecuteHandle<T> : InternalWriteMessageExecuteHandle, IWriteMessageExecuteHandle<T> where T : IMessagePackage
    {
        public T response { get; set; }

        protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
        {
            Engine.Network.SubscribeMessagePackage<T>(Response);
            yield return base.ExecuteCoroutine(paramsList);
        }

        private void Response(IRecviedMessagePackageExecuteHandle iRecviedMessagePackageExecuteHandle)
        {
            response = (T)iRecviedMessagePackageExecuteHandle.message;
            Engine.Network.UnsubscribeMessagePackage<T>(Response);
        }
    }
}