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

    class InternalWriteMessageExecuteHandle : ExecuteHandle, IWriteMessageExecuteHandle
    {
        public IChannel channel { get; set; }
        public IMessagePackage write { get; set; }

        public override void Execute(params object[] paramsList)
        {
            channel = (IChannel)paramsList[0];
            write = (IMessagePackage)paramsList[1];
            MemoryStream memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, write);
            channel.WriteAndFlush(memoryStream.ToArray());
        }
    }

    class InternalWriteMessageExecuteHandle<T> : InternalWriteMessageExecuteHandle, IWriteMessageExecuteHandle<T> where T : IMessagePackage
    {
        public T response { get; set; }

        public override void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            Engine.Network.SubscribeMessagePackage<T>(Response);
            base.Execute(paramsList);
        }

        private void Response(IRecviedMessagePackageExecuteHandle iRecviedMessagePackageExecuteHandle)
        {
            response = (T)iRecviedMessagePackageExecuteHandle.message;
            Engine.Network.UnsubscribeMessagePackage<T>(Response);
            OnComplete();
        }
    }
}