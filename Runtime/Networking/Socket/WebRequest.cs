using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public interface IWebRequestPipeline : IDisposable
    {
        string url { get; }
        UniTask<T> GetAsync<T>();
        UniTask<T> PostAsync<T>(Dictionary<string, string> headers, object data);
        UniTask<string> Head(string headName);

        public static IWebRequestPipeline Create(string url)
        {
            return new WebRequestPipelineHandle(url);
        }

        class WebRequestPipelineHandle : IWebRequestPipeline
        {
            public string url { get; }
            public UnityWebRequest request;

            public WebRequestPipelineHandle(string url)
            {
                this.url = url;
            }

            IEnumerator Start<T>(UniTaskCompletionSource<T> taskCompletionSource)
            {
                yield return request.SendWebRequest();
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    taskCompletionSource.TrySetResult(default);
                    yield break;
                }

                taskCompletionSource.TrySetResult(GetData<T>());
            }

            public async UniTask<T> GetAsync<T>()
            {
                UniTaskCompletionSource<T> taskCompletionSource = new UniTaskCompletionSource<T>();
                request = UnityWebRequest.Get(url);
                request.timeout = 5;
                request.useHttpContinue = true;
                Behaviour.instance.StartCoroutine(Start(taskCompletionSource));
                return await taskCompletionSource.Task;
            }

            public async UniTask<T> PostAsync<T>(Dictionary<string, string> headers, object data)
            {
                UniTaskCompletionSource<T> taskCompletionSource = new UniTaskCompletionSource<T>();
                string str = JsonConvert.SerializeObject(data);
                request = UnityWebRequest.PostWwwForm(url, str);
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

                Behaviour.instance.StartCoroutine(Start(taskCompletionSource));
                return await taskCompletionSource.Task;
            }

            public async UniTask<string> Head(string headName)
            {
                request = UnityWebRequest.Head(url);
                request.timeout = 5;
                request.useHttpContinue = true;
                //通过UnityWebRequest获取head
                await request.SendWebRequest().ToUniTask();
                string result = request.GetResponseHeader(headName);
                request.Dispose();
                return result;
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