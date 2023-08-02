using System.Collections;

namespace ZEngine.Resource
{
    class DefaultResourcePreloadExecuteHandle : IResourcePreloadExecuteHandle
    {
        private Status status;

        public void Release()
        {
            throw new System.NotImplementedException();
        }

        public float progress { get; }

        public IEnumerator Execute(params object[] paramsList)
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(ISubscribe subscribe)
        {
            throw new System.NotImplementedException();
        }

        public void ObserverPorgress(ISubscribe<float> subscribe)
        {
            throw new System.NotImplementedException();
        }
    }
}