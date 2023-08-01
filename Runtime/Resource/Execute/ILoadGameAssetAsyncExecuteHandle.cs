using System.Collections;
using UnityEngine;

namespace ZEngine.Resource
{
    public interface ILoadGameAssetAsyncExecuteHandle<T> : ILoadGameAssetExecuteHandle<T>, IExecuteAsyncHandle<ILoadGameAssetAsyncExecuteHandle<T>> where T : Object
    {
    }

    class DefaultLoadGameAssetAsyncExecuteHandle<T> : ILoadGameAssetAsyncExecuteHandle<T> where T : Object
    {
        private Status _status;
        public float progress { get; private set; }
        public T result { get; }

        Status IExecute.status
        {
            get => _status;
            set => _status = value;
        }

        public void Release()
        {
            throw new System.NotImplementedException();
        }

        public void ToBind(GameObject gameObject)
        {
            throw new System.NotImplementedException();
        }


        public void Execute(params object[] args)
        {
            throw new System.NotImplementedException();
        }


        public IEnumerator GetCoroutine()
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(ISubscribe subscribe)
        {
            throw new System.NotImplementedException();
        }

        public void BindGameObject(GameObject gameObject)
        {
            throw new System.NotImplementedException();
        }

        public void Unload()
        {
            throw new System.NotImplementedException();
        }
    }
}