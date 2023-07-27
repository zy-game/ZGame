using UnityEngine;
using ZEngine.Options;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public class ResourceManager : Single<ResourceManager>
    {
        /// <summary>
        /// 预加载资源模块
        /// </summary>
        /// <param name="module">模块名</param>
        public IResourcePreloadExecuteHandle PreLoadResourceModule(ResourcePreloadOptions options)
        {
            return default;
        }

        public ICheckUpdateExecuteHandle CheckUpdateResource(ResourceCheckUpdateOptions options)
        {
            return default;
        }

        /// <summary>
        /// 获取资源模块版本数据
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <returns></returns>
        public ResourceModuleVersion GetResourceModuleVersion(string moduleName)
        {
            return default;
        }

        /// <summary>
        /// 获取资源包版本数据
        /// </summary>
        /// <param name="moduleName">模块名</param>
        /// <param name="bundleName">资源包名</param>
        /// <returns></returns>
        public ResourceBundleVersion GetResourceBundleVersion(string moduleName, string bundleName)
        {
            return default;
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public ResContext LoadAsset(string assetPath)
        {
            return default;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public IGameAsyncExecuteHandle<ResContext> LoadAssetAsync(string assetPath)
        {
            return default;
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="handle">资源句柄</param>
        public void Release(ResContext handle)
        {
        }
    }
}