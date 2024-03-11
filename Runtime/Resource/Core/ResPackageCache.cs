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
using ZGame.Module;

namespace ZGame.Resource
{
    internal class ResPackageCache : IModule
    {
        private float nextCheckTime;
        private List<ResPackage> cacheList = new List<ResPackage>();
        private List<ResPackage> _packageList = new List<ResPackage>();

        public void OnAwake()
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

        public void Dispose()
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

        public async UniTask<bool> UpdateResourcePackageList(GameConfig config)
        {
            if (config is null)
            {
                throw new ArgumentNullException("config");
            }

            UILoading.SetTitle(WorkApi.Language.Query("正在加载资源信息..."));
            UILoading.SetProgress(0);
            List<ResourcePackageManifest> manifests = WorkApi.Resource.PackageManifest.CheckNeedUpdatePackageList(config.module);
            if (manifests is not null && manifests.Count > 0)
            {
                bool updateState = await DownloadUpdateResourceList(manifests.ToArray());
                if (updateState is false)
                {
                    UIMsgBox.Show("更新资源失败", WorkApi.Uninitialized);
                    return false;
                }
            }

            Debug.Log("资源更新完成");
            UILoading.SetTitle(WorkApi.Language.Query("资源更新完成..."));
            UILoading.SetProgress(1);
            return true;
        }

        private async UniTask<bool> DownloadUpdateResourceList(params ResourcePackageManifest[] manifests)
        {
            UILoading.SetTitle(WorkApi.Language.Query("更新资源列表中..."));
            // var downloadOpt = new DownloadConfiguration()
            // {
            //     ChunkCount = 1, // file parts to download, the default value is 1
            //     ParallelDownload = true // download parts of the file as parallel or not. The default value is false
            // };
            bool[] state = new bool[manifests.Length];
            Debug.Log("需要更新资源：" + string.Join(",", manifests.Select(x => x.name)));
            UniTask[] tasks = new UniTask[manifests.Length];
            for (int i = 0; i < manifests.Length; i++)
            {
                int index = i;
                string url = OSSConfig.instance.GetFilePath(manifests[index].name);
                // var downloader = new DownloadService(downloadOpt);
                // downloader.DownloadStarted += (s, e) => { Debug.Log($"开始下载：{url}"); };
                // downloader.DownloadProgressChanged += (s, e) => { UILoading.SetProgress((float)e.ProgressPercentage); };
                // downloader.DownloadFileCompleted += (s, e) => { Debug.Log($"下载{(downloader.Status == DownloadStatus.Completed ? "成功" : "失败")}：{url}"); };
                tasks[i] = UniTask.Create(async () =>
                {
                    using (UnityWebRequest request = UnityWebRequest.Get(url))
                    {
                        await request.SendWebRequest().ToUniTask();
                        if (request.isNetworkError || request.isHttpError)
                        {
                            Debug.LogErrorFormat("资源下载失败：{0}", url);
                            return;
                        }
                        state[index] = true;
                        WorkApi.VFS.Write(manifests[index].name, request.downloadHandler.data, manifests[index].version);
                        Debug.LogFormat("资源下载完成：{0} Lenght:{1}", url, request.downloadHandler.data.Length);
                    }
                    // using (Stream stream = await downloader.DownloadFileTaskAsync(url))
                    // {
                    //     if (downloader.Status is not DownloadStatus.Completed)
                    //     {
                    //         Debug.LogErrorFormat("资源下载失败：{0}", url);
                    //         return;
                    //     }
                    //
                    //     state[index] = true;
                    //     using (MemoryStream memoryStream = new MemoryStream())
                    //     {
                    //         await stream.CopyToAsync(memoryStream);
                    //         await memoryStream.FlushAsync();
                    //         byte[] bytes = memoryStream.ToArray();
                    //         await WorkApi.VFS.WriteAsync(manifests[index].name, bytes, manifests[index].version);
                    //         downloader.Dispose();
                    //         Debug.LogFormat("资源下载完成：{0} Lenght:{1}", url, bytes.Length);
                    //     }
                    // }
                });
            }

            Extension.StartSample();
            await UniTask.WhenAll(tasks);
            Debug.LogFormat("更新完成，总耗时：{0}", Extension.GetSampleTime());
            return state.Where(x => x is false).Count() == 0;
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
                byte[] bytes = await WorkApi.VFS.ReadAsync(manifests[i].name);
                if (bytes is null || bytes.Length == 0)
                {
                    Clear(manifests.ToArray());
                    UILoading.SetTitle(WorkApi.Language.Query("资源加载失败..."));
                    return;
                }

                Debug.LogFormat("已加载资源文件数据：{0} Lenght:{1}", manifests[i].name, bytes.Length);
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
            UILoading.SetTitle(WorkApi.Language.Query("资源加载完成..."));
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
                byte[] bytes = WorkApi.VFS.Read(manifests[i].name);
                if (bytes is null || bytes.Length == 0)
                {
                    Clear(manifests.ToArray());
                    UILoading.SetTitle(WorkApi.Language.Query("资源加载失败..."));
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

            UILoading.SetTitle(WorkApi.Language.Query("资源加载完成..."));
            UILoading.SetProgress(1);
        }
    }
}