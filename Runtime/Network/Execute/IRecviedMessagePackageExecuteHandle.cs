using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf.Meta;

namespace ZEngine.Network
{
    public interface IRecviedMessagePackageExecuteHandle : IExecuteHandle<IRecviedMessagePackageExecuteHandle>
    {
        IChannel channel { get; }
        IMessagePackage message { get; }
        byte[] bytes { get; }

        IWriteMessageExecuteHandle WriteAndFlush(IMessagePackage messagePackage)
        {
            return Engine.Network.WriteAndFlush(channel.address, messagePackage);
        }

        IWriteMessageExecuteHandle<T> WriteAndFlush<T>(IMessagePackage messagePackage) where T : IMessagePackage
        {
            return Engine.Network.WriteAndFlush<T>(channel.address, messagePackage);
        }
    }

    public class InternalRecviedMessagePackageExecuteHandle : AbstractExecuteHandle, IRecviedMessagePackageExecuteHandle
    {
        public IChannel channel { get; set; }
        public IMessagePackage message { get; set; }
        public byte[] bytes { get; set; }

        protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
        {
            channel = (IChannel)paramsList[0];
            bytes = (byte[])paramsList[1];
            Type msgType = (Type)paramsList[2];
            ISubscribeHandle<IRecviedMessagePackageExecuteHandle> handle = (ISubscribeHandle<IRecviedMessagePackageExecuteHandle>)paramsList[3];
            MemoryStream memoryStream = new MemoryStream(bytes, sizeof(uint), bytes.Length - sizeof(uint));
            message = (IMessagePackage)RuntimeTypeModel.Default.Deserialize(memoryStream, null, msgType);
            handle.Execute(this);
            yield break;
        }
    }
}