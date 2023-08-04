using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    class DefaultAssetBundleRequestExecuteHandle : IAssetBundleRequestExecuteHandle, IAssetBundleRequestResult
    {
        public string name { get; set; }
        public string path { get; set; }
        public Status status { get; set; }
        public string module { get; set; }
        public VersionOptions version { get; set; }
        public IRuntimeBundleHandle bundle { get; set; }
        public float progress => loadCount / count;

        private float count;
        private float loadCount;
        private List<ISubscribeExecuteHandle> subscribeExecuteHandles = new List<ISubscribeExecuteHandle>();
        private List<ISubscribeExecuteHandle<float>> progresListener = new List<ISubscribeExecuteHandle<float>>();

        class LoadBundleData
        {
            public Status status;
            public RuntimeBundleManifest manifest;
            public AssetBundle assetBundle;
        }

        public void Release()
        {
            bundle = null;
            name = String.Empty;
            path = String.Empty;
            module = String.Empty;
            version = null;
            status = Status.None;
        }

        public IEnumerator Complete()
        {
            return new WaitUntil(() => status == Status.Failed || status == Status.Success);
        }

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            subscribeExecuteHandles.Add(subscribe);
        }

        public void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe)
        {
            progresListener.Add(subscribe);
        }

        public IEnumerator Execute(params object[] paramsList)
        {
            RuntimeBundleManifest manifest = (RuntimeBundleManifest)paramsList[0];
            name = manifest.name;
            module = manifest.owner;
            version = manifest.version;
            path = VFSManager.GetLocalFilePath(name);
            RuntimeBundleManifest[] manifests = ResourceManager.instance.GetBundleDependenciesList(manifest);
            count = manifests.Length;
            if (manifests is null || manifests.Length is 0)
            {
                status = Status.Failed;
                subscribeExecuteHandles.ForEach(x => x.Execute(this));
                yield break;
            }

            LoadBundleData[] loadBundleDatas = new LoadBundleData[manifests.Length];
            for (int i = 0; i < manifests.Length; i++)
            {
                loadBundleDatas[i] = new LoadBundleData()
                {
                    manifest = manifests[i],
                    status = Status.None
                };
                LoadBundleAsync(loadBundleDatas[i]).StartCoroutine();
            }

            bool CheckComplete()
            {
                progresListener.ForEach(x => x.Execute(progress));
                return loadBundleDatas.Where(x => x.status == Status.Execute).Count() == 0;
            }

            yield return new WaitUntil(CheckComplete);
            bool success = loadBundleDatas.Where(x => x.status == Status.Failed).Count() == 0;
            for (int i = 0; i < loadBundleDatas.Length; i++)
            {
                if (success is false)
                {
                    loadBundleDatas[i].assetBundle.Unload(true);
                    continue;
                }

                IRuntimeBundleHandle runtimeBundleManifest = RuntimeAssetBundleHandle.Create(loadBundleDatas[i].manifest, loadBundleDatas[i].assetBundle);
                ResourceManager.instance.AddAssetBundleHandle(runtimeBundleManifest);
                if (manifest == loadBundleDatas[i].manifest)
                {
                    bundle = runtimeBundleManifest;
                }
            }

            subscribeExecuteHandles.ForEach(x => x.Execute(this));
            status = success ? Status.Success : Status.Failed;
        }

        private IEnumerator LoadBundleAsync(LoadBundleData item)
        {
            item.status = Status.Execute;
            IReadFileExecuteHandle readFileExecuteHandle = Engine.FileSystem.ReadFileAsync(item.manifest.name);
            yield return readFileExecuteHandle.Complete();
            AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(readFileExecuteHandle.bytes);
            yield return createRequest;
            if (createRequest.isDone is false || createRequest.assetBundle is null)
            {
                item.status = Status.Failed;
                yield break;
            }

            item.assetBundle = createRequest.assetBundle;
            loadCount++;
            item.status = Status.Success;
        }
    }
}