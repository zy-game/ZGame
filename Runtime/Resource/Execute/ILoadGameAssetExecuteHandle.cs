using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    public interface ILoadGameAssetExecuteHandle<T> : IExecute<ILoadGameAssetExecuteHandle<T>> where T : Object
    {
        T result { get; }
        void BindGameObject(GameObject gameObject);
        void Unload();
    }

    class DefaultLoadGameAssetExecuteHandle<T> : ILoadGameAssetExecuteHandle<T> where T : Object
    {
        private Status _status;
        private bool isBindObject = false;
        public T result { get; private set; }

        Status IExecute.status
        {
            get => _status;
            set => _status = value;
        }

        public void Release()
        {
            _status = Status.None;
            result = null;
            isBindObject = false;
        }

        public void Execute(params object[] args)
        {
            string path = args[0].ToString();
            BundleManifest manifest = ResourceManager.instance.GetResourceBundleManifest(path);
            if (manifest is null)
            {
                Engine.Console.Error("Not Find The Asset Bundle Manifest");
                _status = Status.Failed;
                return;
            }

            RuntimeAssetBundleHandle runtimeAssetBundleHandle = ResourceManager.instance.GetRuntimeAssetBundleHandle(manifest.name);
            if (runtimeAssetBundleHandle is null)
            {
                //todo 开始加载资源包
            }

            result = runtimeAssetBundleHandle.Load<T>(path);
            _status = Status.Success;
        }


        public void BindGameObject(GameObject gameObject)
        {
            ObserverGameObjectDestroy observerGameObjectDestroy = gameObject.GetComponent<ObserverGameObjectDestroy>();
            if (observerGameObjectDestroy is null)
            {
                observerGameObjectDestroy = gameObject.AddComponent<ObserverGameObjectDestroy>();
            }

            isBindObject = true;
            observerGameObjectDestroy.subscribe.AddListener(Destroy);
        }

        public void Unload()
        {
            if (isBindObject)
            {
                return;
            }

            ResourceManager.instance.Release(result);
        }

        private void Destroy()
        {
            ResourceManager.instance.Release(result);
        }
    }

    class ObserverGameObjectDestroy : MonoBehaviour
    {
        public UnityEvent subscribe = new UnityEvent();

        private void OnDestroy()
        {
            subscribe.Invoke();
        }
    }
}