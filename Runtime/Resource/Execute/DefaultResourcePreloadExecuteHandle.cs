using System.Collections;

namespace ZEngine.Resource
{
    class DefaultResourcePreloadExecuteHandle : IResourcePreloadExecuteHandle
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

        public void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe)
        {
            throw new System.NotImplementedException();
        }

        public void Release()
        {
            throw new System.NotImplementedException();
        }
    }
}