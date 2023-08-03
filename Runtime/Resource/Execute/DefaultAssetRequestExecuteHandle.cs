using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Resource
{
    class DefaultAssetRequestExecuteHandle<T> : IAssetRequestExecuteHandle<T> where T : Object
    {
        private IEnumerator _enumerator;
        private bool isBindObject = false;
        private List<ISubscribeExecuteHandle> _subscribes = new List<ISubscribeExecuteHandle>();
        public string path { get; set; }
        public Status status { get; set; }
        public float progress { get; set; }
        public T result { get; private set; }


        public void Release()
        {
            _enumerator = null;
            _subscribes.ForEach(Engine.Class.Release);
            _subscribes.Clear();
            status = Status.None;
            progress = 0;
        }

        public void Execute(params object[] args)
        {
            path = args[0].ToString();
            OnStartLoadAsset().StartCoroutine();
        }

        private IEnumerator OnStartLoadAsset()
        {
            BundleManifest manifest = ResourceManager.instance.GetResourceBundleManifest(path);
            if (manifest is null)
            {
                Engine.Console.Error("Not Find The Asset Bundle Manifest");
                status = Status.Failed;
                yield break;
            }

            IRuntimeBundleManifest runtimeAssetBundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.name);
            if (runtimeAssetBundleHandle is null)
            {
                IAssetBundleRequestExecuteHandle assetBundleRequestExecuteHandle = Engine.Class.Loader<DefaultAssetBundleRequestExecuteHandle>();
                assetBundleRequestExecuteHandle.Execute(manifest);
                yield return assetBundleRequestExecuteHandle.Complete();
                runtimeAssetBundleHandle = assetBundleRequestExecuteHandle.result;
            }

            yield return runtimeAssetBundleHandle.LoadAsync<T>(path, ISubscribeExecuteHandle<T>.Create(args => { result = args; }));
            _subscribes.ForEach(x => x.Execute(this));
            status = result == null ? Status.Failed : Status.Success;
        }

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            _subscribes.Add(subscribe);
        }

        public void LinkObject(GameObject gameObject)
        {
            ObserverGameObjectDestroy observerGameObjectDestroy = gameObject.GetComponent<ObserverGameObjectDestroy>();
            if (observerGameObjectDestroy is null)
            {
                observerGameObjectDestroy = gameObject.AddComponent<ObserverGameObjectDestroy>();
            }

            isBindObject = true;
            observerGameObjectDestroy.subscribe.AddListener(() => { ResourceManager.instance.Release(result); });
        }

        public void FreeAsset()
        {
            if (isBindObject)
            {
                return;
            }

            ResourceManager.instance.Release(result);
        }
    }
}