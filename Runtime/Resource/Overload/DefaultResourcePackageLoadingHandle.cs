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

        public async UniTask LoadingResourcePackageList(EntryConfig config)
        {
            UILoading handler = (UILoading)UIManager.instance.Open(typeof(UILoading));
            //todo 这里还需要注意实在webgl平台上面加载资源包的情况
            handler.SetTitle("正在加载资源信息...");
            handler.Report(0);
            List<ResourcePackageManifest> manifests = PackageManifestManager.instance.GetResourcePackageAndDependencyList(config.module);
            int index = 0;
            while (index < manifests.Count)
            {
                handler.Report(index / (float)manifests.Count);
                if (ResourceManager.instance.GetResourcePackageHandle(manifests[index].name) == null)
                {
                    AssetBundle assetBundle = default;
                    byte[] bytes = await VFSManager.instance.ReadAsync(manifests[index].name);
                    if (bytes is null || bytes.Length == 0)
                    {
                        Clear(manifests);
                        handler.SetTitle("资源加载失败...");
                        return;
                    }

                    assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                    ResourceManager.instance.AddResourcePackageHandle(new ResPackageHandle(assetBundle, false));
                    index++;
                }
            }

            handler.SetTitle("资源加载完成...");
            handler.Report(1);
        }


        private void Clear(List<ResourcePackageManifest> fileList)
        {
            foreach (var VARIABLE in fileList)
            {
                ResourceManager.instance.RemoveResourcePackageHandle(VARIABLE.name);
            }
        }
    }
}