using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZEngine.Resource
{
    class DefaultAssetRequestExecuteHandle<T> : IAssetRequestExecuteHandle<T> where T : Object
    {
        private Status status;
        private IEnumerator _enumerator;
        private bool isBindObject = false;
        private List<ISubscribe> _subscribes = new List<ISubscribe>();

        public T result { get; set; }
        public string path { get; set; }
        public float progress { get; set; }

        public void Release()
        {
            _enumerator = null;
            _subscribes.ForEach(Engine.Class.Release);
            _subscribes.Clear();
            status = Status.None;
            progress = 0;
            result = default;
        }

        public IEnumerator Execute(params object[] args)
        {
            path = args[0].ToString();
            BundleManifest manifest = ResourceManager.instance.GetResourceBundleManifest(path);
            if (manifest is null)
            {
                Engine.Console.Error("Not Find The Asset Bundle Manifest");
                status = Status.Failed;
                yield break;
            }

            RuntimeAssetBundleHandle runtimeAssetBundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.name);
            if (runtimeAssetBundleHandle is null)
            {
                DefaultAssetBundleRequestExecuteHandle defaultLoadAssetBundleAsyncExecuteHandle = Engine.Class.Loader<DefaultAssetBundleRequestExecuteHandle>();
                yield return defaultLoadAssetBundleAsyncExecuteHandle.Execute(manifest);
                runtimeAssetBundleHandle = RuntimeAssetBundleHandle.Create(manifest, defaultLoadAssetBundleAsyncExecuteHandle.result);
            }

            yield return runtimeAssetBundleHandle.LoadAsync<T>(path, Method<T>.Create(args => { result = args; }));
            status = result == null ? Status.Failed : Status.Success;
        }

        public void Subscribe(ISubscribe subscribe)
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