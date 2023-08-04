using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace ZEngine.Network
{
    public enum NetworkRequestMethod : byte
    {
        NONE,
        POST,
        GET,
        DELETE,
        PUT,
    }

    public interface INetworkRequestExecuteHandle<T> : IExecuteHandle<T>
    {
        T result { get; }
        string url { get; }
        string name { get; }
        float progress { get; }
        NetworkRequestMethod method { get; }

        void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe);
    }

    class DefaultNetworkRequestExecuteHandle<T> : INetworkRequestExecuteHandle<T>
    {
        public T result => (T)_data;
        public string url { get; set; }
        public string name { get; set; }
        public Status status { get; set; }
        public float progress { get; set; }
        public NetworkRequestMethod method { get; set; }

        private object _data;
        private ISubscribeExecuteHandle<float> progressListener;
        private List<ISubscribeExecuteHandle> completeSubscribe = new List<ISubscribeExecuteHandle>();

        public void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe)
        {
            progressListener = subscribe;
        }

        public IEnumerator Complete()
        {
            return new WaitUntil(() => status == Status.Failed || status == Status.Success);
        }

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            completeSubscribe.Add(subscribe);
        }

        public void Release()
        {
            method = NetworkRequestMethod.NONE;
            progressListener = null;
            status = Status.None;
            url = String.Empty;
            _data = default;
            progress = 0;
        }


        public IEnumerator Execute(params object[] paramsList)
        {
            status = Status.Execute;
            url = paramsList[0].ToString();
            name = Path.GetFileName(url);
            object upload = paramsList[1];
            Dictionary<string, object> header = paramsList[2] is null ? default : (Dictionary<string, object>)paramsList[2];
            method = (NetworkRequestMethod)paramsList[3];
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
                completeSubscribe.ForEach(x => x.Execute(this));
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
                progressListener?.Execute(request.downloadProgress);
                yield return new WaitForSeconds(0.01f);
            }

            if (request.isDone is false || request.result is not UnityWebRequest.Result.Success)
            {
                Engine.Console.Error(request.error);
                completeSubscribe.ForEach(x => x.Execute(this));
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

            completeSubscribe.ForEach(x => x.Execute(this));
            status = Status.Success;
        }
    }
}