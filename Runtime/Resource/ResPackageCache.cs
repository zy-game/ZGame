using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.Config;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Resource.Config;
using ZGame.UI;
using System.Linq;
using System.Threading.Tasks;
using Downloader;

namespace ZGame.Resource
{
    internal class ResPackageCache : GameFrameworkModule
    {
        private float nextCheckTime;
        private List<ResPackage> cacheList = new List<ResPackage>();
        private List<ResPackage> _packageList = new List<ResPackage>();

        public override void OnAwake()
        {
            BehaviourScriptable.instance.SetupUpdateEvent(OnUpdate);
        }


        private void OnUpdate()
        {
            if (Time.realtimeSinceStartup < nextCheckTime)
            {
                return;
            }

            nextCheckTime = Time.realtimeSinceStartup + ResConfig.instance.timeout;
            CheckCanUnloadPackage();
            UnloadPackage();
        }

        private void CheckCanUnloadPackage()
        {
            ResPackage handle = default;
            for (int i = _packageList.Count - 1; i >= 0; i--)
            {
                handle = _packageList[i];
                if (handle.CanUnloadPackage() is false)
                {
                    continue;
                }

                Debug.Log("资源包:" + handle.name + "准备卸载");
                cacheList.Add(handle);
                _packageList.Remove(handle);
            }
        }

        private void UnloadPackage()
        {
            for (int i = cacheList.Count - 1; i >= 0; i--)
            {
                if (cacheList[i].CanUnloadPackage() is false)
                {
                    continue;
                }

                Remove(cacheList[i].name);
            }
        }

        public void Add(ResPackage handle)
        {
            _packageList.Add(handle);
        }

        public void Remove(string packageName)
        {
            ResPackage handle = _packageList.Find(x => x.name == packageName);
            if (handle is not null)
            {
                _packageList.Remove(handle);
                handle.Dispose();
            }

            handle = cacheList.Find(x => x.name == packageName);
            if (handle is null)
            {
                return;
            }

            cacheList.Remove(handle);
            handle.Dispose();
        }

        public bool TryGetValue(string packageName, out ResPackage handle)
        {
            handle = default;
            if ((handle = _packageList.Find(x => x.name == packageName)) is not null)
            {
                return true;
            }

            if ((handle = cacheList.Find(x => x.name == packageName)) is not null)
            {
                cacheList.Remove(handle);
                _packageList.Add(handle);
                return true;
            }

            return false;
        }

        public override void Dispose()
        {
            foreach (var VARIABLE in _packageList)
            {
                VARIABLE.Dispose();
            }

            _packageList.Clear();
            foreach (var VARIABLE in cacheList)
            {
                VARIABLE.Dispose();
            }

            cacheList.Clear();
        }

        public void Clear(params ResourcePackageManifest[] fileList)
        {
            foreach (var VARIABLE in fileList)
            {
                Remove(VARIABLE.name);
            }
        }

        public async UniTask<bool> UpdateResourcePackageList(string packageName)
        {
            if (packageName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("config");
            }

            UILoading.SetTitle(GameFrameworkEntry.Language.Query("正在加载资源信息..."));
            UILoading.SetProgress(0);
            List<ResourcePackageManifest> manifests = GameFrameworkEntry.Resource.PackageManifest.CheckNeedUpdatePackageList(packageName);
            if (manifests is not null && manifests.Count > 0)
            {
                bool updateState = await DownloadUpdateResourceList(manifests.ToArray());
                if (updateState is false)
                {
                    UIMsgBox.Show("更新资源失败", GameFrameworkEntry.Quit);
                    return false;
                }
            }

            Debug.Log("资源更新完成");
            UILoading.SetTitle(GameFrameworkEntry.Language.Query("资源更新完成..."));
            UILoading.SetProgress(1);
            return true;
        }

        private async UniTask<bool> DownloadUpdateResourceList(params ResourcePackageManifest[] manifests)
        {
            UILoading.SetTitle(GameFrameworkEntry.Language.Query("更新资源列表中..."));

            bool[] downloadStateList = new bool[manifests.Length];
            Debug.Log("需要更新资源：" + string.Join(",", manifests.Select(x => x.name)));
            UniTask<bool>[] downloadAssetTaskList = new UniTask<bool>[manifests.Length];
            for (int i = 0; i < manifests.Length; i++)
            {
                string url = OSSConfig.instance.GetFilePath(manifests[i].name);
                downloadAssetTaskList[i] = GameFrameworkEntry.Download.DownloadStreamingAsset(url, manifests[i].name, manifests[i].version);
            }

            Extension.StartSample();
            var result = await UniTask.WhenAll(downloadAssetTaskList);
            Debug.LogFormat("更新完成，总耗时：{0}", Extension.GetSampleTime());
            return result.Where(x => x is false).Count() == 0;
        }

        public async UniTask LoadingResourcePackageListAsync(params ResourcePackageManifest[] manifests)
        {
            if (manifests is null || manifests.Length == 0)
            {
                throw new ArgumentNullException("manifests");
            }

            Extension.StartSample();
            for (int i = 0; i < manifests.Length; i++)
            {
                UILoading.SetProgress(i / (float)manifests.Length);
                if (TryGetValue(manifests[i].name, out _))
                {
                    Debug.Log("资源包已加载：" + manifests[i].name);
                    continue;
                }

                AssetBundle assetBundle = default;
                byte[] bytes = await GameFrameworkEntry.VFS.ReadAsync(manifests[i].name);
                if (bytes is null || bytes.Length == 0)
                {
                    Clear(manifests.ToArray());
                    UILoading.SetTitle(GameFrameworkEntry.Language.Query("资源加载失败..."));
                    return;
                }

                assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                Add(new ResPackage(assetBundle));
                Debug.Log("资源包加载完成：" + manifests[i].name);
            }

            for (int i = 0; i < manifests.Length; i++)
            {
                if (TryGetValue(manifests[i].name, out var target) is false)
                {
                    continue;
                }

                if (manifests[i].dependencies is null || manifests[i].dependencies.Length == 0)
                {
                    continue;
                }

                List<ResPackage> dependencies = new List<ResPackage>();
                foreach (var dependency in manifests[i].dependencies)
                {
                    if (TryGetValue(dependency, out var packageHandle) is false)
                    {
                        continue;
                    }

                    dependencies.Add(packageHandle);
                }

                target.SetDependencies(dependencies.ToArray());
            }

            Debug.Log($"资源加载完成，总耗时：{Extension.GetSampleTime()}");
            UILoading.SetTitle(GameFrameworkEntry.Language.Query("资源加载完成..."));
            UILoading.SetProgress(1);
        }

        public void LoadingResourcePackageListSync(params ResourcePackageManifest[] manifests)
        {
            if (manifests is null || manifests.Length is 0)
            {
                throw new ArgumentNullException("manifests");
            }
    
            for (int i = 0; i < manifests.Length; i++)
            {
                UILoading.SetProgress(i / (float)manifests.Length);
                if (TryGetValue(manifests[i].name, out _))
                {
                    Debug.Log("资源包已加载：" + manifests[i].name);
                    continue;
                }

                AssetBundle assetBundle = default;
                byte[] bytes = GameFrameworkEntry.VFS.Read(manifests[i].name);
                if (bytes is null || bytes.Length == 0)
                {
                    Clear(manifests.ToArray());
                    UILoading.SetTitle(GameFrameworkEntry.Language.Query("资源加载失败..."));
                    return;
                }

                assetBundle = AssetBundle.LoadFromMemory(bytes);
                Add(new ResPackage(assetBundle));
                Debug.Log("资源包加载完成：" + manifests[i].name);
            }

            for (int i = 0; i < manifests.Length; i++)
            {
                if (TryGetValue(manifests[i].name, out var target) is false)
                {
                    continue;
                }

                if (manifests[i].dependencies is null || manifests[i].dependencies.Length == 0)
                {
                    continue;
                }

                List<ResPackage> dependencies = new List<ResPackage>();
                foreach (var dependency in manifests[i].dependencies)
                {
                    if (TryGetValue(dependency, out var packageHandle) is false)
                    {
                        continue;
                    }

                    dependencies.Add(packageHandle);
                }

                target.SetDependencies(dependencies.ToArray());
            }

            UILoading.SetTitle(GameFrameworkEntry.Language.Query("资源加载完成..."));
            UILoading.SetProgress(1);
        }
    }
}