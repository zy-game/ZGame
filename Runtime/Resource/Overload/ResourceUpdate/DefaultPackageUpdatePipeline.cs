using System;
using System.Collections.Generic;
using System.Linq;
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

        private void EqualsLocalFile(string fileName, int version)
        {
            if (CoreApi.File.EqualsVersion(fileName, version) is false)
            {
                options.Add(new DownloadOptions()
                {
                    name = fileName,
                    version = version,
                    url = CoreApi.GetNetworkResourceUrl(fileName)
                });
            }
        }

        public async UniTask<ErrorCode> StartUpdate(ResourceModuleManifest manifest, Action<float> progressCallback)
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
                return ErrorCode.OK;
            }

            DownloadHandle[] downloadHandles = await CoreApi.Network.Download(progressCallback, options.ToArray());
            if (downloadHandles.Where(x => x.error != null).Count() > 0)
            {
                Debug.LogError("下载资源时出现错误");
                Clear();
                return ErrorCode.DOWNLOAD_FAIL;
            }

            Clear();
            return ErrorCode.OK;
        }

        public async UniTask<ErrorCode> StartUpdate(Action<float> progressCallback, params string[] args)
        {
            options = new List<DownloadOptions>();
            foreach (var VARIABLE in args)
            {
                options.Add(new DownloadOptions()
                {
                    name = VARIABLE,
                    version = 0,
                    url = CoreApi.GetNetworkResourceUrl(VARIABLE)
                });
            }

            if (options.Count == 0)
            {
                Clear();
                return ErrorCode.OK;
            }

            DownloadHandle[] downloadHandles = await CoreApi.Network.Download(progressCallback, options.ToArray());
            if (downloadHandles.Where(x => x.error != null).Count() > 0)
            {
                Debug.LogError("下载资源时出现错误");
                Clear();
                return ErrorCode.DOWNLOAD_FAIL;
            }

            Clear();
            return ErrorCode.OK;
        }

        public void Dispose()
        {
            Clear();
            GC.SuppressFinalize(this);
        }
    }
}