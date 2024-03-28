using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Resource.Config;
using ZGame.UI;

namespace ZGame.Resource
{
    class ResPackageLoadingHelper
    {
        public static void UnloadResourcePackage(params string[] names)
        {
            for (int i = 0; i < names.Length; i++)
            {
                if (GameFrameworkEntry.CacheObject.TryGetValue(names[i], out ResPackage packageHandle) is false)
                {
                    continue;
                }

                ResObject[] objects = GameFrameworkEntry.CacheObject.All<ResObject>();
                for (int j = 0; j < objects.Length; j++)
                {
                    if (objects[j].Parent == packageHandle)
                    {
                        GameFrameworkFactory.Release(objects[i]);
                    }
                }

                GameFrameworkFactory.Release(packageHandle);
                GameFrameworkEntry.CacheObject.Remove(names[i]);
            }
        }

        public static async UniTask<Status> LoadingResourcePackageListAsync(params ResourcePackageManifest[] manifests)
        {
            if (manifests is null || manifests.Length == 0)
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
                manifests.Select(x => x.name).ToList().ForEach(x => GameFrameworkEntry.CacheObject.Remove(x));
                return Status.Fail;
            }

            InitializedDependenciesPackage(manifests);
            Debug.Log($"资源加载完成，总耗时：{Extension.GetSampleTime()}");
            UILoading.SetTitle(GameFrameworkEntry.Language.Query("资源加载完成..."));
            UILoading.SetProgress(1);
            return Status.Success;
        }

        private static async UniTask<Status> LoadingAssetBundleAsync(ResourcePackageManifest manifest)
        {
            if (GameFrameworkEntry.CacheObject.TryGetValue(manifest.name, out ResPackage _))
            {
                Debug.Log("资源包已加载：" + manifest.name);
                return Status.Success;
            }

            AssetBundle assetBundle = default;
#if UNITY_WEBGL
            string url = OSSConfig.instance.current.GetFilePath(manifest.name);
            assetBundle = (AssetBundle)await GameFrameworkEntry.Network.GetStreamingAsset(url, Path.GetExtension(manifest.name), manifest.version);
#else
            byte[] bytes = await GameFrameworkEntry.VFS.ReadAsync(manifest.name);
            if (bytes is null || bytes.Length == 0)
            {
                return Status.Fail;
            }

            assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
#endif
            GameFrameworkEntry.CacheObject.SetCacheData(new ResPackage(assetBundle));
            Debug.Log("资源包加载完成：" + manifest.name);
            return Status.Success;
        }

        private static void InitializedDependenciesPackage(params ResourcePackageManifest[] manifests)
        {
            for (int i = 0; i < manifests.Length; i++)
            {
                if (GameFrameworkEntry.CacheObject.TryGetValue(manifests[i].name, out ResPackage target) is false)
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
                    if (GameFrameworkEntry.CacheObject.TryGetValue(dependency, out ResPackage packageHandle) is false)
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