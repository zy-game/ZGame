using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.FileSystem;
using ZGame.Resource.Config;
using ZGame.Window;

namespace ZGame.Resource
{
    class DefaultResourcePackageLoadingHandle : IResourcePackageLoadingHandle
    {
        public void Dispose()
        {
        }

        public void LoadingPackageListSync(params ResourcePackageManifest[] manifests)
        {
            if (manifests is null || manifests.Length == 0)
            {
                Debug.Log("没有需要加载的资源包");
                return;
            }

            for (int i = 0; i < manifests.Length; i++)
            {
                if (PackageHandleCache.instance.TryGetValue(manifests[i].name, out _))
                {
                    Debug.Log("已经加载了资源包：" + manifests[i].name);
                    continue;
                }

                AssetBundle assetBundle = default;
                byte[] bytes = VFSManager.instance.Read(manifests[i].name);
                if (bytes is null || bytes.Length == 0)
                {
                    Clear(manifests);
                    Debug.Log("读取资源包失败：" + manifests[i].name);
                    return;
                }

                assetBundle = AssetBundle.LoadFromMemory(bytes);
                PackageHandleCache.instance.Add(new PackageHandle(assetBundle));
                Debug.Log("资源包加载完成：" + manifests[i].name);
            }

            for (int i = 0; i < manifests.Length; i++)
            {
                if (PackageHandleCache.instance.TryGetValue(manifests[i].name, out var target) is false)
                {
                    continue;
                }

                if (manifests[i].dependencies is null || manifests[i].dependencies.Length == 0)
                {
                    continue;
                }

                List<PackageHandle> dependencies = new List<PackageHandle>();
                foreach (var dependency in manifests[i].dependencies)
                {
                    if (PackageHandleCache.instance.TryGetValue(dependency, out var packageHandle) is false)
                    {
                        continue;
                    }

                    dependencies.Add(packageHandle);
                }

                target.SetDependencies(dependencies.ToArray());
            }
        }

        public async UniTask LoadingPackageListAsync(params ResourcePackageManifest[] manifests)
        {
            for (int i = 0; i < manifests.Length; i++)
            {
                UILoading.SetProgress(i / (float)manifests.Length);
                if (PackageHandleCache.instance.TryGetValue(manifests[i].name, out _))
                {
                    Debug.Log("资源包已加载：" + manifests[i].name);
                    continue;
                }

                AssetBundle assetBundle = default;
                byte[] bytes = await VFSManager.instance.ReadAsync(manifests[i].name);
                if (bytes is null || bytes.Length == 0)
                {
                    Clear(manifests);
                    UILoading.SetTitle("资源加载失败...");
                    return;
                }

                assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                PackageHandleCache.instance.Add(new PackageHandle(assetBundle));
                Debug.Log("资源包加载完成：" + manifests[i].name);
            }

            for (int i = 0; i < manifests.Length; i++)
            {
                if (PackageHandleCache.instance.TryGetValue(manifests[i].name, out var target) is false)
                {
                    continue;
                }

                if (manifests[i].dependencies is null || manifests[i].dependencies.Length == 0)
                {
                    continue;
                }

                List<PackageHandle> dependencies = new List<PackageHandle>();
                foreach (var dependency in manifests[i].dependencies)
                {
                    if (PackageHandleCache.instance.TryGetValue(dependency, out var packageHandle) is false)
                    {
                        continue;
                    }

                    //
                    dependencies.Add(packageHandle);
                }

                target.SetDependencies(dependencies.ToArray());
            }

            UILoading.SetTitle("资源加载完成...");
            UILoading.SetProgress(1);
        }


        private void Clear(params ResourcePackageManifest[] fileList)
        {
            foreach (var VARIABLE in fileList)
            {
                PackageHandleCache.instance.Remove(VARIABLE.name);
            }
        }
    }
}