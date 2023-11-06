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
        private List<AssetBundleHandle> _handles = new List<AssetBundleHandle>();
        private string address;

        public ResourceManager(string rootUrl)
        {
            this.address = rootUrl;
            ResourcesLodaingPipeline = new ResourcesLoadingPipeline();
            NetworkResourceLoadingPipeline = new NetworkResourceLoadingPipeline();
            AssetDatabaseResourceLoadingPipeline = new AssetDatabaseLoadingPipeline();
            AssetBundleResourceLoadingPipeline = new AssetBundleResourceLoadingPipeline();
            PackageUpdatePipeline = new DefaultPackageUpdatePipeline();
            PackageLoadingPipeline = new DefaultPackageLoadingPipeline();
        }


        internal AssetBundleHandle FindAssetBundleHandle(AssetObjectHandle obj)
        {
            return _handles.Find(x => x.Contains(obj));
        }

        internal AssetBundleHandle FindAssetBundleHandle(string path)
        {
            return _handles.Find(x => x.Contains(path));
        }

        internal void RemoveAssetBundleHandle(string name)
        {
            AssetBundleHandle handle = _handles.Find(x => x.name == name);
            if (handle is null)
            {
                return;
            }

            _handles.Remove(handle);
        }

        internal void AddAssetBundleHandle(AssetBundle bundle)
        {
            _handles.Add(new AssetBundleHandle(bundle));
        }

        /// <summary>
        /// 获取文件资源地址
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public string GetNetworkResourceUrl(string fileName)
        {
            return $"{address}/{Engine.GetPlatformName()}/{fileName}";
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        public AssetObjectHandle LoadAsset(string path)
        {
            if (path.StartsWith("Resources"))
            {
                if (ResourcesLodaingPipeline is null)
                {
                    Debug.LogError(new NullReferenceException(nameof(ResourcesLodaingPipeline)));
                    return default;
                }

                return ResourcesLodaingPipeline.LoadAsset(path);
            }

            if (path.StartsWith("http"))
            {
                Debug.LogError("网络资源必须使用异步加载！");
                return default;
            }
#if UNITY_EDITOR
            if (Engine.IsHotfix is false)
            {
                if (AssetDatabaseResourceLoadingPipeline is null)
                {
                    Debug.LogError(new NullReferenceException(nameof(AssetDatabaseResourceLoadingPipeline)));
                    return default;
                }

                return AssetDatabaseResourceLoadingPipeline.LoadAsset(path);
            }
#endif
            if (AssetBundleResourceLoadingPipeline is null)
            {
                Debug.LogError(new NullReferenceException(nameof(AssetBundleResourceLoadingPipeline)));
                return default;
            }

            return AssetBundleResourceLoadingPipeline.LoadAsset(path);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        public async UniTask<AssetObjectHandle> LoadAssetAsync(string path)
        {
            if (path.StartsWith("Resources"))
            {
                if (NetworkResourceLoadingPipeline is null)
                {
                    Debug.LogError(new NullReferenceException(nameof(NetworkResourceLoadingPipeline)));
                    return default;
                }

                return await ResourcesLodaingPipeline.LoadAssetAsync(path);
            }

            if (path.StartsWith("http"))
            {
                if (NetworkResourceLoadingPipeline is null)
                {
                    Debug.LogError(new NullReferenceException(nameof(NetworkResourceLoadingPipeline)));
                    return default;
                }

                return await NetworkResourceLoadingPipeline.LoadAssetAsync(path);
            }
#if UNITY_EDITOR
            if (Engine.IsHotfix is false)
            {
                if (AssetDatabaseResourceLoadingPipeline is null)
                {
                    Debug.LogError(new NullReferenceException(nameof(AssetDatabaseResourceLoadingPipeline)));
                    return default;
                }

                return await AssetDatabaseResourceLoadingPipeline.LoadAssetAsync(path);
            }
#endif
            if (AssetBundleResourceLoadingPipeline is null)
            {
                Debug.LogError(new NullReferenceException(nameof(AssetBundleResourceLoadingPipeline)));
                return default;
            }

            return await AssetBundleResourceLoadingPipeline.LoadAssetAsync(path);
        }

        /// <summary>
        /// 加载资源模块
        /// </summary>
        /// <param name="groupName">资源组名</param>
        /// <param name="progressUpdate">加载进度会回调</param>
        /// <returns>资源列表加载任务</returns>
        public async UniTask LoadingResourcePackageList(string groupName, Action<float> progressUpdate)
        {
#if UNITY_EDITOR
            if (Engine.IsHotfix is false)
            {
                progressUpdate?.Invoke(1);
                return;
            }
#endif
            if (groupName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("groupName");
            }

            ResourceModuleManifest manifest = await ResourceModuleManifest.Find(groupName);
            if (manifest is null)
            {
                Debug.LogError(new ResourceModuleNotFoundException(groupName));
                return;
            }

            if (PackageLoadingPipeline is null)
            {
                throw new NullReferenceException(nameof(PackageLoadingPipeline));
                return;
            }

            await PackageLoadingPipeline.LoadingModulePackageList(manifest, progressUpdate);
        }

        /// <summary>
        /// 加载资源包列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async UniTask LoadingOhterPackageList(Action<float> progressCallback, params string[] args)
        {
            if (PackageLoadingPipeline is null)
            {
                throw new NullReferenceException(nameof(PackageLoadingPipeline));
            }

            await PackageLoadingPipeline.LoadingPackageList(progressCallback, args);
        }

        /// <summary>
        /// 更新资源组
        /// </summary>
        /// <param name="groupName">资源组名</param>
        /// <param name="progressUpdate">更新进度会回调</param>
        /// <returns>资源更新任务</returns>
        public async UniTask UpdateResourcePackageList(string groupName, Action<float> progressUpdate)
        {
#if UNITY_EDITOR
            if (Engine.IsHotfix is false)
            {
                progressUpdate?.Invoke(1);
                return;
            }
#endif
            if (groupName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("groupName");
            }

            ResourceModuleManifest manifest = await ResourceModuleManifest.Find(groupName);
            if (manifest is null)
            {
                throw new ResourceModuleNotFoundException(groupName);
            }

            if (PackageUpdatePipeline is null)
            {
                throw new NullReferenceException(nameof(PackageUpdatePipeline));
            }

            await PackageUpdatePipeline.StartUpdate(manifest, progressUpdate);
        }

        /// <summary>
        /// 更新资源列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async UniTask UpdateResourceList(Action<float> progressCallback, params string[] args)
        {
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
        public void Release(AssetObjectHandle obj)
        {
            ResourcesLodaingPipeline?.Release(obj);
            AssetBundleResourceLoadingPipeline?.Release(obj);
            AssetDatabaseResourceLoadingPipeline?.Release(obj);
        }

        public void Dispose()
        {
            ResourcesLodaingPipeline?.Dispose();
            AssetBundleResourceLoadingPipeline?.Dispose();
            NetworkResourceLoadingPipeline?.Dispose();
            AssetDatabaseResourceLoadingPipeline?.Dispose();
            PackageUpdatePipeline?.Dispose();
            PackageLoadingPipeline?.Dispose();

            ResourcesLodaingPipeline = null;
            NetworkResourceLoadingPipeline = null;
            AssetBundleResourceLoadingPipeline = null;
            AssetDatabaseResourceLoadingPipeline = null;
            PackageUpdatePipeline = null;
            PackageLoadingPipeline = null;
        }
    }
}