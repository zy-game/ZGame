using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.Config;
using ZGame.FileSystem;
using ZGame.Game;
using ZGame.Networking;
using ZGame.Window;

namespace ZGame.Resource
{
    class DefaultResourcePackageUpdateHandle : IResourcePackageUpdateHandle
    {
        public void Dispose()
        {
        }

        public async UniTask Update(ILoadingHandle loadingHandle, params string[] paths)
        {
            if (paths is null || paths.Length == 0)
            {
                return;
            }

            loadingHandle.SetTitle(Language.instance.GetLanguage(100000));
            loadingHandle.Report(0);
            HashSet<ResourcePackageManifest> downloadList = new HashSet<ResourcePackageManifest>();
            HashSet<string> urlList = new HashSet<string>();
            HashSet<string> failure = new HashSet<string>();
            foreach (var VARIABLE in paths)
            {
                if (VARIABLE.IsNullOrEmpty())
                {
                    continue;
                }

                if (VARIABLE.StartsWith("http"))
                {
                    if (urlList.Contains(VARIABLE))
                    {
                        continue;
                    }

                    string tag = await NetworkRequest.Head(VARIABLE, "eTag");
                    uint crc = Crc32.GetCRC32Str(tag);
                    if (VFSManager.instance.Exist(Path.GetFileName(VARIABLE), crc))
                    {
                        continue;
                    }

                    bool state = await DownloadResource(VARIABLE, loadingHandle, crc);
                    urlList.Add(VARIABLE);
                    if (state is false)
                    {
                        failure.Add(VARIABLE);
                    }

                    continue;
                }


                List<ResourcePackageManifest> result = await ResourcePackageListManifest.CheckNeedUpdatePackageList(VARIABLE);
                foreach (var packageManifest in result)
                {
                    if (downloadList.Contains(packageManifest))
                    {
                        continue;
                    }

                    bool state = await DownloadResource(packageManifest, loadingHandle);
                    downloadList.Add(packageManifest);
                    if (state is false)
                    {
                        failure.Add(packageManifest.name);
                    }
                }
            }

            if (failure.Count == 0)
            {
                return;
            }

            Debug.LogError($"Download failure:{string.Join(",", failure.ToArray())}");
            MsgBox.Create(Language.instance.GetLanguage(InternalLanguage.UPDATE_RESOURCE_ERROR), GameManager.instance.QuitGame);
        }

        private async UniTask<bool> DownloadResource(ResourcePackageManifest resourcePackageManifest, ILoadingHandle loadingHandle)
        {
            return await DownloadResource(GlobalConfig.GetNetworkResourceUrl(resourcePackageManifest.name), loadingHandle, resourcePackageManifest.version);
        }

        private async UniTask<bool> DownloadResource(string url, ILoadingHandle loadingHandle, uint crc = 0)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 5;
            request.useHttpContinue = true;
            loadingHandle.SetTitle(Path.GetFileName(url));
            await request.SendWebRequest().ToUniTask(loadingHandle);
            loadingHandle.Report(1);
            bool success = request.result is UnityWebRequest.Result.Success;
            if (success)
            {
                if (crc == 0)
                {
                    crc = Crc32.GetCRC32Str(request.GetRequestHeader("eTag"));
                }

                await VFSManager.instance.WriteAsync(Path.GetFileName(url), request.downloadHandler.data, crc);
            }

            request.Dispose();
            return success;
        }
    }
}