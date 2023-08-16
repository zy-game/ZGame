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
        VersionOptions version { get; }
        InternalRuntimeBundleHandle bundle { get; }
    }

    class DefaultRequestAssetBundleExecuteHandle : ExecuteHandle, IExecuteHandle<DefaultRequestAssetBundleExecuteHandle>, IRequestAssetBundleExecuteHandle
    {
        private float count;
        private float loadCount;
        private RuntimeBundleManifest manifest;


        public string name { get; set; }
        public string module { get; set; }
        public VersionOptions version { get; set; }
        public InternalRuntimeBundleHandle bundle { get; set; }

        class LoadBundleData
        {
            public Status status;
            public RuntimeBundleManifest manifest;
            public AssetBundle assetBundle;
        }


        public override void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            manifest = (RuntimeBundleManifest)paramsList[0];
            module = manifest.owner;
            name = manifest.name;
            version = manifest.version;
            this.StartCoroutine(OnStart(name, version));
        }

        IEnumerator OnStart(string name, VersionOptions ver)
        {
            status = Status.Execute;
            IReadFileExecuteHandle readFileExecuteHandle = Engine.FileSystem.ReadFileAsync(name, ver);

            yield return WaitFor.Create(() => readFileExecuteHandle.status == Status.Success || readFileExecuteHandle.status == Status.Failed);
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
            OnComplete();
        }
    }
}