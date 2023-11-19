using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.FileSystem;
using ZGame.Networking;

namespace ZGame.Resource
{
    public enum ResourceMode : byte
    {
        Networking,
        StreamingAssets,
    }

    public enum RuntimeMode : byte
    {
        Editor,
        Simulator,
    }


    public sealed class ResourceManager : IManager
    {
        private IAssetLoadingPipeline ResourcesLodaingPipeline;
        private IAssetLoadingPipeline NetworkResourceLoadingPipeline;
        private IAssetLoadingPipeline AssetBundleResourceLoadingPipeline;
        private IAssetLoadingPipeline AssetDatabaseResourceLoadingPipeline;
        private IPackageUpdatePipeline PackageUpdatePipeline;
        private IPackageLoadingPipeline PackageLoadingPipeline;
        private ResourceMode resouceMode;
        private RuntimeMode runtimeMode;
        private List<AssetBundleHandle> _handles = new List<AssetBundleHandle>();
        private string address;

        public ResourceManager()
        {
            ResourcesLodaingPipeline = new ResourcesLoadingPipeline();
            NetworkResourceLoadingPipeline = new NetworkResourceLoadingPipeline();
            AssetDatabaseResourceLoadingPipeline = new AssetDatabaseLoadingPipeline();
            AssetBundleResourceLoadingPipeline = new AssetBundleResourceLoadingPipeline();
            PackageUpdatePipeline = new DefaultPackageUpdatePipeline();
            PackageLoadingPipeline = new DefaultPackageLoadingPipeline();
        }

        public void SetResourceAddressable(string url)
        {
            this.address = url;
        }

        public void SetResourceMode(ResourceMode mode)
        {
            this.resouceMode = mode;
        }

        public void SetRuntimeMode(RuntimeMode mode)
        {
            this.runtimeMode = mode;
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
            if (runtimeMode == RuntimeMode.Editor)
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
            if (runtimeMode == RuntimeMode.Editor)
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
            if (runtimeMode == RuntimeMode.Editor)
            {
                progressCallback?.Invoke(1);
                return;
            }
#endif

            if (PackageLoadingPipeline is null)
            {
                throw new NullReferenceException(nameof(PackageLoadingPipeline));
            }

            // PackageListManifest[] packageListManifests = new PackageListManifest[args.Length];
            // for (int i = 0; i < args.Length; i++)
            // {
            //     if (args[i].StartsWith("http"))
            //     {
            //         
            //     }
            //     packageListManifests[i] = await PackageListManifest.Find(args[i]);
            // }

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
            if (runtimeMode == RuntimeMode.Editor)
            {
                progressCallback?.Invoke(1);
                return;
            }
#endif
            if (PackageUpdatePipeline is null)
            {
                throw new NullReferenceException(nameof(PackageUpdatePipeline));
            }

            // PackageListManifest[] manifests = new PackageListManifest[args.Length];
            // for (int i = 0; i < manifests.Length; i++)
            // {
            //     if (args[i].StartsWith("http"))
            //     {
            //         
            //     }
            //     manifests[i] = await PackageListManifest.Find(args[i]);
            // }

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
            _handles.Clear();
            ResourcesLodaingPipeline = null;
            AssetBundleResourceLoadingPipeline = null;
            NetworkResourceLoadingPipeline = null;
            AssetDatabaseResourceLoadingPipeline = null;
            PackageUpdatePipeline = null;
            PackageLoadingPipeline = null;
            _handles = null;
        }
    }
}