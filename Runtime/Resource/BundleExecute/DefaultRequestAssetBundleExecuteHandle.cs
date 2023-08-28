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

    class DefaultRequestAssetBundleExecuteHandle : AbstractExecuteHandle, IExecuteHandle<DefaultRequestAssetBundleExecuteHandle>, IRequestAssetBundleExecuteHandle
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


        public void Release()
        {
            version = null;
            bundle = null;
            module = String.Empty;
            name = String.Empty;
            count = 0;
            loadCount = 0;
            manifest = null;
        }

        protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
        {
            manifest = (RuntimeBundleManifest)paramsList[0];
            module = manifest.owner;
            name = manifest.name;
            version = manifest.version;
            IReadFileExecuteHandle readFileExecuteHandle = Engine.FileSystem.ReadFileAsync(name, version);

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
        }
    }
}