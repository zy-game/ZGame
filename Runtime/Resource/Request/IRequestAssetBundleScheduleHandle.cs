using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    internal interface IRequestAssetBundleScheduleHandle : IScheduleHandle<IRequestAssetBundleScheduleHandle>
    {
        string name { get; }
        string module { get; }
        int version { get; }
        GameAssetBundleManifest manifest { get; }
        AssetBundleRuntimeHandle bundle { get; }

        internal static IRequestAssetBundleScheduleHandle Create(GameAssetBundleManifest manifest)
        {
            InternalRequestAssetBundleScheduleHandle internalRequestAssetBundleScheduleHandle = Activator.CreateInstance<InternalRequestAssetBundleScheduleHandle>();
            internalRequestAssetBundleScheduleHandle.manifest = manifest;
            return internalRequestAssetBundleScheduleHandle;
        }

        class InternalRequestAssetBundleScheduleHandle : IRequestAssetBundleScheduleHandle
        {
            private float count;
            private float loadCount;
            public string name { get; set; }
            public string module { get; set; }
            public int version { get; set; }
            public GameAssetBundleManifest manifest { get; set; }
            public AssetBundleRuntimeHandle bundle { get; set; }
            public Status status { get; set; }
            public IRequestAssetBundleScheduleHandle result => this;

            private ISubscriber subscriber;

            public void Execute(params object[] args)
            {
                if (status is not Status.None)
                {
                    return;
                }

                status = Status.Execute;
                DOExecute().StartCoroutine(OnComplate);
            }

            public void Subscribe(ISubscriber subscriber)
            {
                if (this.subscriber is null)
                {
                    this.subscriber = subscriber;
                    return;
                }

                this.subscriber.Merge(subscriber);
            }

            private void OnComplate()
            {
                if (subscriber is not null)
                {
                    subscriber.Execute(this);
                }

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
                status = Status.None;
            }

            private IEnumerator DOExecute()
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

                IReadFileScheduleHandle readFileScheduleHandle = Engine.FileSystem.ReadFileAsync(name, version);
                yield return WaitFor.Create(() => readFileScheduleHandle.status == Status.Success || readFileScheduleHandle.status == Status.Failed);
                Engine.Console.Log("Load Asset Bundle:", name, readFileScheduleHandle.version, readFileScheduleHandle.bytes?.Length);
                AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(readFileScheduleHandle.bytes);
                yield return createRequest;
                if (createRequest.isDone is false || createRequest.assetBundle is null)
                {
                    status = Status.Failed;
                    yield break;
                }

                bundle = AssetBundleRuntimeHandle.Create(manifest, createRequest.assetBundle);
                ResourceManager.instance.AddAssetBundleHandle(bundle);
                status = Status.Success;
            }
        }
    }
}