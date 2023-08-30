using System.Collections;

namespace ZEngine.Network
{
    public interface IRecvieMessageExecuteHandle<T> : IExecuteHandle<T> where T : IMessagePacket
    {
        IChannel channel { get; }
        T message { get; }

        void OnResponse(T message);

        internal static IRecvieMessageExecuteHandle<T> Create(IChannel channel)
        {
            InternalRecvieMessageExecuteHandle internalRecvieMessageExecuteHandle = Engine.Class.Loader<InternalRecvieMessageExecuteHandle>();
            internalRecvieMessageExecuteHandle.channel = channel;
            return internalRecvieMessageExecuteHandle;
        }

        class InternalRecvieMessageExecuteHandle : AbstractExecuteHandle, IRecvieMessageExecuteHandle<T>
        {
            protected override IEnumerator ExecuteCoroutine()
            {
                yield break;
            }

            public void OnResponse(T message)
            {
                this.message = message;
            }

            public IChannel channel { get; set; }
            public T message { get; set; }
        }
    }
}