using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Networking;

namespace ZGame.Resource
{
    public interface IPackageLoadingPipeline : IDisposable
    {
        UniTask<ErrorCode> LoadingPackageList(Action<float> progressCallback, params string[] args);
        UniTask<ErrorCode> LoadingModulePackageList(Action<float> progressCallback, ResourceModuleManifest manifest);
    }

    public sealed class DefaultPackageLoadingPipeline : IPackageLoadingPipeline
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public UniTask<ErrorCode> LoadingPackageList(Action<float> progressCallback, params string[] args)
        {
            throw new NotImplementedException();
        }

        public async UniTask<ErrorCode> LoadingModulePackageList(Action<float> progressCallback, ResourceModuleManifest manifest)
        {
            List<ResourcePackageManifest> needUpdateList = new List<ResourcePackageManifest>();
            foreach (var VARIABLE in manifest.packages)
            {
                if (CoreApi.File.EnsureFileVersion(VARIABLE.name, VARIABLE.version) is false)
                {
                    needUpdateList.Add(VARIABLE);
                }
            }

            if (needUpdateList.Count == 0)
            {
                return ErrorCode.OK;
            }

            DownloadHandle[] downloadHandles = await CoreApi.Network.Download(progressCallback, needUpdateList.Select(x => new DownloadOptions()
            {
                name = x.name,
                url = CoreApi.GetNetworkResourceUrl(x.name),
                version = x.version
            }).ToArray());

            if (downloadHandles.Where(x => x.error != null).Count() > 0)
            {
                Debug.LogError("下载资源时出现错误");
                return ErrorCode.DOWNLOAD_FAIL;
            }

            return ErrorCode.OK;
        }
    }
}