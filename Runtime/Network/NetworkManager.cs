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

    public interface INetworkRequestExecuteHandle<T> : IExecuteAsyncHandle<INetworkRequestExecuteHandle<T>>
    {
        
    }

    public class NetworkManager : Single<NetworkManager>
    {
        public INetworkRequestExecuteHandle<string> Get(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }

        public INetworkRequestExecuteHandle<string> Post(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }

        public INetworkRequestExecuteHandle<T> Get<T>(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }

        public INetworkRequestExecuteHandle<T> Post<T>(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }
    }
}