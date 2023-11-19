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
    public sealed class DefaultPackageUpdatePipeline : IPackageUpdatePipeline
    {
        private void Clear()
        {
        }

        public async UniTask StartUpdate(Action<float> progressCallback, params string[] args)
        {
            if (args is null || args.Length == 0)
            {
                Clear();
                return;
            }

            List<DownloadOptions> options = await Checkout(args);

            if (options.Count == 0)
            {
                Clear();
                return;
            }

            DownloadHandle[] downloadHandles = await Engine.Network.Download(progressCallback, options.ToArray());
            if (downloadHandles.Where(x => x.error != null).Count() > 0)
            {
                Clear();
                throw new HttpRequestException(downloadHandles.Where(x => x.error != null).FirstOrDefault()?.error);
            }

            Clear();
        }

        private async UniTask<List<DownloadOptions>> Checkout(string[] args)
        {
            List<DownloadOptions> options = new List<DownloadOptions>();
            foreach (var VARIABLE in args)
            {
                if (VARIABLE.StartsWith("http") is false)
                {
                    PackageListManifest packageListManifest = await PackageListManifest.Find(VARIABLE);
                    if (packageListManifest == null)
                    {
                        continue;
                    }

                    List<DownloadOptions> result = CheckPackageListManifest(packageListManifest);
                    foreach (var op in result)
                    {
                        if (options.Find(x => x.name.Equals(op.name)) is null)
                        {
                            options.Add(op);
                        }
                    }

                    continue;
                }

                string etag = await Engine.Network.Head(VARIABLE, "eTag");
                if (etag.IsNullOrEmpty())
                {
                    continue;
                }

                uint crc = Crc32.GetCRC32Str(etag);
                if (Engine.File.Exist(Path.GetFileName(VARIABLE), crc))
                {
                    continue;
                }

                options.Add(new DownloadOptions()
                {
                    name = Path.GetFileName(VARIABLE),
                    version = crc,
                    url = VARIABLE
                });
            }

            return options;
        }


        private List<DownloadOptions> CheckPackageListManifest(PackageListManifest packageListManifest)
        {
            return default;
        }

        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }
    }
}