using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
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
        public override void Dispose()
        {
            base.Dispose();
            ZGame.Cache.RemoveCacheArea<RuntimeAssetBundleHandle>();
            ZGame.Datable.Clear<ResourceModuleManifest>();
            ZGame.Datable.Clear<RuntimeAssetBundleHandle>();
            ZGame.Console.Log("释放所有资源");
        }

        /// <summary>
        /// 预加载资源模块
        /// </summary>
        /// <param name="options">预加载配置</param>
        public UniTask<IRequestResourceModuleLoadResult> LoadingResourceModule(IProgressHandle gameProgressHandle, params ModuleOptions[] options)
        {
            return IRequestResourceModuleLoadResult.Create(gameProgressHandle, options);
        }

        /// <summary>
        /// 加载网络资源包
        /// </summary>
        /// <param name="progressHandle"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public UniTask<IRequestNetworkResourceBundleResult> LoadNetworkResourceBundleAsync(IProgressHandle progressHandle, params string[] args)
        {
            return IRequestNetworkResourceBundleResult.Create(progressHandle, args);
        }

        /// <summary>
        /// 资源更新检查
        /// </summary>
        /// <param name="options">资源更新检查配置</param>
        /// <returns></returns>
        public UniTask<IRequestResourceModuleUpdateResult> CheckModuleResourceUpdate(IProgressHandle gameProgressHandle, params ModuleOptions[] options)
        {
            return IRequestResourceModuleUpdateResult.Create(gameProgressHandle, options);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public IRequestResourceObjectResult LoadAsset(string assetPath)
        {
            return IRequestResourceObjectResult.Create(assetPath);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        /// <returns></returns>
        public UniTask<IRequestResourceObjectResult> LoadAssetAsync(string assetPath)
        {
            return IRequestResourceObjectResult.CreateAsync(assetPath);
        }

        /// <summary>
        /// 回收资源
        /// </summary>
        /// <param name="target">资源句柄</param>
        public void Release(Object target)
        {
            ZGame.Console.Log("Release Asset Object ->", target.name);
            RuntimeAssetBundleHandle runtimeAssetBundleHandle = ZGame.Datable.GetDatable<RuntimeAssetBundleHandle>(x => x.Contains(target));
            if (runtimeAssetBundleHandle is null)
            {
                return;
            }

            runtimeAssetBundleHandle.Unload(target);
            if (runtimeAssetBundleHandle.refCount is not 0)
            {
                return;
            }

            ZGame.Datable.Release(runtimeAssetBundleHandle.name, true);
        }
    }
}