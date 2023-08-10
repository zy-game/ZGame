using System.Collections;
using System.Collections.Generic;
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
            if (path.StartsWith("Resources"))
            {
                string temp = path.Substring("Resources/".Length);
                return result = Resources.Load<T>(temp);
            }
            else
            {
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.On && HotfixOptions.instance.useAsset == Switch.On)
                {
                    return result = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
                }
#endif
                RuntimeBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
                if (manifest is null)
                {
                    Engine.Console.Error("Not Find The Asset Bundle Manifest");
                    return default;
                }

                IRuntimeBundleHandle runtimeAssetBundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.owner, manifest.name);
                if (runtimeAssetBundleHandle is null && HotfixOptions.instance.autoLoad == Switch.On)
                {
                    DefaultBundleRequestExecute defaultLoadAssetBundleExecuteHandle = Engine.Class.Loader<DefaultBundleRequestExecute>();
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

    class DefaultAssetRequestExecuteHandle<T> : IAssetRequestExecuteHandle<T> where T : Object
    {
        private IEnumerator _enumerator;
        private bool isBindObject = false;
        private List<ISubscribeHandle> _subscribes = new List<ISubscribeHandle>();
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

        public IEnumerator ExecuteComplete()
        {
            return WaitFor.Create(() => status == Status.Failed || status == Status.Success);
        }

        public void Subscribe(ISubscribeHandle subscribe)
        {
            _subscribes.Add(subscribe);
        }

        public void Execute(params object[] args)
        {
            status = Status.Execute;
            path = args[0].ToString();
            OnStart().StartCoroutine();
        }

        IEnumerator OnStart()
        {
            RuntimeBundleManifest manifest = ResourceManager.instance.GetBundleManifestWithAssetPath(path);
            if (manifest is null)
            {
                Engine.Console.Error("Not Find The Asset Bundle Manifest");
                status = Status.Failed;
                yield break;
            }

            IRuntimeBundleHandle runtimeAssetBundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.owner, manifest.name);
            if (runtimeAssetBundleHandle is null && HotfixOptions.instance.autoLoad == Switch.On)
            {
                IAssetBundleRequestExecuteHandle assetBundleRequestExecuteHandle = Engine.Class.Loader<DefaultBundleRequestExecuteHandle>();
                assetBundleRequestExecuteHandle.Execute(manifest);
                yield return assetBundleRequestExecuteHandle.ExecuteComplete();
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

            yield return runtimeAssetBundleHandle.LoadAsync<T>(path, ISubscribeHandle.Create<T>(args => { result = args; }));
            status = result == null ? Status.Failed : Status.Success;
            status = Status.Success;
            _subscribes.ForEach(x => x.Execute(this));
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