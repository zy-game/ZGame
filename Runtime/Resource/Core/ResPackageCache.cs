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

namespace ZGame.Resource
{
    class ResPackageCache : Singleton<ResPackageCache>
    {
        private float nextCheckTime;
        private List<ResPackage> cacheList = new List<ResPackage>();
        private List<ResPackage> _packageList = new List<ResPackage>();

        protected override void OnAwake()
        {
            BehaviourScriptable.instance.SetupUpdateEvent(OnUpdate);
        }


        private void OnUpdate()
        {
            if (Time.realtimeSinceStartup < nextCheckTime)
            {
                return;
            }

            nextCheckTime = Time.realtimeSinceStartup + BasicConfig.instance.resTimeout;
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

        public async UniTask UpdateResourcePackageList(EntryConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException("config");
            }

            UILoading.SetTitle(Localliztion.instance.Query("正在加载资源信息..."));
            UILoading.SetProgress(0);
            List<ResourcePackageManifest> manifests = PackageManifestManager.instance.CheckNeedUpdatePackageList(config.module);

            if (manifests is not null && manifests.Count > 0)
            {
                Debug.Log("需要更新资源：" + string.Join(",", manifests.Select(x => x.name)));
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

                if (failure.Count > 0)
                {
                    Debug.LogError($"Download failure:{string.Join(",", failure.ToArray())}");
                    UIMsgBox.Show("更新资源失败", GameManager.instance.QuitGame);
                    return;
                }
            }

            Debug.Log("资源更新完成");
            UILoading.SetTitle(Localliztion.instance.Query("资源更新完成..."));
            UILoading.SetProgress(1);
        }

        public async UniTask LoadingResourcePackageList(params ResourcePackageManifest[] manifests)
        {
            if (manifests is null || manifests.Length == 0)
            {
                throw new ArgumentNullException("manifests");
            }

            for (int i = 0; i < manifests.Length; i++)
            {
                UILoading.SetProgress(i / (float)manifests.Length);
                if (ResPackageCache.instance.TryGetValue(manifests[i].name, out _))
                {
                    Debug.Log("资源包已加载：" + manifests[i].name);
                    continue;
                }

                AssetBundle assetBundle = default;
                byte[] bytes = await VFSManager.instance.ReadAsync(manifests[i].name);
                if (bytes is null || bytes.Length == 0)
                {
                    ResPackageCache.instance.Clear(manifests.ToArray());
                    UILoading.SetTitle(Localliztion.instance.Query("资源加载失败..."));
                    return;
                }

                assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                ResPackageCache.instance.Add(new ResPackage(assetBundle));
                Debug.Log("资源包加载完成：" + manifests[i].name);
            }

            for (int i = 0; i < manifests.Length; i++)
            {
                if (ResPackageCache.instance.TryGetValue(manifests[i].name, out var target) is false)
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
                    if (ResPackageCache.instance.TryGetValue(dependency, out var packageHandle) is false)
                    {
                        continue;
                    }

                    dependencies.Add(packageHandle);
                }

                target.SetDependencies(dependencies.ToArray());
            }

            UILoading.SetTitle(Localliztion.instance.Query("资源加载完成..."));
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
                if (ResPackageCache.instance.TryGetValue(manifests[i].name, out _))
                {
                    Debug.Log("资源包已加载：" + manifests[i].name);
                    continue;
                }

                AssetBundle assetBundle = default;
                byte[] bytes = VFSManager.instance.Read(manifests[i].name);
                if (bytes is null || bytes.Length == 0)
                {
                    ResPackageCache.instance.Clear(manifests.ToArray());
                    UILoading.SetTitle(Localliztion.instance.Query("资源加载失败..."));
                    return;
                }

                assetBundle = AssetBundle.LoadFromMemory(bytes);
                ResPackageCache.instance.Add(new ResPackage(assetBundle));
                Debug.Log("资源包加载完成：" + manifests[i].name);
            }

            for (int i = 0; i < manifests.Length; i++)
            {
                if (ResPackageCache.instance.TryGetValue(manifests[i].name, out var target) is false)
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
                    if (ResPackageCache.instance.TryGetValue(dependency, out var packageHandle) is false)
                    {
                        continue;
                    }

                    dependencies.Add(packageHandle);
                }

                target.SetDependencies(dependencies.ToArray());
            }

            UILoading.SetTitle(Localliztion.instance.Query("资源加载完成..."));
            UILoading.SetProgress(1);
        }
    }
}