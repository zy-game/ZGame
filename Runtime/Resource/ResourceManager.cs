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
        // private List<CacheData> cacheList = new List<CacheData>();
        private List<GameResourceModuleManifest> moduleList = new List<GameResourceModuleManifest>();
        private List<AssetBundleRuntimeHandle> bundleLists = new List<AssetBundleRuntimeHandle>();
        private Dictionary<string, IDisposable> loadAssetHandles = new Dictionary<string, IDisposable>();


        class CacheData : IDisposable
        {
            public float timeout;
            public AssetBundleRuntimeHandle bundle;

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
            Engine.Cache.RemoveCacheArea<AssetBundleRuntimeHandle>();
            // cacheList.ForEach(x => x.Dispose());
            // cacheList.Clear();
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
        internal void AddAssetBundleHandle(AssetBundleRuntimeHandle bundleHandle)
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
        internal AssetBundleRuntimeHandle GetRuntimeAssetBundleHandle(string module, string bundleName)
        {
            AssetBundleRuntimeHandle runtimeAssetBundleHandle = bundleLists.Find(x => x.name == bundleName && x.module == module);
            if (runtimeAssetBundleHandle is not null)
            {
                return runtimeAssetBundleHandle;
            }

            if (Engine.Cache.TryGetValue(bundleName, out runtimeAssetBundleHandle))
            {
                bundleLists.Add(runtimeAssetBundleHandle);
            }


            return runtimeAssetBundleHandle;
        }

        /// <summary>
        /// 获取资源模块版本数据
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <returns></returns>
        public GameResourceModuleManifest GetRuntimeModuleManifest(string moduleName)
        {
            GameResourceModuleManifest manifest = moduleList.Find(x => x.name == moduleName);
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
        public void AddModuleManifest(GameResourceModuleManifest manifest)
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
        public void RemoveModuleManifest(GameResourceModuleManifest manifest)
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
        public GameAssetBundleManifest GetRuntimeBundleManifest(string bundleName)
        {
            GameAssetBundleManifest gameAssetBundleManifest = default;
            foreach (var module in moduleList)
            {
                gameAssetBundleManifest = module.bundleList.Find(x => x.name == bundleName);
                if (gameAssetBundleManifest is not null)
                {
                    break;
                }
            }

            return gameAssetBundleManifest;
        }

        /// <summary>
        /// 获取资源包信息
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public GameAssetBundleManifest GetBundleManifestWithAssetPath(string assetPath)
        {
            foreach (var module in moduleList)
            {
                GameAssetBundleManifest bundleManifest = module.GetBundleManifestWithAsset(assetPath);
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
        public IResourceModuleLoaderScheduleHandle LoaderResourceModule(params ModuleOptions[] options)
        {
            IResourceModuleLoaderScheduleHandle resourceModuleLoaderScheduleHandle = IResourceModuleLoaderScheduleHandle.Create(options);
            resourceModuleLoaderScheduleHandle.Execute();
            return resourceModuleLoaderScheduleHandle;
        }

        /// <summary>
        /// 资源更新检查
        /// </summary>
        /// <param name="options">资源更新检查配置</param>
        /// <returns></returns>
        public ICheckResourceUpdateScheduleHandle CheckModuleResourceUpdate(params ModuleOptions[] options)
        {
            ICheckResourceUpdateScheduleHandle defaultCheckResourceUpdateScheduleHandle = ICheckResourceUpdateScheduleHandle.Create(options);
            defaultCheckResourceUpdateScheduleHandle.Execute();
            return defaultCheckResourceUpdateScheduleHandle;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public IRequestAssetObjectSchedule<T> LoadAsset<T>(string assetPath) where T : Object
        {
            IRequestAssetObjectSchedule<T> requestAssetObjectSchedule = IRequestAssetObjectSchedule<T>.Create(assetPath);
            requestAssetObjectSchedule.Execute();
            return requestAssetObjectSchedule;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public IRequestAssetObjectScheduleHandle<T> LoadAssetAsync<T>(string assetPath) where T : Object
        {
            if (loadAssetHandles.TryGetValue(assetPath, out IDisposable handle))
            {
                return (IRequestAssetObjectScheduleHandle<T>)handle;
            }

            loadAssetHandles.Add(assetPath, handle = IRequestAssetObjectScheduleHandle<T>.Create(assetPath));
            IRequestAssetObjectScheduleHandle<T> requestAssetObjectSchedule = (IRequestAssetObjectScheduleHandle<T>)handle;
            requestAssetObjectSchedule.Subscribe(ISubscriber.Create<IRequestAssetObjectScheduleHandle<T>>(args => { loadAssetHandles.Remove(assetPath); }));
            requestAssetObjectSchedule.Execute();
            return requestAssetObjectSchedule;
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="target">资源句柄</param>
        public void Release(Object target)
        {
            Engine.Console.Log("Release Asset Object ->", target.name);
            AssetBundleRuntimeHandle runtimeAssetObjectHandle = bundleLists.Find(x => x.Contains(target));
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
            Engine.Cache.Handle(runtimeAssetObjectHandle.name, runtimeAssetObjectHandle);
        }
    }
}