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
        private List<DownloadOptions> options;

        private void Clear()
        {
            options.ForEach(x => x.Dispose());
            options.Clear();
        }

        private void EqualsLocalFile(string fileName, uint version)
        {
            if (Engine.File.EqualsVersion(fileName, version) is false)
            {
                options.Add(new DownloadOptions()
                {
                    name = fileName,
                    version = version,
                    url = Engine.Resource.GetNetworkResourceUrl(fileName)
                });
            }
        }

        public async UniTask StartUpdate(BuilderManifest manifest, Action<float> progressCallback)
        {
            options = new List<DownloadOptions>();
            foreach (var VARIABLE in manifest.packages)
            {
                EqualsLocalFile(VARIABLE.name, VARIABLE.version);
                if (VARIABLE.dependencies is null || VARIABLE.dependencies.Length == 0)
                {
                    continue;
                }

                foreach (var VARIABLE2 in VARIABLE.dependencies)
                {
                    EqualsLocalFile(VARIABLE2.name, VARIABLE2.version);
                }
            }

            if (options.Count == 0)
            {
                Clear();
                return;
            }

            DownloadHandle[] downloadHandles = await Engine.Network.Download(progressCallback, options.ToArray());
            if (downloadHandles.Where(x => x.error != null).Count() > 0)
            {
                throw new HttpRequestException(downloadHandles.Where(x => x.error != null).FirstOrDefault()?.error);
            }

            Clear();
        }

        public async UniTask StartUpdate(Action<float> progressCallback, params string[] args)
        {
            options = new List<DownloadOptions>();
            foreach (var VARIABLE in args)
            {
                options.Add(new DownloadOptions()
                {
                    name = VARIABLE,
                    version = 0,
                    url = Engine.Resource.GetNetworkResourceUrl(VARIABLE)
                });
            }

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

        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }
    }
}