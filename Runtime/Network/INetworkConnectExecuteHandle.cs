namespace ZEngine.Network
{
    public interface INetworkConnectExecuteHandle : IExecuteHandle<INetworkConnectExecuteHandle>
    {
        string address { get; }
        IChannel channel { get; }
    }

    class InternalNetworkConnectExecuteHandle:ExecuteHandle,INetworkConnectExecuteHandle
    {
        public override void Execute(params object[] paramsList)
        {
            throw new System.NotImplementedException();
        }

        public string address { get; }
        public IChannel channel { get; }
    }
}