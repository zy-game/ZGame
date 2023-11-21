using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.FileSystem;
using ZGame.Networking;

namespace ZGame.Resource
{
    public sealed class CheckResourceUpdatePipeline : IPackageUpdatePipeline
    {
        public async UniTask StartUpdate(Action<float> progressCallback, params string[] args)
        {
            if (args is null || args.Length == 0)
            {
                return;
            }

            List<string> options = await CheckNeedUpdateList(args);

            if (options.Count == 0)
            {
                return;
            }

            NetworkDownloadPipelineHandle[] downloadHandles = await NetworkManager.instance.Download(progressCallback, options.ToArray());
            bool state = true;
            foreach (var VARIABLE in downloadHandles)
            {
                if (VARIABLE.isDone is false)
                {
                    state = false;
                    continue;
                }

                await FileManager.instance.WriteAsync(VARIABLE.name, VARIABLE.bytes, ResourcePackageListManifest.GetResourcePackageVersion(VARIABLE.name));
            }

            if (state is false)
            {
                throw new HttpRequestException();
            }
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

                string etag = await NetworkManager.instance.Head(VARIABLE, "eTag");
                if (etag.IsNullOrEmpty())
                {
                    Debug.LogError($"Checkout {VARIABLE} etag is null");
                    continue;
                }

                uint crc = Crc32.GetCRC32Str(etag);
                if (FileManager.instance.Exist(Path.GetFileName(VARIABLE), crc))
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