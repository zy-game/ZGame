namespace ZGame.Resource
{


    public interface IPackageFileInfoOptions : IOptions
    {
        string name { get; }
        string path { get; }
    }

    public interface IPackageManifest : IRuntimeOptions
    {
        IPackageFileInfoOptions[] files { get; }
    }

    public interface ICheckResourceGroupStatusResult : IRequest<ICheckResourceGroupStatusResult>
    {
        IPackageManifest[] packages { get; }
    }

    public interface IUpdateResourceGroupResult : IRequest<IUpdateResourceGroupResult>
    {
        string module { get; }
        IPackageManifest[] packages { get; }
    }

    public interface IResourceSystem : ISystem
    {
        /// <summary>
        /// 检查资源组更新
        /// </summary>
        /// <param name="module"></param>
        /// <param name="eventPipeline"></param>
        void CheckResourceGroupStatus(string module, IEvent<ICheckResourceGroupStatusResult> eventPipeline);

        /// <summary>
        /// 更新资源组
        /// </summary>
        /// <param name="checkResourceGroupStatusResult"></param>
        /// <param name="pipeline"></param>
        void UpdateResourceGroup(ICheckResourceGroupStatusResult checkResourceGroupStatusResult, IEvent<IUpdateResourceGroupResult> pipeline);
    }
}