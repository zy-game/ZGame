using System;

namespace ZGame.Resource
{
    public interface IFileInfoOptions : IOptions
    {
        string name { get; }
        string path { get; }
    }

    public interface IPackageManifest : IOptions
    {
        IFileInfoOptions[] files { get; }
    }

    public interface ICheckResourceGroupStatusResult : IRequest
    {
        IPackageManifest[] packages { get; }
    }

    public interface IUpdateResourceGroupResult : IRequest
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
        void CheckResourceGroupStatus(string module, Action<ICheckResourceGroupStatusResult> eventPipeline);

        /// <summary>
        /// 更新资源组
        /// </summary>
        /// <param name="checkResourceGroupStatusResult"></param>
        /// <param name="pipeline"></param>
        void UpdateResourceGroup(ICheckResourceGroupStatusResult checkResourceGroupStatusResult, Action<IUpdateResourceGroupResult> pipeline);
    }
}