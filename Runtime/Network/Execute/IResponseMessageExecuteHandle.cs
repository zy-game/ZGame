using System;
using System.Collections;

namespace ZEngine.Network
{
    public interface IResponseMessageExecuteHandle<T> : IExecuteHandle<T> where T : IMessaged
    {
        IChannel channel { get; }
        T message { get; }

        internal static IResponseMessageExecuteHandle<T> Create()
        {
            return Activator.CreateInstance<WaitingResponseMessageExecuteHandle>();
        }

        internal class WaitingResponseMessageExecuteHandle : AbstractExecuteHandle, IResponseMessageExecuteHandle<T>
        {
            public IChannel channel { get; }
            public T message { get; }

            protected override IEnumerator ExecuteCoroutine()
            {
                yield break;
            }
        }
    }
}