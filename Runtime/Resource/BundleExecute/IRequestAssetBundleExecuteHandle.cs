using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    internal interface IRequestAssetBundleExecuteHandle : IExecuteHandle<IRequestAssetBundleExecuteHandle>
    {
        string name { get; }
        string module { get; }
        int version { get; }
        RuntimeBundleManifest manifest { get; }
        InternalRuntimeBundleHandle bundle { get; }

        internal static IRequestAssetBundleExecuteHandle Create(RuntimeBundleManifest manifest)
        {
            InternalRequestAssetBundleExecuteHandle internalRequestAssetBundleExecuteHandle = Activator.CreateInstance<InternalRequestAssetBundleExecuteHandle>();
            internalRequestAssetBundleExecuteHandle.manifest = manifest;
            return internalRequestAssetBundleExecuteHandle;
        }

        class InternalRequestAssetBundleExecuteHandle : GameExecuteHandle<IRequestAssetBundleExecuteHandle>, IRequestAssetBundleExecuteHandle
        {
            private float count;
            private float loadCount;
            public string name { get; set; }
            public string module { get; set; }
            public int version { get; set; }
            public RuntimeBundleManifest manifest { get; set; }
            public InternalRuntimeBundleHandle bundle { get; set; }

            class LoadBundleData
            {
                public Status status;
                public RuntimeBundleManifest manifest;
                public AssetBundle assetBundle;
            }


            public void Dispose()
            {
                version = 0;
                bundle = null;
                module = String.Empty;
                name = String.Empty;
                count = 0;
                loadCount = 0;
                manifest = null;
            }

            protected override IEnumerator DOExecute()
            {
                if (manifest is null)
                {
                    status = Status.Failed;
                    yield break;
                }

                module = manifest.owner;
                name = manifest.name;
                version = manifest.version;
                if (manifest.unityVersion.Equals(Application.unityVersion) is false)
                {
                    Engine.Console.Error($"{manifest.name}引擎版本不一致 source:{manifest.unityVersion} current:{Application.unityVersion}");
                    status = Status.Failed;
                    yield break;
                }

                IReadFileExecuteHandle readFileExecuteHandle = Engine.FileSystem.ReadFileAsync(name, version);
                yield return WaitFor.Create(() => readFileExecuteHandle.status == Status.Success || readFileExecuteHandle.status == Status.Failed);
                Engine.Console.Log("Load Asset Bundle:", name, readFileExecuteHandle.version, readFileExecuteHandle.bytes?.Length);
                AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(readFileExecuteHandle.bytes);
                yield return createRequest;
                if (createRequest.isDone is false || createRequest.assetBundle is null)
                {
                    status = Status.Failed;
                    yield break;
                }

                bundle = InternalRuntimeBundleHandle.Create(manifest, createRequest.assetBundle);
                ResourceManager.instance.AddAssetBundleHandle(bundle);
                status = Status.Success;
            }
        }
    }
}