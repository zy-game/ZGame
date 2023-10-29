using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.FileSystem;

namespace ZGame.Resource
{
    public interface IPackageUpdatePipeline : IDisposable
    {
        UniTask<ErrorCode> StartUpdate(ResourceModuleManifest packageListManifest, Action<float> progressCallback);
    }

    public sealed class DefaultPackageUpdatePipeline : IPackageUpdatePipeline
    {
        public async UniTask<ErrorCode> StartUpdate(ResourceModuleManifest packageListManifest, Action<float> progressCallback)
        {
            progressCallback?.Invoke(0);
            float index = 0;
            foreach (var VARIABLE in packageListManifest.packages)
            {
                if (CoreApi.File.EnsureFileVersion(VARIABLE.name, VARIABLE.version) is false)
                {
                    return ErrorCode.NOT_FIND;
                }

                IReadFileResult readFileResult = await CoreApi.File.ReadAsync(VARIABLE.name);
                RuntimeAssetBundleHandle runtimeAssetBundleHandle = new RuntimeAssetBundleHandle(await AssetBundle.LoadFromMemoryAsync(readFileResult.bytes));
                index++;
                progressCallback?.Invoke(index / (float)packageListManifest.packages.Length);
            }

            progressCallback?.Invoke(1);
            return ErrorCode.OK;
        }

        public void Dispose()
        {
        }
    }
}