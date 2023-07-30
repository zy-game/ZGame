using System.Collections;
using UnityEngine;

namespace ZEngine.Resource
{
    public interface ILoadGameAssetAsyncExecuteHandle<T> : IExecuteAsyncHandle<ILoadGameAssetAsyncExecuteHandle<T>> where T : Object
    {
    }

    class DefaultLoadGameAssetAsyncExecuteHandle<T> : ILoadGameAssetAsyncExecuteHandle<T> where T : Object
    {
        public void Subscribe(ISubscribe<ILoadGameAssetAsyncExecuteHandle<T>> subscribe)
        {
            throw new System.NotImplementedException();
        }

        public void Release()
        {
            throw new System.NotImplementedException();
        }

        public bool EnsureExecuteSuccessfuly()
        {
            throw new System.NotImplementedException();
        }

        public void Execute(params object[] args)
        {
            throw new System.NotImplementedException();
        }

        public float progress { get; }

        public IEnumerator GetCoroutine()
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(ISubscribe subscribe)
        {
            throw new System.NotImplementedException();
        }

        public void Paused()
        {
            throw new System.NotImplementedException();
        }

        public void Resume()
        {
            throw new System.NotImplementedException();
        }

        public void Cancel()
        {
            throw new System.NotImplementedException();
        }
    }
}