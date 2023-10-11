using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace ZEngine.Network
{
    public interface IWebRequestResult<T> : IDisposable
    {
        string url { get; }
        string name { get; }
        float progress { get; }
        T result { get; }

        internal static IWebRequestResult<T> CreateRequest(string url, Dictionary<string, object> header, UniTaskCompletionSource<IWebRequestResult<T>> uniTaskCompletionSource)
        {
            InternalWebRequestScheduleHandle<T> internalWebRequestScheduleHandle = Activator.CreateInstance<InternalWebRequestScheduleHandle<T>>();
            internalWebRequestScheduleHandle.url = url;
            internalWebRequestScheduleHandle.name = Path.GetFileName(url);
            internalWebRequestScheduleHandle.header = header;
            internalWebRequestScheduleHandle.request = UnityWebRequest.Get(url);
            internalWebRequestScheduleHandle.Execute(uniTaskCompletionSource);
            return internalWebRequestScheduleHandle;
        }

        internal static IWebRequestResult<T> CreatePost(string url, object data, Dictionary<string, object> header, UniTaskCompletionSource<IWebRequestResult<T>> uniTaskCompletionSource)
        {
            InternalWebRequestScheduleHandle<T> internalWebRequestScheduleHandle = Activator.CreateInstance<InternalWebRequestScheduleHandle<T>>();
            internalWebRequestScheduleHandle.url = url;
            internalWebRequestScheduleHandle.name = Path.GetFileName(url);
            internalWebRequestScheduleHandle.upload = data;
            internalWebRequestScheduleHandle.header = header;
            internalWebRequestScheduleHandle.request = UnityWebRequest.PostWwwForm(url, JsonConvert.SerializeObject(data));
            internalWebRequestScheduleHandle.Execute(uniTaskCompletionSource);
            return internalWebRequestScheduleHandle;
        }

        class InternalWebRequestScheduleHandle<T> : IWebRequestResult<T>
        {
            public T result => (T)_data;
            public string url { get; set; }
            public string name { get; set; }
            public float progress { get; set; }
            public object _data;
            public object upload;
            public Dictionary<string, object> header;
            public UnityWebRequest request;


            public async void Execute(UniTaskCompletionSource<IWebRequestResult<T>> uniTaskCompletionSource)
            {
                if (request is null)
                {
                    Launche.Console.Error(new NullReferenceException("request"));
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                if (header is not null && header.Count > 0)
                {
                    foreach (var VARIABLE in header)
                    {
                        request.SetRequestHeader(VARIABLE.Key, VARIABLE.Value.ToString());
                    }
                }

                await request.SendWebRequest().ToUniTask();
                Launche.Console.Log(request.url, request.result);
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    Launche.Console.Error(request.error);
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

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

                uniTaskCompletionSource.TrySetResult(this);
            }

            public void Dispose()
            {
                url = String.Empty;
                _data = default;
                progress = 0;
                name = String.Empty;
                upload = default;
                header = null;
                GC.SuppressFinalize(this);
            }
        }
    }
}