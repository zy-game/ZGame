using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    public interface IRequestResourceBundleResult : IDisposable
    {
        string name { get; }
        string module { get; }
        int version { get; }

        Status status { get; }
        // ResourceBundleManifest manifest { get; }

        internal static UniTask<IRequestResourceBundleResult> Create(ResourceBundleManifest manifest)
        {
            UniTaskCompletionSource<IRequestResourceBundleResult> uniTaskCompletionSource = new UniTaskCompletionSource<IRequestResourceBundleResult>();
            InternalRequestAssetBundleResult internalRequestAssetBundleResult = Activator.CreateInstance<InternalRequestAssetBundleResult>();
            internalRequestAssetBundleResult.manifest = manifest;
            internalRequestAssetBundleResult.Execute(uniTaskCompletionSource);
            return uniTaskCompletionSource.Task;
        }

        class InternalRequestAssetBundleResult : IRequestResourceBundleResult
        {
            private float count;
            private float loadCount;
            public string name { get; set; }
            public int version { get; set; }
            public Status status { get; set; }
            public string module { get; set; }
            public ResourceBundleManifest manifest { get; set; }

            public async void Execute(UniTaskCompletionSource<IRequestResourceBundleResult> uniTaskCompletionSource)
            {
                if (status is not Status.None)
                {
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                if (manifest is null)
                {
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                module = manifest.owner;
                name = manifest.name;
                version = manifest.version;
                if (manifest.unityVersion.Equals(Application.unityVersion) is false)
                {
                    ZGame.Console.Error($"{manifest.name}引擎版本不一致 source:{manifest.unityVersion} current:{Application.unityVersion}");
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                IReadFileResult readFileScheduleHandle = await ZGame.FileSystem.ReadFileAsync(name, version);
                AssetBundle assetBundle = await AssetBundle.LoadFromMemoryAsync(readFileScheduleHandle.bytes);
                if (assetBundle is null)
                {
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                ZGame.Console.Log("Load Asset Bundle:", name, readFileScheduleHandle.version, readFileScheduleHandle.bytes?.Length);
                ZGame.Datable.Add(assetBundle.name, RuntimeAssetBundleHandle.Create(manifest, assetBundle));
                status = Status.Success;
                uniTaskCompletionSource.TrySetResult(this);
            }


            public void Dispose()
            {
                version = 0;
                module = String.Empty;
                name = String.Empty;
                count = 0;
                loadCount = 0;
                manifest = null;
                status = Status.None;
            }
        }
    }
}