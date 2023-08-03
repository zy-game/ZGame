namespace ZEngine.Resource
{
    /// <summary>
    /// 检查资源更新
    /// </summary>
    public interface ICheckUpdateExecuteHandle : IExecuteHandle<ICheckUpdateExecuteHandle>
    {
        ulong length { get; }
        string[] files { get; }
        void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe);
        void OnUpdateDialog(IUpdateResourceDialogExecuteHandle dialogExecuteHandle);
    }

    public interface IUpdateResourceDialogExecuteHandle : IExecuteHandle<IUpdateResourceDialogExecuteHandle>
    {
        Switch isUpdate { get; }
    }
}