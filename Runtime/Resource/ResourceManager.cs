using System.Collections.Generic;
using System.Text;
using UnityEngine;
using ZEngine.Options;
using ZEngine.VFS;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManager : Single<ResourceManager>
    {
        private List<BundleData> _bundleLists = new List<BundleData>();
        private List<ResourceModuleOptions> moduleList = new List<ResourceModuleOptions>();
        private Dictionary<string, IExecuteAsyncHandle> loadAssetHandles = new Dictionary<string, IExecuteAsyncHandle>();

        class BundleData : IReference
        {
            public int refCount;
            public AssetBundle bundle;
            public List<int> objCode;

            public void Release()
            {
            }
        }

        public ResourceManager()
        {
            Subscribe.Create(Update).Timer(ResourceOptions.instance.refershIntervalTime);
            IReadFileExecuteHandle readFileExecuteHandle = Engine.FileSystem.ReadFile("module");
            if (readFileExecuteHandle.EnsureExecuteSuccessfuly() is false)
            {
                return;
            }

            moduleList = Engine.Json.Parse<List<ResourceModuleOptions>>(Encoding.UTF8.GetString(readFileExecuteHandle.bytes));
        }

        private void Update()
        {
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
        public ICheckResourceUpdateExecuteHandle CheckUpdateResource(ResourceCheckUpdateOptions options)
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
        public ResourceModuleOptions GetResourceModuleVersion(string moduleName)
        {
            return moduleList.Find(x => x.moduleName == moduleName);
        }

        /// <summary>
        /// 获取资源包版本数据
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <param name="bundleName">资源包名</param>
        /// <returns></returns>
        public ResourceBundleOptions GetResourceBundleVersion(string moduleName, string bundleName)
        {
            ResourceModuleOptions moduleOptions = GetResourceModuleVersion(moduleName);
            if (moduleOptions is null || moduleOptions.bundleList is null)
            {
                return default;
            }

            return moduleOptions.bundleList.Find(x => x.bundleName == bundleName);
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
            loadGameAssetExecuteHandle.Subscribe(Subscribe.Create((() => { loadAssetHandles.Remove(assetPath); })));
            loadAssetHandles.Add(assetPath, loadGameAssetExecuteHandle);
            loadGameAssetExecuteHandle.Execute(assetPath);
            return loadGameAssetExecuteHandle;
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="handle">资源句柄</param>
        public void Release(Object handle)
        {
            BundleData bundleData = _bundleLists.Find(x => x.objCode.Contains(handle.GetInstanceID()));
            if (bundleData is null)
            {
                Engine.Console.Error("Not Find The Object Bundle");
                return;
            }

            
        }
    }
}