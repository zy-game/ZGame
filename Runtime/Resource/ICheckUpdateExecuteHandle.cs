namespace ZEngine.Resource
{
    /// <summary>
    /// 检查资源更新
    /// </summary>
    public interface ICheckUpdateExecuteHandle : IExecuteHandle<ICheckUpdateExecuteHandle>
    {
        float progress { get; }
        RuntimeBundleManifest[] bundles { get; }
        void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe);
    }

    public interface IUpdateResourceExecuteHandle : IExecuteHandle<IUpdateResourceExecuteHandle>
    {
        float progress { get; }
        RuntimeBundleManifest[] bundles { get; }
        void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe);
    }

    /// <summary>
    /// 资源预加载
    /// </summary>
    public interface IResourcePreloadExecuteHandle : IExecuteHandle<IResourcePreloadExecuteHandle>
    {
        void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe);
        void OnDialog(IDialogHandle<Switch> dialog);
    }
}