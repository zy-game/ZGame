using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public interface IWebRequestPipeline : IRequest
    {
        string url { get; }
        UniTask<T> GetAsync<T>();
        UniTask<T> PostAsync<T>(Dictionary<string, string> headers, object data);

        public static IWebRequestPipeline Create(string url)
        {
            return new WebRequestPipelineHandle(url);
        }

        class WebRequestPipelineHandle : IWebRequestPipeline
        {
            public string guid { get; }
            public IError error { get; }
            public string url { get; }
            public UnityWebRequest request;

            public WebRequestPipelineHandle(string url)
            {
                this.url = url;
            }

            public async UniTask<T> GetAsync<T>()
            {
                request = UnityWebRequest.Get(url);
                await request.SendWebRequest().ToUniTask();
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    return default;
                }

                return GetData<T>();
            }

            public async UniTask<T> PostAsync<T>(Dictionary<string, string> headers, object data)
            {
                string str = JsonConvert.SerializeObject(data);
                request = UnityWebRequest.PostWwwForm(url, str);
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

                return GetData<T>();
            }


            private T GetData<T>()
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

            public void Dispose()
            {
            }
        }
    }
}