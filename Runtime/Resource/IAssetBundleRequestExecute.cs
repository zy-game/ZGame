namespace ZEngine.Resource
{
    /// <summary>
    /// 资源包加载
    /// </summary>
    public interface IAssetBundleRequestExecute : IExecute<IRuntimeBundleManifest>, IAssetBundleRequestResult<IRuntimeBundleManifest>
    {
    }
}