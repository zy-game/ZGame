using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.FileSystem;
using ZGame.Networking;

namespace ZGame.Resource
{
    public sealed class DefaultPackageLoadingPipeline : IPackageLoadingPipeline
    {
        private float count;
        private float index;
        private HashSet<string> loaded;

        public void Dispose()
        {
            count = 0;
            index = 0;
            GC.SuppressFinalize(this);
        }

        public async UniTask<ErrorCode> LoadingPackageList(Action<float> progressCallback, params string[] args)
        {
            index = 0;
            count = (float)(args.Length);
            ErrorCode errorCode = ErrorCode.OK;
            loaded = new HashSet<string>();
            foreach (var VARIABLE in args)
            {
                errorCode = await LoadAssetBundleFromFile(VARIABLE, 0, progressCallback);
                if (errorCode is not ErrorCode.OK)
                {
                    Clear();
                    return errorCode;
                }
            }

            return ErrorCode.OK;
        }

        public async UniTask<ErrorCode> LoadingModulePackageList(ResourceModuleManifest manifest, Action<float> progressCallback)
        {
            index = 0;
            loaded = new HashSet<string>();
            count = (float)(manifest.packages?.Length + manifest.packages.Sum(x => x.dependencies?.Length));
            progressCallback?.Invoke(0);
            ErrorCode errorCode = ErrorCode.OK;
            foreach (var VARIABLE in manifest.packages)
            {
                if (VARIABLE.dependencies is not null && VARIABLE.dependencies.Length > 0)
                {
                    foreach (var dependencie in VARIABLE.dependencies)
                    {
                        errorCode = await LoadAssetBundleFromFile(dependencie.name, dependencie.version, progressCallback);
                        if (errorCode is not ErrorCode.OK)
                        {
                            Clear();
                            return errorCode;
                        }
                    }
                }

                errorCode = await LoadAssetBundleFromFile(VARIABLE.name, VARIABLE.version, progressCallback);
                if (errorCode is not ErrorCode.OK)
                {
                    Clear();
                    return errorCode;
                }
            }

            progressCallback?.Invoke(1);
            return ErrorCode.OK;
        }

        private async UniTask<ErrorCode> LoadAssetBundleFromFile(string fileName, int version, Action<float> progressCallback)
        {
            if (loaded.Contains(fileName))
            {
                return ErrorCode.OK;
            }

            if (CoreApi.File.EqualsVersion(fileName, version) is false)
            {
                return ErrorCode.NOT_FIND;
            }

            IReadFileResult readFileResult = await CoreApi.File.ReadAsync(fileName);
            if (readFileResult.bytes is null || readFileResult.bytes.Length == 0)
            {
                return ErrorCode.READ_FILE_FAIL;
            }

            AssetBundle assetBundle = await AssetBundle.LoadFromMemoryAsync(readFileResult.bytes);
            CoreApi.Resource.AddAssetBundleHandle(assetBundle);
            readFileResult.Dispose();
            index++;
            progressCallback?.Invoke(index / count);
            return ErrorCode.OK;
        }

        private void Clear()
        {
            count = 0;
            index = 0;
            foreach (var VARIABLE in loaded)
            {
                CoreApi.Resource.RemoveAssetBundleHandle(VARIABLE);
            }

            loaded.Clear();
        }
    }
}