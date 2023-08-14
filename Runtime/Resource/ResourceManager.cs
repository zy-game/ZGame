using System.Collections.Generic;
using System.Text;
using UnityEngine;
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
        private List<CacheData> cacheList = new List<CacheData>();
        private List<RuntimeModuleManifest> moduleList = new List<RuntimeModuleManifest>();
        private List<InternalRuntimeBundleHandle> bundleLists = new List<InternalRuntimeBundleHandle>();
        private Dictionary<string, IReference> loadAssetHandles = new Dictionary<string, IReference>();


        class CacheData
        {
            public float timeout;
            public InternalRuntimeBundleHandle bundle;
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
            moduleList.Add(manifest);
        }

        /// <summary>
        /// 移除资源模块数据
        /// </summary>
        /// <param name="manifest"></param>
        public void RemoveModuleManifest(RuntimeModuleManifest manifest)
        {
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
        /// 获取资源包依赖信息
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns></returns>
        public RuntimeBundleManifest[] GetBundleDependenciesList(RuntimeBundleManifest manifest)
        {
            List<RuntimeBundleManifest> runtimeBundleManifests = new List<RuntimeBundleManifest>() { manifest };
            if (manifest.dependencies is null || manifest.dependencies.Count is 0)
            {
                return runtimeBundleManifests.ToArray();
            }

            for (int i = 0; i < manifest.dependencies.Count; i++)
            {
                RuntimeBundleManifest bundleManifest = ResourceManager.instance.GetBundleManifestWithAssetPath(manifest.dependencies[i]);
                if (bundleManifest is null)
                {
                    Engine.Console.Error("Not Find AssetBundle Dependencies:" + manifest.dependencies[i]);
                    return default;
                }

                RuntimeBundleManifest[] runtimeBundleManifestList = GetBundleDependenciesList(bundleManifest);
                if (runtimeBundleManifestList is null)
                {
                    return default;
                }

                foreach (var target in runtimeBundleManifestList)
                {
                    if (runtimeBundleManifests.Contains(target))
                    {
                        continue;
                    }

                    runtimeBundleManifests.Add(target);
                }
            }

            return runtimeBundleManifests.ToArray();
        }

        /// <summary>
        /// 预加载资源模块
        /// </summary>
        /// <param name="options">预加载配置</param>
        public IResourceModuleLoaderExecuteHandle PreLoadResourceModule(params PreloadOptions[] options)
        {
            DefaultResourceModuleLoaderExecuteHandle resourceModuleLoaderExecuteHandle = Engine.Class.Loader<DefaultResourceModuleLoaderExecuteHandle>();
            resourceModuleLoaderExecuteHandle.Execute(options);
            return resourceModuleLoaderExecuteHandle;
        }

        /// <summary>
        /// 资源更新检查
        /// </summary>
        /// <param name="options">资源更新检查配置</param>
        /// <returns></returns>
        public ICheckResourceUpdateExecuteHandle CheckUpdateResource(params UpdateOptions[] options)
        {
            DefaultCheckResourceUpdateExecuteHandle defaultCheckResourceUpdateExecuteHandle = Engine.Class.Loader<DefaultCheckResourceUpdateExecuteHandle>();
            defaultCheckResourceUpdateExecuteHandle.Execute(options);
            return defaultCheckResourceUpdateExecuteHandle;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public RequestAssetResult<T> LoadAsset<T>(string assetPath) where T : Object
        {
            //todo 如果在编辑器并且没有启用热更，那么直接用编辑器的api加载资源
            DefaultRequestAssetExecute<T> defaultLoadAssetExecuteHandle = Engine.Class.Loader<DefaultRequestAssetExecute<T>>();
            defaultLoadAssetExecuteHandle.Execute(assetPath);
            return defaultLoadAssetExecuteHandle.result;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public IRequestAssetExecuteHandle<T> LoadAssetAsync<T>(string assetPath) where T : Object
        {
            //todo 如果在编辑器并且没有启用热更，那么直接用编辑器的api加载资源
            if (loadAssetHandles.TryGetValue(assetPath, out IReference handle))
            {
                return (IRequestAssetExecuteHandle<T>)handle;
            }

            DefaultRequestAssetExecuteHandle<T> defaultRequestAssetExecuteHandle = Engine.Class.Loader<DefaultRequestAssetExecuteHandle<T>>();
            defaultRequestAssetExecuteHandle.Subscribe(ISubscribeHandle.Create<T>(args => { loadAssetHandles.Remove(assetPath); }));
            loadAssetHandles.Add(assetPath, defaultRequestAssetExecuteHandle);
            defaultRequestAssetExecuteHandle.Execute(assetPath);
            return defaultRequestAssetExecuteHandle;
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="target">资源句柄</param>
        public void Release(Object target)
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