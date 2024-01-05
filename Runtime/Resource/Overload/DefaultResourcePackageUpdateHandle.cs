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
            UILoading handler = (UILoading)UIManager.instance.Open(typeof(UILoading));
            handler.SetTitle(Localliztion.Get(100000));
            handler.Report(0);
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
                    handler.SetTitle(Path.GetFileName(url));
                    await request.SendWebRequest().ToUniTask(handler);
                    handler.Report(1);
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