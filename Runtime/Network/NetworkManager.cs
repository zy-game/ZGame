using System;
using System.Collections.Generic;

namespace ZEngine.Network
{
    [Serializable]
    public sealed class NetworkAddressOptions
    {
        public bool isOn;
        public string name;
        public string address;
        public ushort port;
    }

    public interface INetworkRequestExecuteAsyncHandleHandle<T> : IExecuteHandle<INetworkRequestExecuteAsyncHandleHandle<T>>
    {
        
    }

    public class NetworkManager : Single<NetworkManager>
    {
        public INetworkRequestExecuteAsyncHandleHandle<string> Get(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }

        public INetworkRequestExecuteAsyncHandleHandle<string> Post(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }

        public INetworkRequestExecuteAsyncHandleHandle<T> Get<T>(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }

        public INetworkRequestExecuteAsyncHandleHandle<T> Post<T>(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }
    }
}