namespace ZGame.Resource
{
    public interface IModuleOptions : IOptions
    {
    }

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
        IModuleOptions module { get; }
        IPackageManifest[] packages { get; }
    }

    public interface IResourceSystem : ISystem
    {
        /// <summary>
        /// 检查资源组更新
        /// </summary>
        /// <param name="group"></param>
        /// <param name="eventPipeline"></param>
        void CheckResourceGroupStatus(IModuleOptions options, IEvent<ICheckResourceGroupStatusResult> eventPipeline);

        /// <summary>
        /// 更新资源组
        /// </summary>
        /// <param name="checkResourceGroupStatusResult"></param>
        /// <param name="pipeline"></param>
        void UpdateResourceGroup(ICheckResourceGroupStatusResult checkResourceGroupStatusResult, IEvent<IUpdateResourceGroupResult> pipeline);
    }
}