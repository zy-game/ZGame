using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UI;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.Config;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Networking;
using ZGame.Resource.Config;
using ZGame.Window;

namespace ZGame.Resource
{
    class DefaultResourcePackageUpdateHandle : IResourcePackageUpdateHandle
    {
        public void Dispose()
        {
        }

        public async UniTask UpdateResourcePackageList(EntryConfig config)
        {
            UILoading.SetTitle(Localliztion.Get(100000));
            UILoading.SetProgress(0);
            HashSet<ResourcePackageManifest> downloadList = new HashSet<ResourcePackageManifest>();
            HashSet<string> failure = new HashSet<string>();
            List<ResourcePackageManifest> result = PackageManifestManager.instance.CheckNeedUpdatePackageList(config.module);
            foreach (var packageManifest in result)
            {
                if (downloadList.Contains(packageManifest))
                {
                    continue;
                }

                string url = OSSConfig.instance.GetFilePath(config.ossTitle, packageManifest.name);
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    request.timeout = 5;
                    request.useHttpContinue = true;
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

            if (failure.Count == 0)
            {
                return;
            }

            Debug.LogError($"Download failure:{string.Join(",", failure.ToArray())}");
            UIMsgBox.Show("更新资源失败", GameManager.instance.QuitGame);
        }
    }
}