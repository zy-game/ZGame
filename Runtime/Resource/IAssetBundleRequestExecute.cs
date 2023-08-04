namespace ZEngine.Resource
{
    /// <summary>
    /// 资源包加载结果
    /// </summary>
    public interface IAssetBundleRequestResult : IReference
    {
        string name { get; }
        string path { get; }
        string module { get; }
        VersionOptions version { get; }
        IRuntimeBundleHandle bundle { get; }
    }

    /// <summary>
    /// 资源包加载
    /// </summary>
    public interface IAssetBundleRequestExecute : IExecute<IAssetBundleRequestResult>
    {
    }

    /// <summary>
    /// 资源包加载
    /// </summary>
    public interface IAssetBundleRequestExecuteHandle : IExecuteHandle<IAssetBundleRequestResult>, IAssetBundleRequestResult
    {
        void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe);
    }
}