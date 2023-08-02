namespace ZEngine.Resource
{
    /// <summary>
    /// 资源预加载
    /// </summary>
    public interface IResourcePreloadExecuteHandle : IExecuteHandle<IResourcePreloadExecuteHandle>
    {
        void ObserverPorgress(ISubscribe<float> subscribe);
    }
}