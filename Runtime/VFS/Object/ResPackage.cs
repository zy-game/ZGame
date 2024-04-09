﻿using System;
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
    partial class ResPackage : IReferenceObject
    {
        public string name { get; private set; }
        public int refCount { get; private set; }
        public AssetBundle bundle { get; private set; }
        public ResPackage[] dependencies { get; private set; }

        private bool isDefault;
        private float nextCheckTime;


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
    }

    partial class ResPackage
    {
        /// <summary>
        /// 引用对象数据
        /// </summary>
        private static List<ResPackage> packages = new();

        /// <summary>
        /// 缓存池
        /// </summary>
        private static List<ResPackage> packageCache = new();

        /// <summary>
        /// 默认资源包
        /// </summary>
        public static ResPackage DEFAULT = ResPackage.Create("DEFAULT");

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
#if UNITY_EDITOR
        internal static void OnDrawingGUI()
        {
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("资源包", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < packages.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(packages[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(packages[i].refCount.ToString());
                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
            GUILayout.BeginVertical(UnityEditor.EditorStyles.helpBox);
            UnityEditor.EditorGUILayout.LabelField("资源包缓存池", UnityEditor.EditorStyles.boldLabel);
            for (int i = 0; i < packages.Count; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(packages[i].name);
                GUILayout.FlexibleSpace();
                GUILayout.Label(packages[i].refCount.ToString());
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
            for (int i = packages.Count - 1; i >= 0; i--)
            {
                if (packages[i].refCount > 0)
                {
                    continue;
                }

                packageCache.Add(packages[i]);
                packages.RemoveAt(i);
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

                GameFrameworkFactory.Release(packageCache[i]);
            }

            packageCache.Clear();
        }

        internal static bool TryGetValue(string name, out ResPackage package)
        {
            package = packages.Find(x => x.name == name);
            if (package is null)
            {
                package = packageCache.Find(x => x.name == name);
                if (package is not null)
                {
                    packageCache.Remove(package);
                    packages.Add(package);
                }
            }

            return package is not null;
        }

        internal static void UnloadResourcePackage(string name)
        {
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
                manifests.Select(x => x.name).ToList().ForEach(x => UnloadResourcePackage(x));
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
                    manifests.Select(x => x.name).ToList().ForEach(x => UnloadResourcePackage(x));
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
            if (TryGetValue(manifest.name, out ResPackage _))
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

            GameFrameworkEntry.Logger.Log("加载资源包：" + manifest.name);
            ResPackage package = ResPackage.Create(await AssetBundle.LoadFromMemoryAsync(bytes));
            if (package.IsSuccess() is false)
            {
                return Status.Fail;
            }

            packages.Add(package);
            Debug.Log("资源包加载完成：" + manifest.name + " length:" + bytes.Length);
            return Status.Success;
        }

        internal static Status LoadingAssetBundleSync(ResourcePackageManifest manifest)
        {
            if (TryGetValue(manifest.name, out ResPackage _))
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


            packages.Add(package);
            Debug.Log("资源包加载完成：" + manifest.name);
            return Status.Success;
        }

        internal static void InitializedDependenciesPackage(params ResourcePackageManifest[] manifests)
        {
            for (int i = 0; i < manifests.Length; i++)
            {
                if (TryGetValue(manifests[i].name, out ResPackage target) is false)
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
                    if (TryGetValue(dependency, out ResPackage packageHandle) is false)
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