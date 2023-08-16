using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace ZEngine.Network
{
    public interface IWebRequestExecuteHandle<T> : IExecuteHandle<IWebRequestExecuteHandle<T>>
    {
        T result { get; }
        string url { get; }
        string name { get; }
        float progress { get; }
        NetworkRequestMethod method { get; }

        void OnPorgressChange(ISubscribeHandle<float> subscribe);
    }

    class DefaultWebRequestExecuteHandle<T> : ExecuteHandle, IExecuteHandle<IWebRequestExecuteHandle<T>>, IWebRequestExecuteHandle<T>
    {
        public T result => (T)_data;
        public string url { get; set; }
        public string name { get; set; }
        public float progress { get; set; }
        public NetworkRequestMethod method { get; set; }

        private object _data;
        private object upload;
        private Dictionary<string, object> header;

        public override void Release()
        {
            method = NetworkRequestMethod.NONE;
            url = String.Empty;
            _data = default;
            progress = 0;
            base.Release();
        }


        public override void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            RequestOptions requestOptions = paramsList.TryGetValue<RequestOptions>(0, default);
            if (requestOptions is null)
            {
                status = Status.Failed;
                OnComplete();
                return;
            }

            url = requestOptions.url;
            name = Path.GetFileName(url);
            upload = requestOptions.data;
            header = requestOptions.header;
            method = requestOptions.method;
            OnStart().StartCoroutine();
        }

        IEnumerator OnStart()
        {
            UnityWebRequest request = method switch
            {
                NetworkRequestMethod.GET => UnityWebRequest.Get(url),
                NetworkRequestMethod.PUT => UnityWebRequest.Put(url, JsonConvert.SerializeObject(upload)),
                NetworkRequestMethod.POST => UnityWebRequest.Post(url, JsonConvert.SerializeObject(upload)),
                NetworkRequestMethod.DELETE => UnityWebRequest.Delete(url),
                _ => default
            };
            if (request is null)
            {
                Engine.Console.Error(new NotSupportedException(method.ToString()));
                OnComplete();
                status = Status.Failed;
                yield break;
            }

            if (header is not null && header.Count > 0)
            {
                foreach (var VARIABLE in header)
                {
                    request.SetRequestHeader(VARIABLE.Key, VARIABLE.Value.ToString());
                }
            }

            request.SendWebRequest();
            while (request.isDone is false)
            {
                OnProgress(request.downloadProgress);
                yield return new WaitForSeconds(0.01f);
            }

            Engine.Console.Log(request.url, request.result);
            if (request.result is not UnityWebRequest.Result.Success)
            {
                Engine.Console.Error(request.error);
                OnComplete();
                status = Status.Failed;
                yield break;
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

            status = Status.Success;
            OnComplete();
        }
    }
}