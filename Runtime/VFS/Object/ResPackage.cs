using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Resource.Config;
using ZGame.UI;
using ZGame.VFS;

namespace ZGame.Resource
{
    class ResPackage : IGameCacheObject
    {
        public string name { get; private set; }
        public int refCount { get; private set; }
        public AssetBundle bundle { get; private set; }
        public ResPackage[] dependencies { get; private set; }

        private bool isDefault;
        private float nextCheckTime;


        internal static ResPackage Create(string title)
        {
            ResPackage self = GameFrameworkFactory.Spawner<ResPackage>();
            self.name = title;
            self.isDefault = true;
            return self;
        }

        internal static ResPackage Create(AssetBundle bundle)
        {
            ResPackage self = GameFrameworkFactory.Spawner<ResPackage>();
            self.bundle = bundle;
            self.isDefault = false;
            self.name = bundle.name;
            return self;
        }

        internal void SetDependencies(params ResPackage[] dependencies)
        {
            this.dependencies = dependencies;
            Debug.Log(name + " 设置引用资源包：" + string.Join(",", dependencies.Select(x => x.name).ToArray()));
        }

        public bool IsSuccess()
        {
            return bundle != null;
        }

        public void Ref()
        {
            refCount++;
            if (dependencies is null || dependencies.Length == 0)
            {
                return;
            }

            foreach (var VARIABLE in dependencies)
            {
                VARIABLE.Ref();
            }
        }

        public void Unref()
        {
            refCount--;
            if (dependencies is null || dependencies.Length == 0)
            {
                return;
            }

            foreach (var VARIABLE in dependencies)
            {
                VARIABLE.Unref();
            }
        }

        public void Release()
        {
            bundle?.Unload(true);
            Resources.UnloadUnusedAssets();
            Debug.Log("释放资源包:" + name);
        }


        internal static async UniTask<Status> UpdateResourcePackageList(params ResourcePackageManifest[] manifests)
        {
            if (manifests is null || manifests.Length == 0)
            {
                return Status.Success;
            }

            UILoading.SetProgress(0);
            UILoading.SetTitle(GameFrameworkEntry.Language.Query("更新资源列表中..."));
            using (DownloadGroup downloadGroup = DownloadGroup.Create(x => { UILoading.SetProgress(x.progress); }))
            {
                foreach (ResourcePackageManifest manifest in manifests)
                {
                    downloadGroup.Add(OSSConfig.instance.GetFilePath(manifest.name), manifest.version, null);
                }

                if (await downloadGroup.StartAsync() is not Status.Success)
                {
                    UIMsgBox.Show("更新资源失败", GameFrameworkStartup.Quit);
                    return Status.Fail;
                }

                UniTask<Status>[] writeHandles = new UniTask<Status>[downloadGroup.items.Length];
                for (int i = 0; i < downloadGroup.items.Length; i++)
                {
                    writeHandles[i] = GameFrameworkEntry.VFS.WriteAsync(downloadGroup.items[i].name, downloadGroup.items[i].bytes, downloadGroup.items[i].version);
                }

                Status[] result = await UniTask.WhenAll(writeHandles);
                if (result.Any(x => x is not Status.Success))
                {
                    UIMsgBox.Show("更新资源失败", GameFrameworkStartup.Quit);
                    return Status.Fail;
                }

                GameFrameworkEntry.Logger.Log("资源更新完成");
                UILoading.SetTitle(GameFrameworkEntry.Language.Query("资源更新完成..."));
                UILoading.SetProgress(1);
                return Status.Success;
            }
        }

        internal static async UniTask<Status> LoadingResourcePackageListAsync(params ResourcePackageManifest[] manifests)
        {
            if (manifests is null)
            {
                throw new ArgumentNullException("manifests");
            }

            Extension.StartSample();
            UniTask<Status>[] loadAssetBundleTaskList = new UniTask<Status>[manifests.Length];
            for (int i = 0; i < manifests.Length; i++)
            {
                loadAssetBundleTaskList[i] = LoadingAssetBundleAsync(manifests[i]);
            }

            var result = await UniTask.WhenAll(loadAssetBundleTaskList);
            if (result.Where(x => x is Status.Fail).Count() > 0)
            {
                manifests.Select(x => x.name).ToList().ForEach(x => GameFrameworkEntry.Cache.Remove(x));
                return Status.Fail;
            }

            InitializedDependenciesPackage(manifests);
            Debug.Log($"资源加载完成，总耗时：{Extension.GetSampleTime()}");
            UILoading.SetTitle(GameFrameworkEntry.Language.Query("资源加载完成..."));
            UILoading.SetProgress(1);
            return Status.Success;
        }

        internal static Status LoadingResourcePackageListSync(params ResourcePackageManifest[] manifests)
        {
            if (manifests is null)
            {
                throw new ArgumentNullException("manifests");
            }

            Extension.StartSample();
            for (int i = 0; i < manifests.Length; i++)
            {
                if (LoadingAssetBundleSync(manifests[i]) is not Status.Success)
                {
                    manifests.Select(x => x.name).ToList().ForEach(x => GameFrameworkEntry.Cache.Remove(x));
                    return Status.Fail;
                }
            }

            InitializedDependenciesPackage(manifests);
            Debug.Log($"资源加载完成，总耗时：{Extension.GetSampleTime()}");
            UILoading.SetTitle(GameFrameworkEntry.Language.Query("资源加载完成..."));
            UILoading.SetProgress(1);
            return Status.Success;
        }

        internal static async UniTask<Status> LoadingAssetBundleAsync(ResourcePackageManifest manifest)
        {
            if (GameFrameworkEntry.Cache.TryGetValue(manifest.name, out ResPackage _))
            {
                Debug.Log("资源包已加载：" + manifest.name);
                return Status.Success;
            }

            byte[] bytes = await GameFrameworkEntry.VFS.ReadAsync(manifest.name);
            if (bytes == null || bytes.Length == 0)
            {
                Debug.Log("资源包不存在：" + manifest.name);
                return Status.Fail;
            }

            ResPackage package = ResPackage.Create(await AssetBundle.LoadFromMemoryAsync(bytes));
            if (package.IsSuccess() is false)
            {
                return Status.Fail;
            }

            GameFrameworkEntry.Cache.SetCacheData(package);
            Debug.Log("资源包加载完成：" + manifest.name + " length:" + bytes.Length);
            return Status.Success;
        }

        internal static Status LoadingAssetBundleSync(ResourcePackageManifest manifest)
        {
            if (GameFrameworkEntry.Cache.TryGetValue(manifest.name, out ResPackage _))
            {
                Debug.Log("资源包已加载：" + manifest.name);
                return Status.Success;
            }

            byte[] bytes = GameFrameworkEntry.VFS.Read(manifest.name);
            if (bytes == null)
            {
                return Status.Fail;
            }

            ResPackage package = ResPackage.Create(AssetBundle.LoadFromMemory(bytes));
            if (package.IsSuccess() is false)
            {
                return Status.Fail;
            }


            GameFrameworkEntry.Cache.SetCacheData(package);
            Debug.Log("资源包加载完成：" + manifest.name);
            return Status.Success;
        }

        internal static void InitializedDependenciesPackage(params ResourcePackageManifest[] manifests)
        {
            for (int i = 0; i < manifests.Length; i++)
            {
                if (GameFrameworkEntry.Cache.TryGetValue(manifests[i].name, out ResPackage target) is false)
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
                    if (GameFrameworkEntry.Cache.TryGetValue(dependency, out ResPackage packageHandle) is false)
                    {
                        continue;
                    }

                    dependencies.Add(packageHandle);
                }

                target.SetDependencies(dependencies.ToArray());
            }
        }
    }
}