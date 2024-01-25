using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
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
        public async UniTask PerloadingResourcePackageList(EntryConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException("config");
            }

            await UpdateResPackageAsync(config.module);
            await LoadPackageAsync(config.module);
        }

        /// <summary>
        /// 加载资源包列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async UniTask LoadPackageAsync(string configName)
        {
            if (configName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("configName");
            }

            UILoading.SetTitle("正在加载资源信息...");
            UILoading.SetProgress(0);
            List<ResourcePackageManifest> manifests = PackageManifestManager.instance.GetResourcePackageAndDependencyList(configName);
            if (manifests is null || manifests.Count == 0)
            {
                UILoading.SetTitle("资源加载完成...");
                UILoading.SetProgress(1);
            }

            await ResPackageCache.instance.LoadAsync(manifests.ToArray());
        }

        /// <summary>
        /// 加载资源模块
        /// </summary>
        /// <param name="configName"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public void LoadPackageSync(string configName)
        {
            if (configName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("configName");
            }

            List<ResourcePackageManifest> manifests = PackageManifestManager.instance.GetResourcePackageAndDependencyList(configName);
            if (manifests is null || manifests.Count == 0)
            {
                UILoading.SetTitle("资源加载完成...");
                UILoading.SetProgress(1);
            }

            ResPackageCache.instance.LoadSync(manifests.ToArray());
        }

        /// <summary>
        /// 更新资源列表
        /// </summary>
        /// <param name="progressCallback"></param>
        /// <param name="args"></param>
        /// <exception cref="NullReferenceException"></exception>
        public async UniTask UpdateResPackageAsync(string configName)
        {
            if (configName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("configName");
            }

            UILoading.SetTitle("检查资源配置...");
            UILoading.SetProgress(0);
            List<ResourcePackageManifest> manifests = PackageManifestManager.instance.CheckNeedUpdatePackageList(configName);
            if (manifests is null || manifests.Count == 0)
            {
                UILoading.SetTitle("资源更新完成...");
                UILoading.SetProgress(1);
            }

            HashSet<ResourcePackageManifest> downloadList = new HashSet<ResourcePackageManifest>();
            HashSet<string> failure = new HashSet<string>();
            foreach (var packageManifest in manifests)
            {
                if (downloadList.Contains(packageManifest))
                {
                    continue;
                }

                string url = OSSConfig.instance.GetFilePath(packageManifest.name);
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = 5;
                    request.useHttpContinue = true;
                    request.disposeUploadHandlerOnDispose = true;
                    request.disposeDownloadHandlerOnDispose = true;
                    UILoading.SetTitle(Path.GetFileName(url));
                    await request.SendWebRequest().ToUniTask(UILoading.Show());
                    UILoading.SetProgress(1);
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        failure.Add(packageManifest.name);
                        continue;
                    }

                    await VFSManager.instance.WriteAsync(Path.GetFileName(url), request.downloadHandler.data, packageManifest.version);
                }
            }

            if (failure.Count == 0)
            {
                UILoading.SetTitle("资源更新完成...");
                UILoading.SetProgress(1);
                return;
            }

            Debug.LogError($"Download failure:{string.Join(",", failure.ToArray())}");
            UIMsgBox.Show("更新资源失败", GameManager.instance.QuitGame);
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