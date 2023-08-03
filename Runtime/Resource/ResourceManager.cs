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
        private List<RuntimeModuleManifest> moduleList = new List<RuntimeModuleManifest>();
        private List<CacheTokenHandle> cacheList = new List<CacheTokenHandle>();
        private List<IRuntimeBundleManifest> _bundleLists = new List<IRuntimeBundleManifest>();
        private Dictionary<string, IReference> loadAssetHandles = new Dictionary<string, IReference>();


        public ResourceManager()
        {
            IReadFileExecute readFileExecute = Engine.FileSystem.ReadFile("module");
            if (readFileExecute.bytes is null || readFileExecute.bytes.Length is 0)
            {
                return;
            }

            moduleList = Engine.Json.Parse<List<RuntimeModuleManifest>>(Encoding.UTF8.GetString(readFileExecute.bytes));
        }

        internal void SaveModuleData()
        {
            string str = Engine.Json.ToJson(moduleList);
            Engine.FileSystem.WriteFile("module", Encoding.UTF8.GetBytes(str), VersionOptions.None);
        }

        internal void AddAssetBundleHandle(IRuntimeBundleManifest bundleHandle)
        {
        }

        public bool HasLoadAssetBundle(string name)
        {
            return GetRuntimeAssetBundleHandle(name) is not null;
        }

        internal IRuntimeBundleManifest GetRuntimeAssetBundleHandle(string bundleName)
        {
            IRuntimeBundleManifest runtimeAssetBundleHandle = _bundleLists.Find(x => x.name == bundleName);
            if (runtimeAssetBundleHandle is null)
            {
                CacheTokenHandle cacheTokenHandle = cacheList.Find(x => x.name == bundleName);
                if (cacheTokenHandle is not null)
                {
                    runtimeAssetBundleHandle = Engine.Cache.Dequeue<IRuntimeBundleManifest>(cacheTokenHandle);
                    _bundleLists.Add(runtimeAssetBundleHandle);
                }
            }

            return runtimeAssetBundleHandle;
        }

        /// <summary>
        /// 获取资源模块版本数据
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <returns></returns>
        public RuntimeModuleManifest GetModuleManifest(string moduleName)
        {
            return moduleList.Find(x => x.name == moduleName);
        }

        /// <summary>
        /// 获取资源包版本数据
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <param name="bundleName">资源包名</param>
        /// <returns></returns>
        public RuntimeBundleManifest GetResourceBundleManifest(string moduleName, string bundleName)
        {
            RuntimeModuleManifest moduleManifest = GetModuleManifest(moduleName);
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
        public RuntimeBundleManifest GetResourceBundleManifest(string assetPath)
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
            //todo 如果在编辑器并且没有启用热更，那么直接用编辑器的api加载资源
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
            //todo 如果在编辑器并且没有启用热更，那么直接用编辑器的api加载资源
            if (loadAssetHandles.TryGetValue(assetPath, out IReference handle))
            {
                return (IAssetRequestExecuteHandle<T>)handle;
            }

            DefaultAssetRequestExecuteHandle<T> asyncAssetRequestExecuteHandle = Engine.Class.Loader<DefaultAssetRequestExecuteHandle<T>>();
            asyncAssetRequestExecuteHandle.Subscribe(ISubscribeExecuteHandle<T>.Create(args => { loadAssetHandles.Remove(assetPath); }));
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
            IRuntimeBundleManifest runtimeAssetObjectHandle = _bundleLists.Find(x => x.Contains(target));
            if (runtimeAssetObjectHandle is null)
            {
                return;
            }

            runtimeAssetObjectHandle.Unload(target);
            if (runtimeAssetObjectHandle.refCount is not 0)
            {
                return;
            }

            _bundleLists.Remove(runtimeAssetObjectHandle);
            cacheList.Add(Engine.Cache.Enqueue(runtimeAssetObjectHandle));
        }
    }
}