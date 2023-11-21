using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.FileSystem;
using ZGame.Networking;

namespace ZGame.Resource
{
    public sealed class ResourceManager : SingletonBehaviour<ResourceManager>
    {
        private IAssetLoadingPipeline ResourcesLodaingPipeline;
        private IPackageUpdatePipeline PackageUpdatePipeline;
        private IPackageLoadingPipeline PackageLoadingPipeline;
        private List<ABHandle> _handles = new List<ABHandle>();
        private string address;

        public ResourceManager()
        {
            ResourcesLodaingPipeline = new AssetObjectLoadingPipeline();
            PackageUpdatePipeline = new CheckResourceUpdatePipeline();
            PackageLoadingPipeline = new AssetBundleLoadingPipeline();
        }

        public void SetupAssetLoader<T>() where T : IAssetLoadingPipeline
        {
        }

        public void SetupAssetBundleLoaderHelper<T>() where T : IPackageLoadingPipeline
        {
        }

        public void SetupResourceUpdateHelper<T>() where T : IPackageUpdatePipeline
        {
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        public ResHandle LoadAsset(string path)
        {
            if (ResourcesLodaingPipeline is null)
            {
                Debug.LogError(new NullReferenceException(nameof(ResourcesLodaingPipeline)));
                return default;
            }

            return ResourcesLodaingPipeline.LoadAsset(path);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        public async UniTask<ResHandle> LoadAssetAsync(string path)
        {
            if (ResourcesLodaingPipeline is null)
            {
                Debug.LogError(new NullReferenceException(nameof(ResourcesLodaingPipeline)));
                return default;
            }

            return await ResourcesLodaingPipeline.LoadAssetAsync(path);
        }

        /// <summary>
        /// 加载资源包列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async UniTask LoadingResourcePackageList(Action<float> progressCallback, params string[] args)
        {
            if (args is null || args.Length == 0)
            {
                throw new ArgumentNullException("args");
            }
#if UNITY_EDITOR
            if (GameSeting.current.runtime == RuntimeMode.Editor)
            {
                progressCallback?.Invoke(1);
                return;
            }
#endif

            if (PackageLoadingPipeline is null)
            {
                throw new NullReferenceException(nameof(PackageLoadingPipeline));
            }

            await PackageLoadingPipeline.LoadingPackageList(progressCallback, args);
        }

        /// <summary>
        /// 更新资源列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async UniTask CheckUpdateResourcePackageList(Action<float> progressCallback, params string[] args)
        {
            if (args is null || args.Length == 0)
            {
                throw new ArgumentNullException("args");
            }
#if UNITY_EDITOR
            if (GameSeting.current.runtime == RuntimeMode.Editor)
            {
                progressCallback?.Invoke(1);
                return;
            }
#endif
            if (PackageUpdatePipeline is null)
            {
                throw new NullReferenceException(nameof(PackageUpdatePipeline));
            }

            await PackageUpdatePipeline.StartUpdate(progressCallback, args);
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="obj"></param>
        public void Release(ResHandle obj)
        {
            ABManager.instance.Release(obj);
        }

        public void Dispose()
        {
            ResourcesLodaingPipeline?.Dispose();
            PackageUpdatePipeline?.Dispose();
            PackageLoadingPipeline?.Dispose();
            _handles.Clear();
            ResourcesLodaingPipeline = null;
            PackageUpdatePipeline = null;
            PackageLoadingPipeline = null;
            _handles = null;
        }
    }
}