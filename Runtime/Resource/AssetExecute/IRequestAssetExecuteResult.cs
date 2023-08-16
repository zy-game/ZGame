using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    public interface IRequestAssetExecuteResult<T> : IReference where T : Object
    {
        T asset { get; }
        string path { get; }
        void BindTo(GameObject gameObject);
        void Free();
    }

    class InternalRequestAssetExecuteResult<T> : IRequestAssetExecuteResult<T> where T : Object
    {
        public void Release()
        {
            asset = null;
            path = String.Empty;
            isBindObject = false;
        }

        public T asset { get; set; }
        public string path { get; set; }
        private bool isBindObject;

        public void BindTo(GameObject gameObject)
        {
            ObserverGameObjectDestroy observerGameObjectDestroy = gameObject.GetComponent<ObserverGameObjectDestroy>();
            if (observerGameObjectDestroy is null)
            {
                observerGameObjectDestroy = gameObject.AddComponent<ObserverGameObjectDestroy>();
            }

            isBindObject = true;
            observerGameObjectDestroy.subscribe.AddListener(() => { ResourceManager.instance.Release(asset); });
        }

        public void Free()
        {
            if (isBindObject)
            {
                return;
            }

            ResourceManager.instance.Release(asset);
        }
    }
}