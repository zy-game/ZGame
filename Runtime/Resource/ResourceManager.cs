using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.FileSystem;
using ZGame.Networking;

namespace ZGame.Resource
{
    public sealed class ResourceManager : IManager
    {
        public IAssetLoadingPipeline ResourcesLodaingPipeline;
        public IAssetLoadingPipeline NetworkResourceLoadingPipeline;
        public IAssetLoadingPipeline AssetBundleResourceLoadingPipeline;
        public IAssetLoadingPipeline AssetDatabaseResourceLoadingPipeline;
        public IPackageUpdatePipeline PackageUpdatePipeline;
        public IPackageLoadingPipeline PackageLoadingPipeline;

        public ResourceManager()
        {
            ResourcesLodaingPipeline = new ResourcesLoadingPipeline();
            NetworkResourceLoadingPipeline = new NetworkResourceLoadingPipeline();
            AssetDatabaseResourceLoadingPipeline = new AssetDatabaseLoadingPipeline();
            AssetBundleResourceLoadingPipeline = new AssetBundleResourceLoadingPipeline();
            PackageUpdatePipeline = new DefaultPackageUpdatePipeline();
            PackageLoadingPipeline = new DefaultPackageLoadingPipeline();
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        public ResObject LoadAsset(string path)
        {
            if (path.StartsWith("Resources"))
            {
                return ResourcesLodaingPipeline.LoadAsset(path);
            }

            if (path.StartsWith("http"))
            {
                return NetworkResourceLoadingPipeline.LoadAsset(path);
            }
#if UNITY_EDITOR
            if (CoreApi.IsHotfix is false)
            {
                return AssetDatabaseResourceLoadingPipeline.LoadAsset(path);
            }
#endif
            return AssetBundleResourceLoadingPipeline.LoadAsset(path);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        public async UniTask<ResObject> LoadAssetAsync(string path)
        {
            if (path.StartsWith("Resources"))
            {
                return await ResourcesLodaingPipeline.LoadAssetAsync(path);
            }

            if (path.StartsWith("http"))
            {
                return await NetworkResourceLoadingPipeline.LoadAssetAsync(path);
            }
#if UNITY_EDITOR
            if (CoreApi.IsHotfix is false)
            {
                return await AssetDatabaseResourceLoadingPipeline.LoadAssetAsync(path);
            }
#endif
            return await AssetBundleResourceLoadingPipeline.LoadAssetAsync(path);
        }

        /// <summary>
        /// 加载包列表
        /// </summary>
        /// <param name="groupName">资源组名</param>
        /// <param name="progressUpdate">加载进度会回调</param>
        /// <returns>资源列表加载任务</returns>
        public async UniTask<ErrorCode> LoadingResourcePackageList(string groupName, Action<float> progressUpdate)
        {
#if UNITY_EDITOR
            if (CoreApi.IsHotfix is false)
            {
                progressUpdate?.Invoke(1);
                return ErrorCode.OK;
            }
#endif
            if (groupName.IsNullOrEmpty())
            {
                Debug.LogError(new ArgumentNullException("groupName"));
                return ErrorCode.PARAMETER_IS_EMPTY;
            }

            ResourceModuleManifest manifest = await ResourceModuleManifest.Find(groupName);
            if (manifest is null)
            {
                return ErrorCode.NOT_FIND;
            }

            return await PackageUpdatePipeline.StartUpdate(manifest, progressUpdate);
        }

        /// <summary>
        /// 更新资源组
        /// </summary>
        /// <param name="groupName">资源组名</param>
        /// <param name="progressUpdate">更新进度会回调</param>
        /// <returns>资源更新任务</returns>
        public async UniTask<ErrorCode> UpdateResourcePackageList(string groupName, Action<float> progressUpdate)
        {
#if UNITY_EDITOR
            if (CoreApi.IsHotfix is false)
            {
                progressUpdate?.Invoke(1);
                return ErrorCode.OK;
            }
#endif
            if (groupName.IsNullOrEmpty())
            {
                Debug.LogError(new ArgumentNullException("groupName"));
                return ErrorCode.PARAMETER_IS_EMPTY;
            }

            ResourceModuleManifest manifest = await ResourceModuleManifest.Find(groupName);
            if (manifest is null)
            {
                Debug.LogError("没有找到指定的资源组");
                return ErrorCode.NOT_FIND;
            }

            return await PackageLoadingPipeline.LoadingModulePackageList(progressUpdate, manifest);
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="obj"></param>
        public void Release(ResObject obj)
        {
            ResourcesLodaingPipeline.Release(obj);
            AssetBundleResourceLoadingPipeline.Release(obj);
            AssetDatabaseResourceLoadingPipeline.Release(obj);
        }

        public void Dispose()
        {
            ResourcesLodaingPipeline.Dispose();
            AssetBundleResourceLoadingPipeline.Dispose();
            NetworkResourceLoadingPipeline.Dispose();
            AssetDatabaseResourceLoadingPipeline.Dispose();
            PackageUpdatePipeline.Dispose();
            PackageLoadingPipeline.Dispose();

            ResourcesLodaingPipeline = null;
            NetworkResourceLoadingPipeline = null;
            AssetBundleResourceLoadingPipeline = null;
            AssetDatabaseResourceLoadingPipeline = null;
            PackageUpdatePipeline = null;
            PackageLoadingPipeline = null;
        }
    }
}