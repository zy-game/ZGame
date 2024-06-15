using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Downloader;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.Language;
using ZGame.UI;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;

namespace ZGame.Resource
{
    partial class ResPackage
    {
        private static float compeleteCount;

        /// <summary>
        /// 已加载的包列表
        /// </summary>
        private static List<ResPackage> usegePackageList = new();

        /// <summary>
        /// 缓存池
        /// </summary>
        private static List<ResPackage> packageCache = new();

        /// <summary>
        /// 默认资源包
        /// </summary>
        public static ResPackage DEFAULT = ResPackage.Create("DEFAULT");

        static ResPackage Create(string title)
        {
            ResPackage self = RefPooled.Alloc<ResPackage>();
            self.name = title;
            self.isDefault = true;
            return self;
        }

        internal static ResPackage Create(AssetBundle bundle)
        {
            ResPackage self = RefPooled.Alloc<ResPackage>();
            self.bundle = bundle;
            self.isDefault = false;
            self.name = bundle.name;
            usegePackageList.Add(self);
            return self;
        }
#if UNITY_EDITOR
        internal static void OnDrawingGUI()
        {
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("资源包", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < usegePackageList.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(usegePackageList[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(usegePackageList[i].refCount.ToString());
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("资源包缓存池", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < packageCache.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(packageCache[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(packageCache[i].refCount.ToString());
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
#endif
        /// <summary>
        /// 检查未引用的对象
        /// </summary>
        internal static void CheckUnusedRefObject()
        {
            for (int i = usegePackageList.Count - 1; i >= 0; i--)
            {
                if (usegePackageList[i].refCount > 0)
                {
                    continue;
                }

                packageCache.Add(usegePackageList[i]);
                usegePackageList.RemoveAt(i);
            }
        }

        /// <summary>
        /// 清理缓存
        /// </summary>
        internal static void ReleaseUnusedRefObject()
        {
            for (int i = packageCache.Count - 1; i >= 0; i--)
            {
                if (packageCache[i].refCount > 0)
                {
                    continue;
                }

                RefPooled.Free(packageCache[i]);
            }

            packageCache.Clear();
            Resources.UnloadUnusedAssets();
        }

        internal static bool TryGetValue(string name, out ResPackage package)
        {
            package = usegePackageList.Find(x => x.name == name);
            if (package is null)
            {
                package = packageCache.Find(x => x.name == name);
                if (package is not null)
                {
                    packageCache.Remove(package);
                    usegePackageList.Add(package);
                }
            }

            return package is not null;
        }

        internal static void InitializedDependenciesPackage(params ResourcePackageManifest[] manifests)
        {
            for (int i = 0; i < manifests.Length; i++)
            {
                if (ResPackage.TryGetValue(manifests[i].name, out ResPackage target) is false)
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
                    if (ResPackage.TryGetValue(dependency, out ResPackage packageHandle) is false)
                    {
                        continue;
                    }

                    dependencies.Add(packageHandle);
                }

                target.SetDependencies(dependencies.ToArray());
            }
        }

        internal static void Unload(string name)
        {
            if (TryGetValue(name, out var package) is false)
            {
                return;
            }

            packageCache.Add(package);
            usegePackageList.Remove(package);
        }

        internal static async UniTask<Status> LoadBundleAsync(params ResourcePackageManifest[] manifests)
        {
            manifests = manifests.Where(x => x.name.EndsWith(".bytes") is false).ToArray();
            if (manifests is null || manifests.Length == 0)
            {
                return Status.Success;
            }


#if UNITY_EDITOR
            using (Watcher watcher = Watcher.Start(nameof(LoadBundleAsync)))
            {
#endif
                for (int i = 0; i < manifests.Length; i++)
                {
                    if (await LoadingAssetBundleAsync(manifests[i]) is not Status.Success)
                    {
                        manifests.Select(x => x.name).ToList().ForEach(x => ResPackage.Unload(x));
                        return Status.Fail;
                    }
                }

                ResPackage.InitializedDependenciesPackage(manifests);
                return Status.Success;
#if UNITY_EDITOR
            }
#endif
        }


        internal static Status LoadBundle(params ResourcePackageManifest[] manifests)
        {
            if (manifests is null || manifests.Length == 0)
            {
                return Status.Success;
            }
#if UNITY_EDITOR
            using (Watcher watcher = Watcher.Start(nameof(LoadBundle)))
            {
#endif
                for (int i = 0; i < manifests.Length; i++)
                {
                    if (LoadingAssetBundleSync(manifests[i]) is not Status.Success)
                    {
                        manifests.Select(x => x.name).ToList().ForEach(x => ResPackage.Unload(x));
                        return Status.Fail;
                    }
                }

                ResPackage.InitializedDependenciesPackage(manifests);
                return Status.Success;
#if UNITY_EDITOR
            }
#endif
        }


        private static DownloadConfiguration CurrentDownloadConfiguration = new DownloadConfiguration()
        {
            // usually, hosts support max to 8000 bytes, default value is 8000
            BufferBlockSize = 10240,
            // file parts to download, the default value is 1
            ChunkCount = 8,
            // download speed limited to 2MB/s, default values is zero or unlimited
            MaximumBytesPerSecond = 1024 * 1024 * 2,
            // the maximum number of times to fail
            MaxTryAgainOnFailover = 5,
            // release memory buffer after each 50 MB
            MaximumMemoryBufferBytes = 1024 * 1024 * 50,
            // download parts of the file as parallel or not. The default value is false
            ParallelDownload = true,
            // number of parallel downloads. The default value is the same as the chunk count
            ParallelCount = 4,
            // timeout (millisecond) per stream block reader, default values is 1000
            Timeout = 1000,
            // set true if you want to download just a specific range of bytes of a large file
            RangeDownload = false,
            // floor offset of download range of a large file
            RangeLow = 0,
            // ceiling offset of download range of a large file
            RangeHigh = 0,
            // clear package chunks data when download completed with failure, default value is false
            ClearPackageOnCompletionWithFailure = true,
            // minimum size of chunking to download a file in multiple parts, the default value is 512
            MinimumSizeOfChunking = 1024,
            // Before starting the download, reserve the storage space of the file as file size, the default value is false
            ReserveStorageSpaceBeforeStartingDownload = true,
            // config and customize request headers
            RequestConfiguration = new()
            {
                Accept = "*/*",
                Headers = new WebHeaderCollection(), // { your custom headers }
                KeepAlive = true, // default value is false
                ProtocolVersion = HttpVersion.Version11, // default value is HTTP 1.1
                UseDefaultCredentials = false,
                // your custom user agent or your_app_name/app_version.
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
            }
        };

        internal static async UniTask<Status> UpdatePackageAsync(params ResourcePackageManifest[] manifests)
        {
            if (manifests is null || manifests.Length == 0)
            {
                return Status.Success;
            }

            compeleteCount = 0;
            UILoading.SetTitle(AppCore.Language.Query(LanguageCode.DownloadPackageList));
            foreach (ResourcePackageManifest downloadItem in manifests)
            {
                if (await DownloadFile(downloadItem).ConfigureAwait(false) is not Status.Success)
                {
                    UIMsgBox.Show("更新资源失败", AppCore.Quit);
                    return Status.Fail;
                }

                compeleteCount++;
            }

            await UniTask.SwitchToMainThread();
            AppCore.Logger.Log("资源更新完成");
            UILoading.SetTitle(AppCore.Language.Query(LanguageCode.DownloadPackageListComplete));
            UILoading.SetProgress(1);
            return Status.Success;
        }

        private static async Task<Status> DownloadFile(ResourcePackageManifest downloadItem)
        {
            string fileUrl = AppCore.GetFileUrl(downloadItem.name);
            if (fileUrl.IsNullOrEmpty())
            {
                return Status.Fail;
            }

            IDownloadService CurrentDownloadService = CreateDownloadService(CurrentDownloadConfiguration);
            Stream stream = await CurrentDownloadService.DownloadFileTaskAsync(fileUrl).ConfigureAwait(false);
            if (CurrentDownloadService.Status is DownloadStatus.Completed)
            {
                await UniTask.SwitchToMainThread();
                await AppCore.File.WriteAsync(downloadItem.name, stream, downloadItem.version);
            }

            return CurrentDownloadService.Status == DownloadStatus.Completed ? Status.Success : Status.Fail;
        }

        private static DownloadService CreateDownloadService(DownloadConfiguration config)
        {
            var downloadService = new DownloadService(config);
            downloadService.DownloadProgressChanged += OnDownloadProgressChanged;
            return downloadService;
        }

        private static void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            UILoading.SetProgress((float)e.ProgressPercentage / 100f + compeleteCount);
        }

        static Status LoadingAssetBundleSync(ResourcePackageManifest manifest)
        {
            if (ResPackage.TryGetValue(manifest.name, out ResPackage _))
            {
                AppCore.Logger.Log("资源包已加载：" + manifest.name);
                return Status.Success;
            }

            byte[] bytes = AppCore.File.Read(manifest.name);
            if (bytes == null)
            {
                return Status.Fail;
            }

            AppCore.Logger.Log("资源包加载完成：" + manifest.name);
            return ResPackage.Create(AssetBundle.LoadFromMemory(bytes)).IsSuccess() ? Status.Success : Status.Fail;
        }

        static async UniTask<Status> LoadingAssetBundleAsync(ResourcePackageManifest manifest)
        {
            if (ResPackage.TryGetValue(manifest.name, out ResPackage _))
            {
                AppCore.Logger.Log("资源包已加载：" + manifest.name);
                return Status.Success;
            }

            AssetBundle bundle = default;
#if UNITY_WEBGL
            using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(AppCore.GetFileUrl(manifest.name), manifest.version))
            {
                await request.SendWebRequest();
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    return Status.Fail;
                }

                bundle = DownloadHandlerAssetBundle.GetContent(request);
            }
#else
            byte[] bytes = await AppCore.File.ReadAsync(manifest.name);
            if (bytes == null || bytes.Length == 0)
            {
                AppCore.Logger.Log("资源包不存在：" + manifest.name);
                return Status.Fail;
            }

            bundle = await AssetBundle.LoadFromMemoryAsync(bytes);
#endif
            AppCore.Logger.Log("资源包加载完成：" + manifest.name);
            return ResPackage.Create(bundle).IsSuccess() ? Status.Success : Status.Fail;
        }
    }
}