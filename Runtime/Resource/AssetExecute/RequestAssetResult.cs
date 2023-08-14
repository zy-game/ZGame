using UnityEngine;

namespace ZEngine.Resource
{
    /// <summary>
    /// 资源加载
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RequestAssetResult<T> : IReference where T : Object
    {
        public T result;
        public string path;
        private bool isBindObject;

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

        public void Release()
        {
        }

        public static RequestAssetResult<T> Create(string path, T target)
        {
            RequestAssetResult<T> assetBundleRequestResult = Engine.Class.Loader<RequestAssetResult<T>>();
            assetBundleRequestResult.path = path;
            assetBundleRequestResult.result = target;
            assetBundleRequestResult.isBindObject = false;
            return assetBundleRequestResult;
        }
    }
}