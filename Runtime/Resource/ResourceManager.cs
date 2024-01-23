using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame.FileSystem;
using ZGame.Networking;
using ZGame.UI;

namespace ZGame.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public sealed class ResourceManager : Singleton<ResourceManager>
    {
        private IResourcePackageUpdateHandle _resourcePackageUpdateHandle;
        private IResourcePackageLoadingHandle _resourceResourcePackageLoadingHandle;
        private List<IResourceLoadingHandle> _resourceLoadingHandles = new List<IResourceLoadingHandle>();

        protected override void OnAwake()
        {
            SetResourceUpdateHandle<DefaultResourcePackageUpdateHandle>();
            SetupResourceBundleLoadingHandle<DefaultResourcePackageLoadingHandle>();
            SetupResourceLoadingHandle<DefaultUnityResourceLoadingHandle>();
            SetupResourceLoadingHandle<DefaultNetworkResourceLoadingHandle>();
#if UNITY_EDITOR
            if (BasicConfig.instance.resMode == ResourceMode.Editor)
            {
                SetupResourceLoadingHandle<DefaultEditorResourceLoadingHandle>();
                return;
            }
#endif
            SetupResourceLoadingHandle<DefaultPackageResourceLoadingHandle>();
        }

        protected override void OnDestroy()
        {
            _resourceLoadingHandles.ForEach(x => x.Dispose());
            _resourceResourcePackageLoadingHandle.Dispose();
            _resourcePackageUpdateHandle.Dispose();
            _resourceLoadingHandles.Clear();
            _resourceResourcePackageLoadingHandle = null;
            _resourcePackageUpdateHandle = null;
        }

        /// <summary>
        /// 设置资源加载管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetupResourceLoadingHandle<T>() where T : IResourceLoadingHandle
        {
            SetupResourceLoadingHandle(typeof(T));
        }

        /// <summary>
        /// 设置资源加载管道
        /// </summary>
        /// <param name="type"></param>
        public void SetupResourceLoadingHandle(Type type)
        {
            if (typeof(IResourceLoadingHandle).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(typeof(IResourceLoadingHandle).Name);
            }

            IResourceLoadingHandle resourceLoadingHelper = Activator.CreateInstance(type) as IResourceLoadingHandle;
            if (resourceLoadingHelper is null)
            {
                return;
            }

            SetupResourceLoadingHandle(resourceLoadingHelper);
        }

        /// <summary>
        /// 设置资源加载管道
        /// </summary>
        /// <param name="resourceLoadingHelper"></param>
        public void SetupResourceLoadingHandle(IResourceLoadingHandle resourceLoadingHelper)
        {
            if (_resourceLoadingHandles.Contains(resourceLoadingHelper))
            {
                return;
            }

            _resourceLoadingHandles.Add(resourceLoadingHelper);
        }

        /// <summary>
        /// 移除资源加载管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnSetupResourceLoadingHandle<T>() where T : IResourceLoadingHandle
        {
            UnSetupResourceLoadingHandle(typeof(T));
        }

        /// <summary>
        /// 移除资源加载管道
        /// </summary>
        /// <param name="type"></param>
        public void UnSetupResourceLoadingHandle(Type type)
        {
            IResourceLoadingHandle resourceLoadingHelper = _resourceLoadingHandles.Find(r => r.GetType() == type);
            if (resourceLoadingHelper is null)
            {
                return;
            }

            UnSetupResourceLoadingHandle(resourceLoadingHelper);
        }

        /// <summary>
        /// 移除资源加载管道
        /// </summary>
        /// <param name="resourceLoadingHelper"></param>
        public void UnSetupResourceLoadingHandle(IResourceLoadingHandle resourceLoadingHelper)
        {
            _resourceLoadingHandles.Remove(resourceLoadingHelper);
        }

        /// <summary>
        /// 设置资源包加载管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetupResourceBundleLoadingHandle<T>() where T : IResourcePackageLoadingHandle
        {
            SetupResourceBundleLoadingHandle(typeof(T));
        }

        /// <summary>
        /// 设置资源包加载管道
        /// </summary>
        /// <param name="type"></param>
        public void SetupResourceBundleLoadingHandle(Type type)
        {
            if (typeof(IResourcePackageLoadingHandle).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(typeof(IResourcePackageLoadingHandle).Name);
            }

            IResourcePackageLoadingHandle _resourcePackageLoadingHelper = Activator.CreateInstance(type) as IResourcePackageLoadingHandle;
            if (_resourcePackageLoadingHelper is null)
            {
                return;
            }

            SetupResourceBundleLoadingHandle(_resourcePackageLoadingHelper);
        }

        /// <summary>
        /// 设置资源包加载管道
        /// </summary>
        /// <param name="_resourcePackageLoadingHelper"></param>
        public void SetupResourceBundleLoadingHandle(IResourcePackageLoadingHandle _resourcePackageLoadingHelper)
        {
            _resourceResourcePackageLoadingHandle?.Dispose();
            _resourceResourcePackageLoadingHandle = _resourcePackageLoadingHelper;
        }

        /// <summary>
        /// 移除资源包加载管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnSetupResourceBundleLoadingHandle()
        {
            SetupResourceBundleLoadingHandle<DefaultResourcePackageLoadingHandle>();
        }

        /// <summary>
        /// 设置资源更新管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SetResourceUpdateHandle<T>() where T : IResourcePackageUpdateHandle
        {
            SetResourceUpdateHandle(typeof(T));
        }

        /// <summary>
        /// 设置资源更新管道
        /// </summary>
        /// <param name="type"></param>
        public void SetResourceUpdateHandle(Type type)
        {
            if (typeof(IResourcePackageUpdateHandle).IsAssignableFrom(type) is false)
            {
                throw new NotImplementedException(typeof(IResourcePackageUpdateHandle).Name);
            }

            IResourcePackageUpdateHandle _resourcePackageUpdateHandle = Activator.CreateInstance(type) as IResourcePackageUpdateHandle;
            if (_resourcePackageUpdateHandle is null)
            {
                return;
            }

            SetResourceUpdateHandle(_resourcePackageUpdateHandle);
        }

        /// <summary>
        /// 设置资源更新管道
        /// </summary>
        /// <param name="handle"></param>
        public void SetResourceUpdateHandle(IResourcePackageUpdateHandle handle)
        {
            _resourcePackageUpdateHandle?.Dispose();
            _resourcePackageUpdateHandle = handle;
        }

        /// <summary>
        /// 移除资源更新管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnSetResourceUpdateHandle()
        {
            SetResourceUpdateHandle<DefaultResourcePackageUpdateHandle>();
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        public ResObject LoadAsset(string path)
        {
            Debug.Log("加载资源：" + path);
            ResObject result = default;
            IResourceLoadingHandle resourceLoadingHandle = _resourceLoadingHandles.Find(x => x.Contains(path));
            if (resourceLoadingHandle is not null)
            {
                result = resourceLoadingHandle.LoadAsset(path);
            }

            return result;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        public async UniTask<ResObject> LoadAssetAsync(string path)
        {
            Debug.Log("加载资源：" + path);
            ResObject result = default;
            IResourceLoadingHandle resourceLoadingHandle = _resourceLoadingHandles.Find(x => x.Contains(path));
            if (resourceLoadingHandle is not null)
            {
                result = await resourceLoadingHandle.LoadAssetAsync(path);
            }

            return result;
        }

        /// <summary>
        /// 预加载资源包列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async UniTask PerloadingResourcePackageList(string configName)
        {
            if (configName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("configName");
            }

            await UpdateResourcePackageList(configName);
            await LoadingResourcePackageList(configName);
        }

        /// <summary>
        /// 加载资源包列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async UniTask LoadingResourcePackageList(string configName)
        {
            if (configName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("configName");
            }

            UILoading.SetTitle("正在加载资源信息...");
            UILoading.SetProgress(0);
            List<ResourcePackageManifest> manifests = PackageManifestManager.instance.GetResourcePackageAndDependencyList(configName);
            if (manifests is null || manifests.Count == 0)
            {
                UILoading.SetTitle("资源加载完成...");
                UILoading.SetProgress(1);
            }

            await _resourceResourcePackageLoadingHandle.LoadingPackageListAsync(manifests.ToArray());
        }

        public void LoadingResourcePackageListSync(string configName)
        {
            if (configName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("configName");
            }

            List<ResourcePackageManifest> manifests = PackageManifestManager.instance.GetResourcePackageAndDependencyList(configName);
            if (manifests is null || manifests.Count == 0)
            {
                UILoading.SetTitle("资源加载完成...");
                UILoading.SetProgress(1);
            }

            _resourceResourcePackageLoadingHandle.LoadingPackageListSync(manifests.ToArray());
        }

        public void LoadingPackageListSync(params ResourcePackageManifest[] manifest)
        {
            _resourceResourcePackageLoadingHandle.LoadingPackageListSync(manifest);
        }

        public async UniTask LoadingPackageListAsync(params ResourcePackageManifest[] manifest)
        {
            await _resourceResourcePackageLoadingHandle.LoadingPackageListAsync(manifest);
        }

        /// <summary>
        /// 更新资源列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async UniTask UpdateResourcePackageList(string configName)
        {
            if (configName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("configName");
            }

            UILoading.SetTitle("检查资源配置...");
            UILoading.SetProgress(0);
            List<ResourcePackageManifest> manifests = PackageManifestManager.instance.CheckNeedUpdatePackageList(configName);
            if (manifests is null || manifests.Count == 0)
            {
                UILoading.SetTitle("资源更新完成...");
                UILoading.SetProgress(1);
            }

            await _resourcePackageUpdateHandle.UpdateResourcePackageList(manifests);
        }

        /// <summary>
        /// 卸载资源包列表
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="isUnloadDependenecis"></param>
        public void UnloadPackageList(string packageName, bool isUnloadDependenecis = true)
        {
        }
    }
}