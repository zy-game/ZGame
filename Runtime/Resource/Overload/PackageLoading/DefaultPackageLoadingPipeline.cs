using System;
using System.Collections.Generic;
using System.IO;
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

        public async UniTask LoadingPackageList(Action<float> progressCallback, params string[] args)
        {
            index = 0;
            count = (float)(args.Length);
            loaded = new HashSet<string>();
            foreach (var VARIABLE in args)
            {
                await LoadAssetBundleFromFile(VARIABLE, 0, progressCallback);
            }

            Clear();
        }

        public async UniTask LoadingModulePackageList(ResourceModuleManifest manifest, Action<float> progressCallback)
        {
            index = 0;
            loaded = new HashSet<string>();
            count = (float)(manifest.packages?.Length + manifest.packages.Sum(x => x.dependencies?.Length));
            progressCallback?.Invoke(0);
            foreach (var VARIABLE in manifest.packages)
            {
                if (VARIABLE.dependencies is not null && VARIABLE.dependencies.Length > 0)
                {
                    foreach (var dependencie in VARIABLE.dependencies)
                    {
                        await LoadAssetBundleFromFile(dependencie.name, dependencie.version, progressCallback);
                    }
                }

                await LoadAssetBundleFromFile(VARIABLE.name, VARIABLE.version, progressCallback);
            }

            Clear();
            progressCallback?.Invoke(1);
        }

        private async UniTask LoadAssetBundleFromFile(string fileName, int version, Action<float> progressCallback)
        {
            if (loaded.Contains(fileName))
            {
                return;
            }

            if (Engine.File.EqualsVersion(fileName, version) is false)
            {
                throw new FileNotFoundException(fileName);
            }

            IReadFileResult readFileResult = await Engine.File.ReadAsync(fileName);
            if (readFileResult.bytes is null || readFileResult.bytes.Length == 0)
            {
                throw new FileLoadException(readFileResult.name);
            }

            AssetBundle assetBundle = await AssetBundle.LoadFromMemoryAsync(readFileResult.bytes);
            Engine.Resource.AddAssetBundleHandle(assetBundle);
            readFileResult.Dispose();
            index++;
            progressCallback?.Invoke(index / count);
        }

        private void Clear()
        {
            count = 0;
            index = 0;
            foreach (var VARIABLE in loaded)
            {
                Engine.Resource.RemoveAssetBundleHandle(VARIABLE);
            }

            loaded.Clear();
        }
    }
}