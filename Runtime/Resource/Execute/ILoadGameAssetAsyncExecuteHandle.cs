using System.Collections;
using UnityEngine;

namespace ZEngine.Resource
{
    public interface ILoadGameAssetAsyncExecuteHandle<T> : IRuntimeAssetObjectHandle, IExecuteAsyncHandle<ILoadGameAssetAsyncExecuteHandle<T>> where T : Object
    {
    }

    class DefaultLoadGameAssetAsyncExecuteHandle<T> : ILoadGameAssetAsyncExecuteHandle<T> where T : Object
    {
        private Status _status;
        public float progress { get; private set; }

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
    }
}