using System.Collections.Generic;
using System.Text;
using UnityEngine.Events;
using ZEngine.Options;
using ZEngine.VFS;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    internal class ResourceManager : Single<ResourceManager>
    {
        private List<ModuleManifest> moduleList = new List<ModuleManifest>();
        private List<CacheTokenHandle> cacheList = new List<CacheTokenHandle>();
        private List<RuntimeAssetBundleHandle> _bundleLists = new List<RuntimeAssetBundleHandle>();
        private Dictionary<string, IExecuteHandle> loadAssetHandles = new Dictionary<string, IExecuteHandle>();


        public ResourceManager()
        {
            IReadFileExecute readFileExecuteHandleHandle = Engine.FileSystem.ReadFile("module");
            if (readFileExecuteHandleHandle.EnsureExecuteSuccessfuly() is false)
            {
                return;
            }

            moduleList = Engine.Json.Parse<List<ModuleManifest>>(Encoding.UTF8.GetString(readFileExecuteHandleHandle.bytes));
        }

        internal void AddAssetBundleHandle(RuntimeAssetBundleHandle bundleHandle)
        {
            _bundleLists.Add(bundleHandle);
        }

        internal void RemoveAssetBundleHandle(RuntimeAssetBundleHandle bundleHandle)
        {
            _bundleLists.Remove(bundleHandle);
            cacheList.Add(Engine.Cache.Enqueue(bundleHandle));
        }

        internal RuntimeAssetBundleHandle GetRuntimeAssetBundleHandle(string bundleName)
        {
            RuntimeAssetBundleHandle runtimeAssetBundleHandle = _bundleLists.Find(x => x.name == bundleName);
            if (runtimeAssetBundleHandle is null)
            {
                CacheTokenHandle cacheTokenHandle = cacheList.Find(x => x.name == bundleName);
                if (cacheTokenHandle is not null)
                {
                    runtimeAssetBundleHandle = Engine.Cache.Dequeue<RuntimeAssetBundleHandle>(cacheTokenHandle);
                }
            }

            return runtimeAssetBundleHandle;
        }

        /// <summary>
        /// 获取资源模块版本数据
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <returns></returns>
        public ModuleManifest GetModuleManifest(string moduleName)
        {
            return moduleList.Find(x => x.name == moduleName);
        }

        /// <summary>
        /// 获取资源包版本数据
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <param name="bundleName">资源包名</param>
        /// <returns></returns>
        public BundleManifest GetResourceBundleManifest(string moduleName, string bundleName)
        {
            ModuleManifest moduleManifest = GetModuleManifest(moduleName);
            if (moduleManifest is null || moduleManifest.bundleList is null)
            {
                return default;
            }

            return moduleManifest.bundleList.Find(x => x.name == bundleName);
        }

        /// <summary>
        /// 获取资源包信息
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public BundleManifest GetResourceBundleManifest(string assetPath)
        {
            foreach (var module in moduleList)
            {
                BundleManifest bundleManifest = module.GetBundleManifestWithAsset(assetPath);
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
        public IResourcePreloadExecuteHandle PreLoadResourceModule(ResourcePreloadOptions options)
        {
            DefaultResourcePreloadExecuteHandle defaultResourcePreloadExecuteHandle = Engine.Class.Loader<DefaultResourcePreloadExecuteHandle>();
            defaultResourcePreloadExecuteHandle.Execute(options);
            return defaultResourcePreloadExecuteHandle;
        }

        /// <summary>
        /// 资源更新检查
        /// </summary>
        /// <param name="options">资源更新检查配置</param>
        /// <returns></returns>
        public ICheckUpdateExecuteHandle CheckUpdateResource(ResourceUpdateOptions options)
        {
            DefaultCheckUpdateExecuteHandle defaultCheckUpdateExecuteHandle = Engine.Class.Loader<DefaultCheckUpdateExecuteHandle>();
            defaultCheckUpdateExecuteHandle.Execute(options);
            return defaultCheckUpdateExecuteHandle;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public IAssetRequestExecute<T> LoadAsset<T>(string assetPath) where T : Object
        {
            DefaultAssetRequestExecute<T> defaultLoadAssetExecuteHandle = Engine.Class.Loader<DefaultAssetRequestExecute<T>>();
            defaultLoadAssetExecuteHandle.Execute(assetPath);
            return defaultLoadAssetExecuteHandle;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public IAssetRequestExecuteHandle<T> LoadAssetAsync<T>(string assetPath) where T : Object
        {
            if (loadAssetHandles.TryGetValue(assetPath, out IExecuteHandle handle))
            {
                return (IAssetRequestExecuteHandle<T>)handle;
            }

            DefaultAssetRequestExecuteHandle<T> asyncAssetRequestExecuteHandle = Engine.Class.Loader<DefaultAssetRequestExecuteHandle<T>>();
            asyncAssetRequestExecuteHandle.Subscribe(Method.Create(() => { loadAssetHandles.Remove(assetPath); }));
            loadAssetHandles.Add(assetPath, asyncAssetRequestExecuteHandle);
            asyncAssetRequestExecuteHandle.Execute(assetPath);
            return asyncAssetRequestExecuteHandle;
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="target">资源句柄</param>
        public void Release(Object target)
        {
            RuntimeAssetBundleHandle runtimeAssetObjectHandle = _bundleLists.Find(x => x.Contains(target));
            if (runtimeAssetObjectHandle is null)
            {
                return;
            }

            runtimeAssetObjectHandle.Unload(target);
        }
    }
}