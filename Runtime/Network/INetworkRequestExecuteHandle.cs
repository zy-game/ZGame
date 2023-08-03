using System.Collections;

namespace ZEngine.Network
{
    public enum NetworkRequestMethod : byte
    {
        NONE,
        POST,
        GET,
        DELETE,
        PUT,
    }

    public interface INetworkRequestExecuteHandle<T> : IExecuteHandle<INetworkRequestExecuteHandle<T>>
    {
        T result { get; }
        string url { get; }
        NetworkRequestMethod method { get; }

        void ObserverExecuteProgress(ISubscribeExecuteHandle<float> subscribe);
    }

    class DefaultNetworkRequestExecuteHandle<T> : INetworkRequestExecuteHandle<T>
    {
        public T result { get; }
        public string url { get; }
        public float progress { get; }
        public Status status { get; set; }
        public NetworkRequestMethod method { get; }

        public void ObserverExecuteProgress(ISubscribeExecuteHandle<float> subscribe)
        {
            throw new System.NotImplementedException();
        }

        public void Release()
        {
            throw new System.NotImplementedException();
        }


        public void Execute(params object[] paramsList)
        {
            throw new System.NotImplementedException();
        }

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            throw new System.NotImplementedException();
        }
    }
}