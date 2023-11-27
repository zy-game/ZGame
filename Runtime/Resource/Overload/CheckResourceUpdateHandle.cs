using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.FileSystem;
using ZGame.Networking;
using ZGame.Window;

namespace ZGame.Resource
{
    public sealed class CheckResourceUpdateHandle : IDisposable
    {
        public async UniTask StartUpdate(Action<float> progressCallback, params string[] args)
        {
            if (args is null || args.Length == 0)
            {
                return;
            }

            UIManager.instance.GetWindow<Loading>().SetupTitle("正在检查资源更新...").SetupProgress(0);

            List<string> options = await CheckNeedUpdateList(args);

            if (options.Count == 0)
            {
                return;
            }

            MultipDownloadHandle.DownloadData[] downloadDatas = await MultipDownloadHandle.Download(progressCallback, options.ToArray());
            for (int i = 0; i < downloadDatas.Length; i++)
            {
                if (downloadDatas[i].isDone is false)
                {
                    continue;
                }

                uint crc = ResourcePackageListManifest.GetResourcePackageVersion(downloadDatas[i].name);
                if (crc == 0)
                {
                    crc = downloadDatas[i].crc;
                }

                await VFSManager.instance.WriteAsync(downloadDatas[i].name, downloadDatas[i].bytes, crc);
            }

            if (downloadDatas.Where(x => x.isDone == false).Count() == 0)
            {
                return;
            }

            MsgBox.Create("资源更新失败！", Extension.QuitGame);
        }

        private async UniTask<List<string>> CheckNeedUpdateList(string[] args)
        {
            List<string> options = new List<string>();
            foreach (var VARIABLE in args)
            {
                if (VARIABLE.StartsWith("http") is false)
                {
                    List<ResourcePackageManifest> result = await ResourcePackageListManifest.CheckNeedUpdatePackageList(VARIABLE);
                    foreach (var op in result)
                    {
                        if (options.Contains(GameSeting.GetNetworkResourceUrl(op.name)))
                        {
                            continue;
                        }

                        options.Add(GameSeting.GetNetworkResourceUrl(op.name));
                    }

                    continue;
                }

                string etag = await NetworkRequest.Head(VARIABLE, "eTag");
                if (etag.IsNullOrEmpty())
                {
                    Debug.LogError($"Checkout {VARIABLE} etag is null");
                    continue;
                }

                uint crc = Crc32.GetCRC32Str(etag);
                if (VFSManager.instance.Exist(Path.GetFileName(VARIABLE), crc))
                {
                    continue;
                }

                options.Add(VARIABLE);
            }

            return options;
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}