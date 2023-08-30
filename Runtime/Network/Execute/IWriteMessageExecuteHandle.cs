using System.Collections;
using System.IO;
using ProtoBuf;

namespace ZEngine.Network
{
    public interface IWriteMessageExecuteHandle : IExecuteHandle<IWriteMessageExecuteHandle>
    {
        IChannel channel { get; }
        IMessagePacket message { get; }

        internal static IWriteMessageExecuteHandle Create(IChannel channel, IMessagePacket messagePackage)
        {
            InternalWriteMessageExecuteHandle internalWriteMessageExecuteHandle = Engine.Class.Loader<InternalWriteMessageExecuteHandle>();
            internalWriteMessageExecuteHandle.channel = channel;
            internalWriteMessageExecuteHandle.message = messagePackage;
            return internalWriteMessageExecuteHandle;
        }

        class InternalWriteMessageExecuteHandle : AbstractExecuteHandle, IWriteMessageExecuteHandle
        {
            public IChannel channel { get; set; }
            public IMessagePacket message { get; set; }

            protected override IEnumerator ExecuteCoroutine()
            {

                IWriteResult writeResult = channel.WriteAndFlush(message);
                yield return WaitFor.Create(() => writeResult.status == Status.Failed || writeResult.status == Status.Success);
            }
        }
    }
}