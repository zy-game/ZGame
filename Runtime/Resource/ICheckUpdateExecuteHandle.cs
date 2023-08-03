namespace ZEngine.Resource
{
    /// <summary>
    /// 检查资源更新
    /// </summary>
    public interface ICheckUpdateExecuteHandle : IExecuteHandle<ICheckUpdateExecuteHandle>
    {
        void ObserverPorgress(ISubscribeExecuteHandle<float> subscribe);
    }
}