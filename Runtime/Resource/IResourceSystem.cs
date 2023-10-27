using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Resource
{
    public interface IFileInfoOptions : IOptions
    {
        string path { get; }
    }

    public interface IPackageManifest : IOptions
    {
        IFileInfoOptions[] files { get; }
    }

    public interface IPackageGroupOptions : IOptions
    {
        IPackageManifest[] packages { get; }
    }

    public interface ILoadResourceObjectResult : IRequest
    {
        UnityEngine.Object asset { get; }
    }

    public static class Extension
    {
    }

    public interface IResourceSystem : IManager
    {

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        ILoadResourceObjectResult LoadAsset(string path);

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        UniTask<ILoadResourceObjectResult> LoadAssetAsync(string path);

        /// <summary>
        /// 回收资源对象
        /// </summary>
        /// <param name="obj"></param>
        void Release(UnityEngine.Object obj);
    }
}