using System.Collections;
using System.Collections.Generic;

namespace ZEngine.Network
{
    public interface IRecviedMessagePackageExecuteHandle : IExecuteHandle<IRecviedMessagePackageExecuteHandle>
    {
        IChannel channel { get; }
        IMessagePackage message { get; }

        IWriteMessageExecuteHandle WriteAndFlush(IMessagePackage messagePackage)
        {
            return Engine.Network.WriteAndFlush(channel.address, messagePackage);
        }

        IWriteMessageExecuteHandle<T> WriteAndFlush<T>(IMessagePackage messagePackage) where T : IMessagePackage
        {
            return Engine.Network.WriteAndFlush<T>(channel.address, messagePackage);
        }
    }

    public class RecviedMessagePackageExecuteHandle : ExecuteHandle, IRecviedMessagePackageExecuteHandle
    {
        public IChannel channel { get; }
        public IMessagePackage message { get; }

        public override void Execute(params object[] paramsList)
        {
        }

        private IEnumerator OnStartExecuteMessage()
        {
            yield break;
        }

        protected override void OnComplete()
        {
        }
    }
}