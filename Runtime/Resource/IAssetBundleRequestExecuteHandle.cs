namespace ZEngine.Resource
{
    /// <summary>
    /// 资源包加载
    /// </summary>
    public interface IAssetBundleRequestExecuteHandle : IExecuteHandle<IAssetBundleRequestExecuteHandle>, IAssetBundleRequestResult
    {
        void ObserverPorgress(ISubscribe<float> subscribe);
    }
}