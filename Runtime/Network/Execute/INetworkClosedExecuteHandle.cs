using System.Collections;

namespace ZEngine.Network
{
    public interface INetworkClosedExecuteHandle : IExecuteHandle<INetworkClosedExecuteHandle>
    {
        IChannel channel { get; }
    }

    class InternalNetworkClosedExecuteHandle : ExecuteHandle, INetworkClosedExecuteHandle
    {
        public IChannel channel { get; set; }

        public override void Execute(params object[] paramsList)
        {
            channel = (IChannel)paramsList[0];
        }

        private IEnumerator OnStartClosed()
        {
            if (channel is null)
            {
                yield break;
            }

            channel.Close();
            yield return WaitFor.Create(() => channel.connected == false);
            OnComplete();
        }
    }
}