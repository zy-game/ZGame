using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;
using ZGame.FileSystem;
using ZGame.Window;

namespace ZGame.Resource
{
    class DefaultResourcePackageLoadingHandle : IResourcePackageLoadingHandle
    {
        public void Dispose()
        {
        }

        public async UniTask Loading(params string[] paths)
        {
            Queue<string> fileList = new Queue<string>();
            ILoading handler = (ILoading)UIManager.instance.Open(typeof(ILoading));
            //todo 这里还需要注意实在webgl平台上面加载资源包的情况
            handler.SetTitle("正在加载资源信息...");
            handler.Report(0);
            foreach (var VARIABLE in paths)
            {
                if (VARIABLE.IsNullOrEmpty())
                {
                    continue;
                }

                if (VARIABLE.StartsWith("http"))
                {
                    fileList.Enqueue(Path.GetFileName(VARIABLE));
                    continue;
                }

                List<ResourcePackageManifest> manifests = await ResourcePackageListManifest.GetPackageList(VARIABLE);
                foreach (var VARIABLE2 in manifests)
                {
                    fileList.Enqueue(VARIABLE2.name);
                }
            }

            await LoadBundleList(fileList, handler);
        }

        private async UniTask LoadBundleList(Queue<string> fileList, ILoading loadingHandle)
        {
            int count = fileList.Count;
            for (int i = 0; i < count; i++)
            {
                string VARIABLE = fileList.Dequeue();
                loadingHandle.SetTitle($"正在加载资源包: {VARIABLE}");
                loadingHandle.Report(i / (float)count);
                if (ResourceManager.instance.GetResourcePackageHandle(VARIABLE) != null)
                {
                    continue;
                }

                byte[] bytes = await VFSManager.instance.ReadAsync(VARIABLE);
                if (bytes is null || bytes.Length == 0)
                {
                    Clear(fileList);
                    return;
                }

                loadingHandle.SetTitle($"正在加载资源包: {VARIABLE}");
                loadingHandle.Report((i + 1) / (float)count);
                AssetBundle assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                ResourceManager.instance.AddResourcePackageHandle(new ResPackageHandle(assetBundle, false));
            }

            loadingHandle.SetTitle("资源加载完成...");
            loadingHandle.Report(1);
        }

        private void Clear(Queue<string> fileList)
        {
            while (fileList.Count > 0)
            {
                string VARIABLE = fileList.Dequeue();
                ResourceManager.instance.RemoveResourcePackageHandle(VARIABLE);
            }
        }
    }
}