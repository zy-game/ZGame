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
    class UnloadQueueTask
    {
        public float time;
        public ResPackageHandle handle;
    }

    /// <summary>
    /// 资源管理器
    /// </summary>
    public sealed class ResourceManager : Singleton<ResourceManager>
    {
        private IResourcePackageUpdateHandle _resourcePackageUpdateHandle;
        private IResourcePackageLoadingHandle _resourceResourcePackageLoadingHandle;
        private List<IResourceLoadingHandle> _resourceLoadingHandles = new List<IResourceLoadingHandle>();
        private List<ResPackageHandle> _handles = new List<ResPackageHandle>();
        private List<UnloadQueueTask> unloadList = new List<UnloadQueueTask>();
        private float checkTime = 0;

        protected override void OnAwake()
        {
            MoveNextTime();
            SetResourceUpdateHandle<DefaultResourcePackageUpdateHandle>();
            SetupResourceBundleLoadingHandle<DefaultResourcePackageLoadingHandle>();
            SetupResourceLoadingHandle<DefaultUnityResourceLoadingHandle>();
            SetupResourceLoadingHandle<DefaultNetworkResourceLoadingHandle>();
#if UNITY_EDITOR
            if (GlobalConfig.instance.resConfig.resMode == ResourceMode.Editor)
            {
                SetupResourceLoadingHandle<DefaultEditorResourceLoadingHandle>();
                return;
            }
#endif
            SetupResourceLoadingHandle<DefaultPackageResourceLoadingHandle>();
        }

        protected override void OnDestroy()
        {
            _handles.ForEach(x => x.Dispose());
            _handles.Clear();
            _resourceLoadingHandles.ForEach(x => x.Dispose());
            _resourceResourcePackageLoadingHandle.Dispose();
            _resourcePackageUpdateHandle.Dispose();
            _resourceLoadingHandles.Clear();
            _resourceResourcePackageLoadingHandle = null;
            _resourcePackageUpdateHandle = null;
        }

        private void MoveNextTime()
        {
            checkTime = Time.realtimeSinceStartup + GlobalConfig.instance.resConfig.unloadInterval;
        }

        protected override void OnUpdate()
        {
            if (Time.realtimeSinceStartup < checkTime)
            {
                return;
            }

            MoveNextTime();

            //todo 检查是否需要卸载资源包
            for (int i = 0; i < _handles.Count; i++)
            {
                if (_handles[i].refCount > 0 || _handles[i].DefaultPackage)
                {
                    continue;
                }

                //todo 加入待卸载列表
                unloadList.Add(new UnloadQueueTask()
                {
                    handle = _handles[i],
                    time = Time.realtimeSinceStartup + GlobalConfig.instance.resConfig.unloadInterval
                });
                _handles.Remove(_handles[i]);
            }

            //todo 卸载资源包
            for (int i = 0; i < unloadList.Count; i++)
            {
                if (unloadList[i].time > Time.realtimeSinceStartup)
                {
                    continue;
                }

                unloadList[i].handle.Dispose();
                unloadList.RemoveAt(i);
                i--;
            }
        }

        public void AddResourcePackageHandle(ResPackageHandle handle)
        {
            _handles.Add(handle);
        }

        public void RemoveResourcePackageHandle(string name)
        {
            ResPackageHandle handle = _handles.Find(x => x.name == name);
            if (handle is null)
            {
                return;
            }

            _handles.Remove(handle);
            handle.Dispose();
        }

        public ResPackageHandle GetResourcePackageHandle(string name)
        {
            ResPackageHandle handle = _handles.Find(x => x.name == name);
            if (handle is null)
            {
                UnloadQueueTask task = unloadList.Find(x => x.handle.name == name);
                if (task != null)
                {
                    _handles.Add(task.handle);
                    unloadList.Remove(task);
                }
            }

            return handle;
        }

        public ResPackageHandle GetResourcePackageHandleWithAssetPath(string path)
        {
            ResourcePackageManifest manifest = ResourcePackageListManifest.GetResourcePackageManifestWithAssetName(path);
            if (manifest is null)
            {
                return default;
            }

            return GetResourcePackageHandle(manifest.name);
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
            SetResourceUpdateHandle<DefaultResourcePackageUpdateHandle>();
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        public ResHandle LoadAsset(string path)
        {
            ResHandle result = default;
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
        public async UniTask<ResHandle> LoadAssetAsync(string path, ILoadingHandle loadingHandle = null)
        {
            ResHandle result = default;
            IResourceLoadingHandle resourceLoadingHandle = _resourceLoadingHandles.Find(x => x.Contains(path));
            if (resourceLoadingHandle is not null)
            {
                result = await resourceLoadingHandle.LoadAssetAsync(path);
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
            if (GlobalConfig.instance.resConfig.resMode == ResourceMode.Editor)
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
            if (GlobalConfig.instance.resConfig.resMode == ResourceMode.Editor)
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
        public void ReleaseAsset(ResHandle obj)
        {
            ReleaseAsset(obj.path);
        }

        public void ReleaseAsset(string path)
        {
            ResPackageHandle packageHandle = GetResourcePackageHandleWithAssetPath(path);
            if (packageHandle is null)
            {
                _resourceLoadingHandles.ForEach(x => x.Release(path));
                return;
            }

            packageHandle.Release(path);
        }
    }
}