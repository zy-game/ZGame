using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using ZEngine.VFS;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    internal class ResourceManager : Singleton<ResourceManager>
    {
        private List<CacheData> cacheList = new List<CacheData>();
        private List<RuntimeModuleManifest> moduleList = new List<RuntimeModuleManifest>();
        private List<InternalRuntimeBundleHandle> bundleLists = new List<InternalRuntimeBundleHandle>();
        private Dictionary<string, IDisposable> loadAssetHandles = new Dictionary<string, IDisposable>();


        class CacheData : IDisposable
        {
            public float timeout;
            public InternalRuntimeBundleHandle bundle;

            public void Dispose()
            {
                timeout = 0;
                bundle.Dispose();
                bundle = null;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            cacheList.ForEach(x => x.Dispose());
            cacheList.Clear();
            moduleList.Clear();
            bundleLists.ForEach(x => x.Dispose());
            bundleLists.Clear();
            foreach (var loadAssetHandle in loadAssetHandles.Values)
            {
                loadAssetHandle.Dispose();
            }

            loadAssetHandles.Clear();
            Engine.Console.Log("释放所有资源");
        }

        /// <summary>
        /// 添加资源包
        /// </summary>
        /// <param name="bundleHandle"></param>
        internal void AddAssetBundleHandle(InternalRuntimeBundleHandle bundleHandle)
        {
            bundleLists.Add(bundleHandle);
        }

        /// <summary>
        /// 是否已加载指定的资源包
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal bool HasLoadAssetBundle(string module, string name)
        {
            return GetRuntimeAssetBundleHandle(module, name) is not null;
        }

        /// <summary>
        /// 获取已加载的资源包
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        internal InternalRuntimeBundleHandle GetRuntimeAssetBundleHandle(string module, string bundleName)
        {
            InternalRuntimeBundleHandle runtimeAssetBundleHandle = bundleLists.Find(x => x.name == bundleName && x.module == module);
            if (runtimeAssetBundleHandle is null)
            {
                CacheData cacheData = cacheList.Find(x => x.bundle.name == bundleName && x.bundle.module == module);
                if (cacheData is not null)
                {
                    runtimeAssetBundleHandle = cacheData.bundle;
                    cacheList.Remove(cacheData);
                    bundleLists.Add(runtimeAssetBundleHandle);
                }
            }

            return runtimeAssetBundleHandle;
        }

        /// <summary>
        /// 获取资源模块版本数据
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <returns></returns>
        public RuntimeModuleManifest GetRuntimeModuleManifest(string moduleName)
        {
            RuntimeModuleManifest manifest = moduleList.Find(x => x.name == moduleName);
            if (manifest is null)
            {
                return default;
            }

            return manifest;
        }

        /// <summary>
        /// 添加资源模块数据
        /// </summary>
        /// <param name="manifest"></param>
        public void AddModuleManifest(RuntimeModuleManifest manifest)
        {
            if (moduleList.Find(x => x.name == manifest.name) is not null)
            {
                return;
            }

            moduleList.Add(manifest);
        }

        /// <summary>
        /// 移除资源模块数据
        /// </summary>
        /// <param name="manifest"></param>
        public void RemoveModuleManifest(RuntimeModuleManifest manifest)
        {
            if (moduleList.Find(x => x.name == manifest.name) is null)
            {
                return;
            }

            moduleList.Remove(manifest);
        }

        /// <summary>
        /// 获取资源包版本数据
        /// </summary>
        /// <param name="bundleName">资源包名</param>
        /// <returns></returns>
        public RuntimeBundleManifest GetRuntimeBundleManifest(string bundleName)
        {
            RuntimeBundleManifest runtimeBundleManifest = default;
            foreach (var module in moduleList)
            {
                runtimeBundleManifest = module.bundleList.Find(x => x.name == bundleName);
                if (runtimeBundleManifest is not null)
                {
                    break;
                }
            }

            return runtimeBundleManifest;
        }

        /// <summary>
        /// 获取资源包信息
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public RuntimeBundleManifest GetBundleManifestWithAssetPath(string assetPath)
        {
            foreach (var module in moduleList)
            {
                RuntimeBundleManifest bundleManifest = module.GetBundleManifestWithAsset(assetPath);
                if (bundleManifest is null)
                {
                    continue;
                }

                return bundleManifest;
            }

            return default;
        }

        /// <summary>
        /// 预加载资源模块
        /// </summary>
        /// <param name="options">预加载配置</param>
        public IResourceModuleLoaderExecuteHandle LoaderResourceModule(params ModuleOptions[] options)
        {
            IResourceModuleLoaderExecuteHandle resourceModuleLoaderExecuteHandle = IResourceModuleLoaderExecuteHandle.Create(options);
            resourceModuleLoaderExecuteHandle.Execute();
            return resourceModuleLoaderExecuteHandle;
        }

        /// <summary>
        /// 资源更新检查
        /// </summary>
        /// <param name="options">资源更新检查配置</param>
        /// <returns></returns>
        public ICheckResourceUpdateExecuteHandle CheckModuleResourceUpdate(params ModuleOptions[] options)
        {
            ICheckResourceUpdateExecuteHandle defaultCheckResourceUpdateExecuteHandle = ICheckResourceUpdateExecuteHandle.Create(options);
            defaultCheckResourceUpdateExecuteHandle.Execute();
            return defaultCheckResourceUpdateExecuteHandle;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public IRequestAssetExecute<T> LoadAsset<T>(string assetPath) where T : Object
        {
            //todo 如果在编辑器并且没有启用热更，那么直接用编辑器的api加载资源
            IRequestAssetExecute<T> requestAssetExecute = IRequestAssetExecute<T>.Create(assetPath);
            requestAssetExecute.Execute();
            return requestAssetExecute;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public IRequestAssetExecuteHandle<T> LoadAssetAsync<T>(string assetPath) where T : Object
        {
            //todo 如果在编辑器并且没有启用热更，那么直接用编辑器的api加载资源
            if (loadAssetHandles.TryGetValue(assetPath, out IDisposable handle))
            {
                return (IRequestAssetExecuteHandle<T>)handle;
            }

            loadAssetHandles.Add(assetPath, handle = IRequestAssetExecuteHandle<T>.Create(assetPath));
            IRequestAssetExecuteHandle<T> requestAssetExecuteHandle = (IRequestAssetExecuteHandle<T>)handle;
            requestAssetExecuteHandle.Subscribe(ISubscriber.Create<IRequestAssetExecuteHandle<T>>(args => { loadAssetHandles.Remove(assetPath); }));
            requestAssetExecuteHandle.Execute();
            return requestAssetExecuteHandle;
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="target">资源句柄</param>
        public void Dispose(Object target)
        {
            InternalRuntimeBundleHandle runtimeAssetObjectHandle = bundleLists.Find(x => x.Contains(target));
            if (runtimeAssetObjectHandle is null)
            {
                return;
            }

            runtimeAssetObjectHandle.Unload(target);
            if (runtimeAssetObjectHandle.refCount is not 0)
            {
                return;
            }

            bundleLists.Remove(runtimeAssetObjectHandle);
            cacheList.Add(new CacheData()
            {
                timeout = Time.realtimeSinceStartup + HotfixOptions.instance.cachetime,
                bundle = runtimeAssetObjectHandle,
            });
        }
    }
}