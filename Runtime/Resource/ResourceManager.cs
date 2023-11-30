using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame.FileSystem;
using ZGame.Networking;

namespace ZGame.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public sealed class ResourceManager : SingletonBehaviour<ResourceManager>
    {
        private AssetBundleLoadingHandle _assetBundleLoadingHandle;
        private ResourceObjectLoadingHandle _resourcesLodaingHandle;
        private CheckResourceUpdateHandle _checkResourceUpdateHandle;
        private List<ABHandle> _handles = new List<ABHandle>();
        private string address;

        public ResourceManager()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            _assetBundleLoadingHandle = new AssetBundleLoadingHandle();
            _resourcesLodaingHandle = new ResourceObjectLoadingHandle();
            _checkResourceUpdateHandle = new CheckResourceUpdateHandle();
        }

        private void OnSceneUnloaded(Scene arg0)
        {
            ResourcePackageManifest manifest = ResourcePackageListManifest.GetResourcePackageManifestWithAssetName(arg0.path);
            if (manifest is null)
            {
                return;
            }

            ABHandle bundleHandle = ABManager.instance.GetABHandleWithName(manifest.name);
            if (bundleHandle is null)
            {
                throw new FileNotFoundException(arg0.path);
            }

            bundleHandle.RemoveRef();
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        public ResHandle LoadAsset(string path)
        {
            if (_resourcesLodaingHandle is null)
            {
                Debug.LogError(new NullReferenceException(nameof(_resourcesLodaingHandle)));
                return default;
            }

            return _resourcesLodaingHandle.LoadAsset(path);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        public async UniTask<ResHandle> LoadAssetAsync(string path)
        {
            if (_resourcesLodaingHandle is null)
            {
                Debug.LogError(new NullReferenceException(nameof(_resourcesLodaingHandle)));
                return default;
            }

            return await _resourcesLodaingHandle.LoadAssetAsync(path);
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
            if (GlobalConfig.current.runtime == RuntimeMode.Editor)
            {
                progressCallback?.Invoke(1);
                return;
            }
#endif

            if (_assetBundleLoadingHandle is null)
            {
                throw new NullReferenceException(nameof(_assetBundleLoadingHandle));
            }

            await _assetBundleLoadingHandle.LoadingPackageList(progressCallback, args);
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
            if (GlobalConfig.current.runtime == RuntimeMode.Editor)
            {
                progressCallback?.Invoke(1);
                return;
            }
#endif
            if (_checkResourceUpdateHandle is null)
            {
                throw new NullReferenceException(nameof(_checkResourceUpdateHandle));
            }

            await _checkResourceUpdateHandle.StartUpdate(progressCallback, args);
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
            _resourcesLodaingHandle?.Dispose();
            _checkResourceUpdateHandle?.Dispose();
            _assetBundleLoadingHandle?.Dispose();
            _handles.Clear();
            _resourcesLodaingHandle = null;
            _checkResourceUpdateHandle = null;
            _assetBundleLoadingHandle = null;
            _handles = null;
        }
    }
}