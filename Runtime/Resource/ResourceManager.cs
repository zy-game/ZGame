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
        private List<IRuntimeBundleHandle> _bundleLists = new List<IRuntimeBundleHandle>();
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

        /// <summary>
        /// 保存资源信息
        /// </summary>
        internal void SaveModuleData()
        {
            string str = Engine.Json.ToJson(moduleList);
            Engine.FileSystem.WriteFile("module", Encoding.UTF8.GetBytes(str), VersionOptions.None);
        }

        /// <summary>
        /// 添加资源包
        /// </summary>
        /// <param name="bundleHandle"></param>
        internal void AddAssetBundleHandle(IRuntimeBundleHandle bundleHandle)
        {
            _bundleLists.Add(bundleHandle);
        }

        /// <summary>
        /// 是否已加载指定的资源包
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        internal bool HasLoadAssetBundle(string name)
        {
            return GetRuntimeAssetBundleHandle(name) is not null;
        }

        /// <summary>
        /// 获取已加载的资源包
        /// </summary>
        /// <param name="bundleName"></param>
        /// <returns></returns>
        internal IRuntimeBundleHandle GetRuntimeAssetBundleHandle(string bundleName)
        {
            IRuntimeBundleHandle runtimeAssetBundleHandle = _bundleLists.Find(x => x.name == bundleName);
            if (runtimeAssetBundleHandle is null)
            {
                CacheTokenHandle cacheTokenHandle = cacheList.Find(x => x.name == bundleName);
                if (cacheTokenHandle is not null)
                {
                    runtimeAssetBundleHandle = Engine.Cache.Dequeue<IRuntimeBundleHandle>(cacheTokenHandle);
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
        /// <param name="bundleName">资源包名</param>
        /// <returns></returns>
        public RuntimeBundleManifest GetResourceBundleManifest(string bundleName)
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

        public RuntimeBundleManifest[] GetDifferenceBundleManifest(RuntimeModuleManifest runtimeModuleManifest, RuntimeModuleManifest compers)
        {
            List<RuntimeBundleManifest> differenceList = new List<RuntimeBundleManifest>();
            if (compers is null || compers.version != runtimeModuleManifest.version)
            {
                differenceList.AddRange(runtimeModuleManifest.bundleList);
            }
            else
            {
                for (int i = 0; i < runtimeModuleManifest.bundleList.Count; i++)
                {
                    RuntimeBundleManifest manifest = compers.bundleList.Find(x => x.Equals(runtimeModuleManifest.bundleList[i]));
                    if (manifest is not null)
                    {
                        continue;
                    }

                    differenceList.Add(manifest);
                }
            }

            return differenceList.ToArray();
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
        public IResourcePreloadExecuteHandle PreLoadResourceModule()
        {
            DefaultResourcePreloadExecuteHandle defaultResourcePreloadExecuteHandle = Engine.Class.Loader<DefaultResourcePreloadExecuteHandle>();
            defaultResourcePreloadExecuteHandle.Execute().StartCoroutine();
            return defaultResourcePreloadExecuteHandle;
        }

        /// <summary>
        /// 资源更新检查
        /// </summary>
        /// <param name="options">资源更新检查配置</param>
        /// <returns></returns>
        public ICheckUpdateExecuteHandle CheckUpdateResource(UpdateOptions options)
        {
            DefaultCheckUpdateExecuteHandle defaultCheckUpdateExecuteHandle = Engine.Class.Loader<DefaultCheckUpdateExecuteHandle>();
            defaultCheckUpdateExecuteHandle.Execute(options).StartCoroutine();
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
            asyncAssetRequestExecuteHandle.Execute(assetPath).StartCoroutine();
            return asyncAssetRequestExecuteHandle;
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="target">资源句柄</param>
        public void Release(Object target)
        {
            IRuntimeBundleHandle runtimeAssetObjectHandle = _bundleLists.Find(x => x.Contains(target));
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