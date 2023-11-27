using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public class NetworkRequest
    {
        public static async UniTask<T> Get<T>(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 5;
            request.useHttpContinue = true;
            await request.SendWebRequest().ToUniTask();
            if (request.result is not UnityWebRequest.Result.Success)
            {
                return default;
            }

            return GetData<T>(request);
        }

        public static async UniTask<T> Post<T>(string url, Dictionary<string, string> headers, object data)
        {
            string str = JsonConvert.SerializeObject(data);
            UnityWebRequest request = UnityWebRequest.PostWwwForm(url, str);
            request.timeout = 5;
            request.useHttpContinue = true;
            request.uploadHandler = new UploadHandlerRaw(UTF8Encoding.UTF8.GetBytes(str));
            if (headers is not null)
            {
                foreach (var VARIABLE in headers)
                {
                    request.SetRequestHeader(VARIABLE.Key, VARIABLE.Value);
                }
            }

            await request.SendWebRequest().ToUniTask();
            if (request.result is not UnityWebRequest.Result.Success)
            {
                return default;
            }

            return GetData<T>(request);
        }

        public static async UniTask<string> Head(string url, string headName)
        {
            UnityWebRequest request = UnityWebRequest.Head(url);
            request.timeout = 5;
            request.useHttpContinue = true;
            //通过UnityWebRequest获取head
            await request.SendWebRequest().ToUniTask();
            string result = request.GetResponseHeader(headName);
            request.Dispose();
            return result;
        }

        private static T GetData<T>(UnityWebRequest request)
        {
            object _data = default;
            if (typeof(T) == typeof(string))
            {
                _data = request.downloadHandler.text;
            }
            else if (typeof(T) == typeof(byte[]))
            {
                _data = request.downloadHandler.data;
            }
            else
            {
                _data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            }

            return (T)_data;
        }
    }
}