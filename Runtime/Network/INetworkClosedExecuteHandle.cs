namespace ZEngine.Network
{
    public interface INetworkClosedExecuteHandle : IExecuteHandle<INetworkClosedExecuteHandle>
    {
        IChannel channel { get; }
    }

    class InternalNetworkClosedExecuteHandle:ExecuteHandle,INetworkClosedExecuteHandle
    {
        public override void Execute(params object[] paramsList)
        {
            
        }

        public IChannel channel { get; }
    }
}