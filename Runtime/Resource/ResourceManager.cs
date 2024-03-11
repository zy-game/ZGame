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
using ZGame.Module;
using ZGame.Networking;
using ZGame.Resource.Config;
using ZGame.UI;

namespace ZGame.Resource
{
    /// <summary>
    /// 资源管理器
    /// </summary>
    public sealed class ResourceManager : IModule
    {
        internal ResPackageCache ResPackageCache { get; set; }
        internal ResObjectCache ResObjectCache { get; set; }
        internal PackageManifestManager PackageManifest { get; set; }

        public void OnAwake()
        {
            ResPackageCache = new ResPackageCache();
            ResPackageCache.OnAwake();
            ResObjectCache = new ResObjectCache();
            ResObjectCache.OnAwake();
            PackageManifest = new PackageManifestManager();
            PackageManifest.OnAwake();
        }

        public void Dispose()
        {
            ResPackageCache.Dispose();
            ResObjectCache.Dispose();
            PackageManifest.Dispose();
            ResPackageCache = null;
            ResObjectCache = null;
            PackageManifest = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载结果</returns>
        public ResObject LoadAsset(string path, string extension = "")
        {
            return ResObjectCache.LoadSync(path, extension);
        }

        /// <summary>
        /// 加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源加载任务</returns>
        public async UniTask<ResObject> LoadAssetAsync(string path, string extension = "")
        {
            return await ResObjectCache.LoadAsync(path, extension);
        }

        /// <summary>
        /// 预加载资源包列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async UniTask<bool> PreloadingResourcePackageList(GameConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException("config");
            }

            Extension.StartSample();
            await PackageManifest.SetupPackageManifest(BasicConfig.instance.curGame.module);
            bool state = await ResPackageCache.UpdateResourcePackageList(config);
            if (state is false)
            {
                return false;
            }

            UILoading.SetTitle(WorkApi.Language.Query("正在加载资源信息..."));
            UILoading.SetProgress(0);
            List<ResourcePackageManifest> manifests = PackageManifest.GetResourcePackageAndDependencyList(config.module);
            if (manifests is null || manifests.Count == 0)
            {
                UILoading.SetTitle(WorkApi.Language.Query("资源加载完成..."));
                UILoading.SetProgress(1);
                return true;
            }

            Debug.Log("加载资源：" + string.Join(",", manifests.Select(x => x.name)));
            await ResPackageCache.LoadingResourcePackageListAsync(manifests.ToArray());
            Debug.Log($"资源预加载完成，总计耗时：{Extension.GetSampleTime()}");
            return true;
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