using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.FileSystem;
using ZGame.Networking;
using ZGame.Window;

namespace ZGame.Resource
{
    public sealed class AssetBundleLoadingHandle : IDisposable
    {
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        public async UniTask LoadingPackageList(Action<float> progressCallback, params string[] args)
        {
            Queue<string> fileList = new Queue<string>();
            //todo 这里还需要注意实在webgl平台上面加载资源包的情况
            UIManager.instance.GetWindow<Loading>().SetupTitle("正在加载资源信息...").SetupProgress(0);
            foreach (var VARIABLE in args)
            {
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

            LoadBundleList(fileList, progressCallback);
        }

        private async UniTask LoadBundleList(Queue<string> fileList, Action<float> progressCallback)
        {
            int count = fileList.Count;
            for (int i = 0; i < count; i++)
            {
                string VARIABLE = fileList.Dequeue();
                UIManager.instance.GetWindow<Loading>().SetupTitle($"正在加载资源包: {VARIABLE}").SetupProgress(i / (float)count);
                if (ABManager.instance.GetBundleHandle(VARIABLE) != null)
                {
                    continue;
                }

                byte[] bytes = await VFSManager.instance.ReadAsync(VARIABLE);
                if (bytes is null || bytes.Length == 0)
                {
                    Clear(fileList);
                    return;
                }

                AssetBundle assetBundle = await AssetBundle.LoadFromMemoryAsync(bytes);
                ABManager.instance.Add(assetBundle);
            }
        }

        private void Clear(Queue<string> fileList)
        {
            while (fileList.Count > 0)
            {
                string VARIABLE = fileList.Dequeue();
                ABManager.instance.Remove(VARIABLE);
            }
        }
    }
}