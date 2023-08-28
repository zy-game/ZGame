using System.Collections;

namespace ZEngine.Network
{
    public interface INetworkConnectExecuteHandle : IExecuteHandle<INetworkConnectExecuteHandle>
    {
        string address { get; }
        IChannel channel { get; }
    }

    class InternalNetworkConnectExecuteHandle : AbstractExecuteHandle, INetworkConnectExecuteHandle
    {
        public string address { get; set; }
        public IChannel channel { get; set; }



        protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
        {
            address = paramsList[0].ToString();
            if (address.StartsWith("ws") || address.StartsWith("wss"))
            {
                channel = Engine.Class.Loader<WebSocket>();
                channel.Connect(address);
            }

            //todo 等后面接入TCP KCP之后在写
            if (channel is null)
            {
                yield break;
            }

            yield return WaitFor.Create(() => channel.connected is true);
        }
    }
}