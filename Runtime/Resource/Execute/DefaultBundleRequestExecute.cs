using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    class DefaultBundleRequestExecute : IAssetBundleRequestExecute, IAssetBundleRequestResult
    {
        public string name { get; set; }
        public string path { get; set; }
        public string module { get; set; }
        public VersionOptions version { get; set; }
        public IRuntimeBundleHandle bundle { get; set; }


        public void Release()
        {
            bundle = null;
            name = String.Empty;
            path = String.Empty;
            module = String.Empty;
            version = null;
        }

        public IAssetBundleRequestResult Execute(params object[] args)
        {
            RuntimeBundleManifest manifest = (RuntimeBundleManifest)args[0];
            name = manifest.name;
            module = manifest.owner;
            version = manifest.version;
            path = VFSManager.GetLocalFilePath(name);
            RuntimeBundleManifest[] manifests = ResourceManager.instance.GetBundleDependenciesList(manifest);
            if (manifests is null || manifests.Length is 0)
            {
                return default;
            }

            bool success = true;
            Dictionary<RuntimeBundleManifest, AssetBundle> map = new Dictionary<RuntimeBundleManifest, AssetBundle>();
            for (int i = 0; i < manifests.Length; i++)
            {
                if (ResourceManager.instance.HasLoadAssetBundle(manifests[i].name))
                {
                    continue;
                }

                IReadFileExecute readFileExecute = Engine.FileSystem.ReadFile(manifests[i].name);
                if (readFileExecute.bytes is null || readFileExecute.bytes.Length is 0)
                {
                    success = false;
                    break;
                }

                AssetBundle assetBundle = AssetBundle.LoadFromMemory(readFileExecute.bytes);
                if (assetBundle is null)
                {
                    success = false;
                    break;
                }

                map.Add(manifests[i], assetBundle);
            }

            foreach (var VARIABLE in map)
            {
                if (success is false)
                {
                    VARIABLE.Value.Unload(true);
                    continue;
                }

                IRuntimeBundleHandle runtimeBundleManifest = RuntimeAssetBundleHandle.Create(VARIABLE.Key, VARIABLE.Value);
                ResourceManager.instance.AddAssetBundleHandle(runtimeBundleManifest);
                if (manifest == VARIABLE.Key)
                {
                    bundle = runtimeBundleManifest;
                }
            }

            return this;
        }
    }

    class DefaultBundleRequestExecuteHandle : IAssetBundleRequestExecuteHandle, IAssetBundleRequestResult
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
        private RuntimeBundleManifest manifest;
        private ISubscribeHandle<float> progresListener;
        private List<ISubscribeHandle> subscribeExecuteHandles = new List<ISubscribeHandle>();

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

        public IEnumerator ExecuteComplete()
        {
            return WaitFor.Create(() => status == Status.Failed || status == Status.Success);
        }

        public void Subscribe(ISubscribeHandle subscribe)
        {
            subscribeExecuteHandles.Add(subscribe);
        }

        public void OnPorgressChange(ISubscribeHandle<float> subscribe)
        {
            progresListener = subscribe;
        }

        public void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            manifest = (RuntimeBundleManifest)paramsList[0];
            name = manifest.name;
            module = manifest.owner;
            version = manifest.version;
            path = VFSManager.GetLocalFilePath(name);
            OnStart().StartCoroutine();
        }

        IEnumerator OnStart()
        {
            RuntimeBundleManifest[] manifests = ResourceManager.instance.GetBundleDependenciesList(manifest);
            count = manifests.Length;
            if (manifests is null || manifests.Length is 0)
            {
                status = Status.Failed;
                progresListener.Execute(1);
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
                progresListener.Execute(progress);
                return loadBundleDatas.Where(x => x.status == Status.Execute).Count() == 0;
            }

            yield return WaitFor.Create(CheckComplete);
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

            status = success ? Status.Success : Status.Failed;
            subscribeExecuteHandles.ForEach(x => x.Execute(this));
        }

        private IEnumerator LoadBundleAsync(LoadBundleData item)
        {
            item.status = Status.Execute;
            IReadFileExecuteHandle readFileExecuteHandle = Engine.FileSystem.ReadFileAsync(item.manifest.name);
            yield return readFileExecuteHandle.ExecuteComplete();
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