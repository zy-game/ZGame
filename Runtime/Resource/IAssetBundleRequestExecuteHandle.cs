namespace ZEngine.Resource
{
    /// <summary>
    /// 资源包加载
    /// </summary>
    public interface IAssetBundleRequestExecuteHandle : IExecuteHandle<IAssetBundleRequestExecuteHandle>, IAssetBundleRequestResult<IRuntimeBundleManifest>
    {
        void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe);
    }
}