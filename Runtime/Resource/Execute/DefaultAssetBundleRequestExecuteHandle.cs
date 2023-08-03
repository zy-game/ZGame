using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    class DefaultAssetBundleRequestExecuteHandle : IAssetBundleRequestExecuteHandle
    {
        public string name { get; set; }
        public string path { get; set; }
        public Status status { get; set; }
        public string module { get; set; }
        public IRuntimeBundleManifest result { get; set; }
        public VersionOptions version { get; set; }
        public float progress => loadCount / count;


        private float count;
        private float loadCount;
        private List<ISubscribeExecuteHandle> subscribeExecuteHandles = new List<ISubscribeExecuteHandle>();
        private List<ISubscribeExecuteHandle<float>> progresListener = new List<ISubscribeExecuteHandle<float>>();

        class LoadItem
        {
            public Status status;
            public RuntimeBundleManifest manifest;
            public AssetBundle assetBundle;
        }

        public void Release()
        {
            result = null;
            name = String.Empty;
            path = String.Empty;
            module = String.Empty;
            version = null;
            status = Status.None;
        }

        public void Execute(params object[] paramsList)
        {
            RuntimeBundleManifest manifest = (RuntimeBundleManifest)paramsList[0];
            name = manifest.name;
            module = manifest.owner;
            version = manifest.version;
            path = VFSManager.GetLocalFilePath(name);
            List<RuntimeBundleManifest> manifests = GetDependenciesList(manifest);
            count = manifests.Count;
            if (manifests is null || manifests.Count is 0)
            {
                status = Status.Failed;
                subscribeExecuteHandles.ForEach(x => x.Execute(this));
                return;
            }

            OnStartLoadBundle(manifest, manifests.ToArray()).StartCoroutine();
        }

        private IEnumerator OnStartLoadBundle(RuntimeBundleManifest main, RuntimeBundleManifest[] manifests)
        {
            LoadItem[] items = new LoadItem[manifests.Length];
            for (int i = 0; i < manifests.Length; i++)
            {
                items[i] = new LoadItem()
                {
                    manifest = manifests[i],
                    status = Status.None
                };
                LoadBundleAsync(items[i]).StartCoroutine();
            }

            bool CheckComplete()
            {
                progresListener.ForEach(x => x.Execute(progress));
                return items.Where(x => x.status == Status.Execute).Count() == 0;
            }

            yield return new WaitUntil(CheckComplete);
            bool success = items.Where(x => x.status == Status.Failed).Count() == 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (success is false)
                {
                    items[i].assetBundle.Unload(true);
                    continue;
                }

                IRuntimeBundleManifest runtimeBundleManifest = RuntimeAssetBundleHandle.Create(items[i].manifest, items[i].assetBundle);
                ResourceManager.instance.AddAssetBundleHandle(runtimeBundleManifest);
                if (main == items[i].manifest)
                {
                    result = runtimeBundleManifest;
                }
            }

            subscribeExecuteHandles.ForEach(x => x.Execute(this));
            status = success ? Status.Success : Status.Failed;
        }

        private IEnumerator LoadBundleAsync(LoadItem item)
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

        private List<RuntimeBundleManifest> GetDependenciesList(RuntimeBundleManifest manifest)
        {
            List<RuntimeBundleManifest> list = new List<RuntimeBundleManifest>() { manifest };
            if (manifest.dependencies is null || manifest.dependencies.Count is 0)
            {
                return list;
            }

            for (int i = 0; i < manifest.dependencies.Count; i++)
            {
                RuntimeBundleManifest bundleManifest = ResourceManager.instance.GetResourceBundleManifest(manifest.dependencies[i]);
                if (bundleManifest is null)
                {
                    Engine.Console.Error("Not Find AssetBundle Dependencies:" + manifest.dependencies[i]);
                    return default;
                }

                List<RuntimeBundleManifest> manifests = GetDependenciesList(bundleManifest);
                foreach (var target in manifests)
                {
                    if (list.Contains(target))
                    {
                        continue;
                    }

                    list.Add(target);
                }
            }

            return list;
        }

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            subscribeExecuteHandles.Add(subscribe);
        }

        public void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe)
        {
            progresListener.Add(subscribe);
        }
    }
}