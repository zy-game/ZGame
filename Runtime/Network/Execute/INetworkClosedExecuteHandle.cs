using System.Collections;

namespace ZEngine.Network
{
    public interface INetworkClosedExecuteHandle : IExecuteHandle<INetworkClosedExecuteHandle>
    {
        IChannel channel { get; }

        internal static INetworkClosedExecuteHandle Create(IChannel channel)
        {
            InternalNetworkClosedExecuteHandle internalNetworkClosedExecuteHandle = Engine.Class.Loader<InternalNetworkClosedExecuteHandle>();
            internalNetworkClosedExecuteHandle.channel = channel;
            return internalNetworkClosedExecuteHandle;
        }

        class InternalNetworkClosedExecuteHandle : AbstractExecuteHandle, INetworkClosedExecuteHandle
        {
            public IChannel channel { get; set; }

            protected override IEnumerator ExecuteCoroutine()
            {
                if (channel is null)
                {
                    yield break;
                }

                channel.Close();
                yield return WaitFor.Create(() => channel.connected == false);
            }
        }
    }
}