using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    class DefaultAssetRequestExecute<T> : IAssetRequestExecute<T> where T : Object
    {
        private bool isBindObject = false;
        public string path { get; set; }
        public T result { get; private set; }

        public void Release()
        {
            result = null;
            isBindObject = false;
        }

        public T Execute(params object[] args)
        {
            path = args[0].ToString();
            RuntimeBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
            if (manifest is null)
            {
                Engine.Console.Error("Not Find The Asset Bundle Manifest");
                return default;
            }

            IRuntimeBundleHandle runtimeAssetBundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.name);
            if (runtimeAssetBundleHandle is null && HotfixOptions.instance.autoLoad == Switch.On)
            {
                DefaultAssetBundleRequestExecute defaultLoadAssetBundleExecuteHandle = Engine.Class.Loader<DefaultAssetBundleRequestExecute>();
                IAssetBundleRequestResult assetBundleRequestResult = defaultLoadAssetBundleExecuteHandle.Execute(manifest);
                if (assetBundleRequestResult is null || assetBundleRequestResult.bundle is null)
                {
                    return default;
                }

                runtimeAssetBundleHandle = assetBundleRequestResult.bundle;
            }

            if (runtimeAssetBundleHandle is null)
            {
                Engine.Console.Error($"Not find the asset bundle:{manifest.name}.please check your is loaded the bundle");
                return default;
            }

            return result = runtimeAssetBundleHandle.Load<T>(path);
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