using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    class DefaultAssetRequestExecute<T> : IAssetRequestExecute<T> where T : Object
    {
        private Status status;
        private bool isBindObject = false;

        public string path { get; set; }
        public T result { get; private set; }

        public void Release()
        {
            status = Status.None;
            result = null;
            isBindObject = false;
        }

        public bool EnsureExecuteSuccessfuly()
        {
            return status == Status.Success;
        }

        public void Execute(params object[] args)
        {
            path = args[0].ToString();
            BundleManifest manifest = ResourceManager.instance.GetResourceBundleManifest(path);
            if (manifest is null)
            {
                Engine.Console.Error("Not Find The Asset Bundle Manifest");
                status = Status.Failed;
                return;
            }

            RuntimeAssetBundleHandle runtimeAssetBundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.name);
            if (runtimeAssetBundleHandle is null)
            {
                DefaultAssetBundleRequestExecute defaultLoadAssetBundleExecuteHandle = Engine.Class.Loader<DefaultAssetBundleRequestExecute>();
                defaultLoadAssetBundleExecuteHandle.Execute(manifest);
                runtimeAssetBundleHandle = RuntimeAssetBundleHandle.Create(manifest, defaultLoadAssetBundleExecuteHandle.result);
            }

            result = runtimeAssetBundleHandle.Load<T>(path);
            status = result == null ? Status.Failed : Status.Success;
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