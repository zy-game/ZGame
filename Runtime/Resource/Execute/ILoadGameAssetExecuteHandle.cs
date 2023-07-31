using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ZEngine.Resource
{
    public interface ILoadGameAssetExecuteHandle<T> : IRuntimeAssetObjectHandle, IExecute<ILoadGameAssetExecuteHandle<T>> where T : Object
    {
    }

    class DefaultLoadGameAssetExecuteHandle<T> : ILoadGameAssetExecuteHandle<T> where T : Object
    {
        private Status _status;

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
    }
}