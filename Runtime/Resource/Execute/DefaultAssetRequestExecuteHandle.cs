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
        public T result { get; private set; }


        public void Release()
        {
            _enumerator = null;
            _subscribes.ForEach(Engine.Class.Release);
            _subscribes.Clear();
            status = Status.None;
        }

        public IEnumerator Complete()
        {
            return WaitFor.Create(() => status == Status.Failed || status == Status.Success);
        }

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            _subscribes.Add(subscribe);
        }

        public IEnumerator Execute(params object[] args)
        {
            status = Status.Execute;
            path = args[0].ToString();
            RuntimeBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
            if (manifest is null)
            {
                Engine.Console.Error("Not Find The Asset Bundle Manifest");
                status = Status.Failed;
                yield break;
            }

            IRuntimeBundleHandle runtimeAssetBundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.name);
            if (runtimeAssetBundleHandle is null && HotfixOptions.instance.autoLoad == Switch.On)
            {
                IAssetBundleRequestExecuteHandle assetBundleRequestExecuteHandle = Engine.Class.Loader<DefaultAssetBundleRequestExecuteHandle>();
                yield return assetBundleRequestExecuteHandle.Execute(manifest);
                if (assetBundleRequestExecuteHandle is null || assetBundleRequestExecuteHandle.bundle is null)
                {
                    status = Status.Failed;
                    _subscribes.ForEach(x => x.Execute(this));
                    yield break;
                }

                runtimeAssetBundleHandle = assetBundleRequestExecuteHandle.bundle;
            }

            if (runtimeAssetBundleHandle is null)
            {
                Engine.Console.Error($"Not find the asset bundle:{manifest.name}.please check your is loaded the bundle");
                status = Status.Failed;
                yield break;
            }

            yield return runtimeAssetBundleHandle.LoadAsync<T>(path, ISubscribeExecuteHandle<T>.Create(args => { result = args; }));
            _subscribes.ForEach(x => x.Execute(this));
            status = result == null ? Status.Failed : Status.Success;
            status = Status.Success;
        }

        public void Link(GameObject gameObject)
        {
            ObserverGameObjectDestroy observerGameObjectDestroy = gameObject.GetComponent<ObserverGameObjectDestroy>();
            if (observerGameObjectDestroy is null)
            {
                observerGameObjectDestroy = gameObject.AddComponent<ObserverGameObjectDestroy>();
            }

            isBindObject = true;
            observerGameObjectDestroy.subscribe.AddListener(() => { ResourceManager.instance.Release(result); });
        }

        public void Free()
        {
            if (isBindObject)
            {
                return;
            }

            ResourceManager.instance.Release(result);
        }
    }
}