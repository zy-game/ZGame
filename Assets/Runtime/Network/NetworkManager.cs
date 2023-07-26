using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

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
        public UniTask<string> Get(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }

        public UniTask<string> Post(string url, object data, Dictionary<string, object> header = default)
        {
            return default;
        }

        public async UniTask<T> Get<T>(string url, object data, Dictionary<string, object> header = default)
        {
            string result = await Get(url, data, header);
            return Engine.Json.Parse<T>(result);
        }

        public async UniTask<T> Post<T>(string url, object data, Dictionary<string, object> header = default)
        {
            string result = await Post(url, data, header);
            return Engine.Json.Parse<T>(result);
        }
    }
}