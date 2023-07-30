using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    public interface ILoadGameAssetExecuteHandle<T> : IExecuteHandle<ILoadGameAssetExecuteHandle<T>> where T : Object
    {
    }

    class DefaultLoadGameAssetExecuteHandle<T> : ILoadGameAssetExecuteHandle<T> where T : Object
    {
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
    }
}