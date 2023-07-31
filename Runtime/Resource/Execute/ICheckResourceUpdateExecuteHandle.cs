using System.Collections;
using System.Collections.Generic;

namespace ZEngine.Resource
{
    public interface ICheckResourceUpdateExecuteHandle : IExecuteAsyncHandle<ICheckResourceUpdateExecuteHandle>
    {
        void SubscribeUpdateProgress(ISubscribe<float> subscribe);
    }

    class DefaultCheckResourceUpdateExecuteHandle : IExecuteAsyncHandle<ICheckResourceUpdateExecuteHandle>, ICheckResourceUpdateExecuteHandle
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

        public void SubscribeUpdateProgress(ISubscribe<float> subscribe)
        {
            throw new System.NotImplementedException();
        }
    }
}