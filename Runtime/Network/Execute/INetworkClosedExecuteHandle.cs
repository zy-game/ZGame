using System.Collections;

namespace ZEngine.Network
{
    public interface INetworkClosedExecuteHandle : IExecuteHandle<INetworkClosedExecuteHandle>
    {
        IChannel channel { get; }
    }

    class InternalNetworkClosedExecuteHandle : AbstractExecuteHandle, INetworkClosedExecuteHandle
    {
        public IChannel channel { get; set; }

        protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
        {
            channel = (IChannel)paramsList[0];
            if (channel is null)
            {
                yield break;
            }

            channel.Close();
            yield return WaitFor.Create(() => channel.connected == false);
        }
    }
}