using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using ZEngine.Options;
using ZEngine.VFS;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    class RuntimeAssetBundleHandle : IReference
    {
        public string name;
        public int refCount;
        public string module;

        private AssetBundle bundle;
        private HashSet<Object> _handles = new HashSet<Object>();

        public void Release()
        {
        }

        public bool Contains(Object target)
        {
            return _handles.Contains(target);
        }


        public T Load<T>(string path) where T : Object
        {
            return default;
        }

        public void Unload(Object obj)
        {
        }
    }

    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManager : Single<ResourceManager>
    {
        private List<ModuleManifest> moduleList = new List<ModuleManifest>();
        private List<RuntimeAssetBundleHandle> _bundleLists = new List<RuntimeAssetBundleHandle>();
        private Dictionary<string, IExecuteAsyncHandle> loadAssetHandles = new Dictionary<string, IExecuteAsyncHandle>();


        public ResourceManager()
        {
            IReadFileExecuteHandle readFileExecuteHandle = Engine.FileSystem.ReadFile("module");
            if (readFileExecuteHandle.EnsureExecuteSuccessfuly() is false)
            {
                return;
            }

            moduleList = Engine.Json.Parse<List<ModuleManifest>>(Encoding.UTF8.GetString(readFileExecuteHandle.bytes));
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
        public ICheckResourceUpdateExecuteHandle CheckUpdateResource(ResourceUpdateOptions options)
        {
            DefaultCheckResourceUpdateExecuteHandle defaultCheckUpdateExecuteHandle = Engine.Class.Loader<DefaultCheckResourceUpdateExecuteHandle>();
            defaultCheckUpdateExecuteHandle.Execute(options);
            return defaultCheckUpdateExecuteHandle;
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

        internal RuntimeAssetBundleHandle GetRuntimeAssetBundleHandle(string bundleName)
        {
            return _bundleLists.Find(x => x.name == bundleName);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public ILoadGameAssetExecuteHandle<T> LoadAsset<T>(string assetPath) where T : Object
        {
            DefaultLoadGameAssetExecuteHandle<T> defaultLoadGameAssetExecuteHandle = Engine.Class.Loader<DefaultLoadGameAssetExecuteHandle<T>>();
            defaultLoadGameAssetExecuteHandle.Execute(assetPath);
            return defaultLoadGameAssetExecuteHandle;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public ILoadGameAssetAsyncExecuteHandle<T> LoadAssetAsync<T>(string assetPath) where T : Object
        {
            if (loadAssetHandles.TryGetValue(assetPath, out IExecuteAsyncHandle handle))
            {
                return (ILoadGameAssetAsyncExecuteHandle<T>)handle;
            }

            DefaultLoadGameAssetAsyncExecuteHandle<T> loadGameAssetExecuteHandle = Engine.Class.Loader<DefaultLoadGameAssetAsyncExecuteHandle<T>>();
            loadGameAssetExecuteHandle.Subscribe(SubscribeMethodHandle.Create(() => { loadAssetHandles.Remove(assetPath); }));
            loadAssetHandles.Add(assetPath, loadGameAssetExecuteHandle);
            loadGameAssetExecuteHandle.Execute(assetPath);
            return loadGameAssetExecuteHandle;
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