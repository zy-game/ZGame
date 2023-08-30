using System.Collections;

namespace ZEngine.Network
{
    public interface INetworkConnectExecuteHandle : IExecuteHandle<INetworkConnectExecuteHandle>
    {
        int id { get; }
        string address { get; }
        IChannel channel { get; }

        internal static INetworkConnectExecuteHandle Create(string address, int id = 0)
        {
            InternalNetworkConnectExecuteHandle internalNetworkConnectExecuteHandle = Engine.Class.Loader<InternalNetworkConnectExecuteHandle>();
            internalNetworkConnectExecuteHandle.address = address;
            internalNetworkConnectExecuteHandle.id = id;
            return internalNetworkConnectExecuteHandle;
        }

        class InternalNetworkConnectExecuteHandle : AbstractExecuteHandle, INetworkConnectExecuteHandle
        {
            public int id { get; set; }
            public string address { get; set; }
            public IChannel channel { get; set; }


            protected override IEnumerator ExecuteCoroutine()
            {
                if (channel is null || address.IsNullOrEmpty())
                {
                    status = Status.Failed;
                    yield break;
                }

                if (address.StartsWith("ws") || address.StartsWith("wss"))
                {
                    channel = Engine.Class.Loader<WebSocket>();
                    channel.Connect(address);
                }
                else
                {
                    //todo 等后面接入TCP KCP之后在写    
                    KCPChannel kcpChannel = Engine.Class.Loader<KCPChannel>();
                    kcpChannel.SetConv(id);
                    kcpChannel.Connect(address);
                }

                yield return WaitFor.Create(() => channel.connected is true);
                status = channel.connected ? Status.Success : Status.Failed;
            }
        }
    }
}