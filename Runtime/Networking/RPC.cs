using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace ZGame.Networking
{
    public class RPC<T> where T : class
    {
        public int code;
        public string msg;
        public T data;

        public bool IsSuccess()
        {
            return code == 0 || code == 200;
        }

        public static UniTask<RPC<T>> CallPost(string url, object data)
        {
            return NetworkManager.Post<RPC<T>>(url, data);
        }

        public static UniTask<RPC<T>> CallPost(string url, object data, Dictionary<string, object> headers)
        {
            return NetworkManager.Post<RPC<T>>(url, data, headers);
        }

        public static UniTask<RPC<T>> CallGet(string url)
        {
            return NetworkManager.Get<RPC<T>>(url);
        }
    }
}