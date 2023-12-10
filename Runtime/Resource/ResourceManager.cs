using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZGame.FileSystem;
using ZGame.Networking;
using ZGame.Window;

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

        protected internal override void OnAwake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
            SetResourceUpdateHandle<DefaultResourcePackagePackageUpdateHandle>();
            SetupResourceBundleLoadingHandle<DefaultResourcePackageLoadingHandle>();
            SetupResourceLoadingHandle<InternalResourceLoadingHandle>();
            SetupResourceLoadingHandle<NetworkResourceLoadingHandle>();
#if UNITY_EDITOR
            if (GlobalConfig.current.runtime == RuntimeMode.Editor)
            {
                SetupResourceLoadingHandle<EditorModeResourceLoadingHandle>();
                return;
            }
#endif
            SetupResourceLoadingHandle<BundleResourceLoadingHandle>();
        }

        private void OnSceneUnloaded(Scene arg0)
        {
            ResourcePackageManifest manifest = ResourcePackageListManifest.GetResourcePackageManifestWithAssetName(arg0.path);
            if (manifest is null)
            {
                return;
            }

            ResourcePackageHandle bundleHandle = BundleManager.instance.GetABHandleWithName(manifest.name);
            if (bundleHandle is null)
            {
                throw new FileNotFoundException(arg0.path);
            }

            bundleHandle.RemoveRef();
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
            Debug.Log("Setup Resource Loading Handle:" + resourceLoadingHelper.GetType().Name);
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
            SetResourceUpdateHandle<DefaultResourcePackagePackageUpdateHandle>();
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        public ResHandle LoadAsset(string path)
        {
            ResHandle result = default;
            foreach (var VARIABLE in _resourceLoadingHandles)
            {
                result = VARIABLE.LoadAsset(path);
                if (result is not null)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        public async UniTask<ResHandle> LoadAssetAsync(string path, ILoadingHandle loadingHandle = null)
        {
            ResHandle result = default;
            foreach (var VARIABLE in _resourceLoadingHandles)
            {
                result = await VARIABLE.LoadAssetAsync(path, loadingHandle);
                if (result is not null)
                {
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// 加载资源包列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async UniTask LoadingResourcePackageList(ILoadingHandle loadingHandle, params string[] args)
        {
            if (args is null || args.Length == 0)
            {
                throw new ArgumentNullException("args");
            }
#if UNITY_EDITOR
            if (GlobalConfig.current.runtime == RuntimeMode.Editor)
            {
                loadingHandle?.Report(1);
                return;
            }
#endif

            await _resourceResourcePackageLoadingHandle.Loading(loadingHandle, args);
        }

        /// <summary>
        /// 更新资源列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async UniTask CheckUpdateResourcePackageList(ILoadingHandle loadingHandle, params string[] args)
        {
            if (args is null || args.Length == 0)
            {
                throw new ArgumentNullException("args");
            }
#if UNITY_EDITOR
            if (GlobalConfig.current.runtime == RuntimeMode.Editor)
            {
                loadingHandle?.Report(1);
                return;
            }
#endif
            await _resourcePackageUpdateHandle.Update(loadingHandle, args);
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="obj"></param>
        public void Release(ResHandle obj)
        {
            foreach (var VARIABLE in _resourceLoadingHandles)
            {
                if (VARIABLE.Release(obj))
                {
                    return;
                }
            }
        }

        public void Dispose()
        {
            _resourceLoadingHandles.ForEach(x => x.Dispose());
            _resourceResourcePackageLoadingHandle.Dispose();
            _resourcePackageUpdateHandle.Dispose();
            _resourceLoadingHandles.Clear();
            _resourceResourcePackageLoadingHandle = null;
            _resourcePackageUpdateHandle = null;
        }
    }
}