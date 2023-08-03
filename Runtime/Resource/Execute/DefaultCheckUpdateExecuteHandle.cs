using System.Collections;
using System.Collections.Generic;

namespace ZEngine.Resource
{
    class DefaultCheckUpdateExecuteHandle : IExecuteHandle<ICheckUpdateExecuteHandle>, ICheckUpdateExecuteHandle
    {
        public float progress { get; }
        public Status status { get; set; }

        public void Execute(params object[] paramsList)
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            throw new System.NotImplementedException();
        }

        public void ObserverPorgress(ISubscribeExecuteHandle<float> subscribe)
        {
            throw new System.NotImplementedException();
        }

        public void Release()
        {
            throw new System.NotImplementedException();
        }
    }
}