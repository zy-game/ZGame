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

    public class NetworkManager : Single<NetworkManager>
    {
        public IGameAsyncExecuteHandle<string> Get(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }

        public IGameAsyncExecuteHandle<string> Post(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }

        public IGameAsyncExecuteHandle<T> Get<T>(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }

        public IGameAsyncExecuteHandle<T> Post<T>(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }
    }
}