using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using ZGame.Config;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Networking;
using ZGame.Resource.Config;
using ZGame.UI;

namespace ZGame.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public sealed class ResourceManager : Singleton<ResourceManager>
    {
        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        public ResObject LoadAsset(string path, string extension = "")
        {
            // UniTask<ResObject> task = ResObjectCache.instance.LoadAsync(path, extension);
            // UniTask<ResObject>.Awaiter awaiter = task.GetAwaiter();
            // task.Forget();
            return ResObjectCache.instance.LoadSync(path, extension);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        public async UniTask<ResObject> LoadAssetAsync(string path, string extension = "")
        {
            return await ResObjectCache.instance.LoadAsync(path, extension);
        }

        /// <summary>
        /// 预加载资源包列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async UniTask PreloadingResourcePackageList(EntryConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException("config");
            }

            await ResPackageCache.instance.UpdateResourcePackageList(config);
            UILoading.SetTitle(Localliztion.instance.Query("正在加载资源信息..."));
            UILoading.SetProgress(0);
            List<ResourcePackageManifest> manifests = PackageManifestManager.instance.GetResourcePackageAndDependencyList(config.module);
            if (manifests is null || manifests.Count == 0)
            {
                UILoading.SetTitle(Localliztion.instance.Query("资源加载完成..."));
                UILoading.SetProgress(1);
                return;
            }

            Debug.Log("加载资源：" + string.Join(",", manifests.Select(x => x.name)));
            await ResPackageCache.instance.LoadingResourcePackageList(manifests.ToArray());
        }

        /// <summary>
        /// 卸载资源包列表
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="isUnloadDependenecis"></param>
        public void UnloadPackageList(string packageName, bool isUnloadDependenecis = true)
        {
        }
    }
}