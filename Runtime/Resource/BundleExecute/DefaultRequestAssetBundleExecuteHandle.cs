using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    class DefaultRequestAssetBundleExecuteHandle : ExecuteHandle<RequestAssetBundleResult>
    {
        private float count;
        private float loadCount;
        private RuntimeBundleManifest manifest;

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
            string name = manifest.name;
            string module = manifest.owner;
            VersionOptions version = manifest.version;
            string path = Engine.Custom.GetLocalFilePath(name);
            OnStart(name, path, module, version).StartCoroutine();
        }

        IEnumerator OnStart(string name, string path, string module, VersionOptions ver)
        {
            RuntimeBundleManifest[] manifests = ResourceManager.instance.GetBundleDependenciesList(manifest);
            count = manifests.Length;
            if (manifests is null || manifests.Length is 0)
            {
                status = Status.Failed;
                OnProgress(1);
                OnComplete();
                yield break;
            }

            List<LoadBundleData> loadBundleDatas = new List<LoadBundleData>();
            for (int i = 0; i < manifests.Length; i++)
            {
                if (ResourceManager.instance.HasLoadAssetBundle(manifests[i].owner, manifests[i].name))
                {
                    continue;
                }

                loadBundleDatas.Add(new LoadBundleData()
                {
                    manifest = manifests[i],
                    status = Status.None
                });
                LoadBundleAsync(loadBundleDatas[i]).StartCoroutine();
            }

            bool CheckComplete()
            {
                OnProgress(loadCount / count);
                return loadBundleDatas.Where(x => x.status == Status.Execute).Count() == 0;
            }

            yield return WaitFor.Create(CheckComplete);
            bool success = loadBundleDatas.Where(x => x.status == Status.Failed).Count() == 0;
            InternalRuntimeBundleHandle mainBundle = default;
            for (int i = 0; i < loadBundleDatas.Count; i++)
            {
                if (success is false)
                {
                    loadBundleDatas[i].assetBundle.Unload(true);
                    continue;
                }

                InternalRuntimeBundleHandle runtimeBundleManifest = InternalRuntimeBundleHandle.Create(loadBundleDatas[i].manifest, loadBundleDatas[i].assetBundle);
                ResourceManager.instance.AddAssetBundleHandle(runtimeBundleManifest);
                if (manifest == loadBundleDatas[i].manifest)
                {
                    mainBundle = runtimeBundleManifest;
                }
            }

            result = RequestAssetBundleResult.Create(name, path, module, ver, mainBundle);
            status = success ? Status.Success : Status.Failed;
            OnComplete();
        }

        private IEnumerator LoadBundleAsync(LoadBundleData item)
        {
            item.status = Status.Execute;
            IReadFileExecuteHandle readFileExecuteHandle = Engine.FileSystem.ReadFileAsync(item.manifest.name, item.manifest.version);
            yield return readFileExecuteHandle.ExecuteComplete();
            AssetBundleCreateRequest createRequest = AssetBundle.LoadFromMemoryAsync(readFileExecuteHandle.result.bytes);
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