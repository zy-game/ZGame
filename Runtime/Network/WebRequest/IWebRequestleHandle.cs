using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace ZEngine.Network
{
    public interface IWebRequestleHandle<T> : IScheduleHandle<T>
    {
        string url { get; }
        string name { get; }
        float progress { get; }

        internal static IWebRequestleHandle<T> CreateRequest(string url, Dictionary<string, object> header)
        {
            InternalWebRequestScheduleHandle<T> internalWebRequestScheduleHandle = Activator.CreateInstance<InternalWebRequestScheduleHandle<T>>();
            internalWebRequestScheduleHandle.url = url;
            internalWebRequestScheduleHandle.name = Path.GetFileName(url);
            internalWebRequestScheduleHandle.header = header;
            internalWebRequestScheduleHandle.request = UnityWebRequest.Get(url);
            return internalWebRequestScheduleHandle;
        }

        internal static IWebRequestleHandle<T> CreatePost(string url, object data, Dictionary<string, object> header)
        {
            InternalWebRequestScheduleHandle<T> internalWebRequestScheduleHandle = Activator.CreateInstance<InternalWebRequestScheduleHandle<T>>();
            internalWebRequestScheduleHandle.url = url;
            internalWebRequestScheduleHandle.name = Path.GetFileName(url);
            internalWebRequestScheduleHandle.upload = data;
            internalWebRequestScheduleHandle.header = header;
            internalWebRequestScheduleHandle.request = UnityWebRequest.PostWwwForm(url, JsonConvert.SerializeObject(data));
            return internalWebRequestScheduleHandle;
        }

        class InternalWebRequestScheduleHandle<T> : IWebRequestleHandle<T>
        {
            public Status status { get; set; }
            public T result => (T)_data;

            public string url { get; set; }
            public string name { get; set; }
            public float progress { get; set; }
            public object _data;
            public object upload;
            public Dictionary<string, object> header;
            private ISubscriber subscriber;
            public UnityWebRequest request;


            public void Execute(params object[] args)
            {
                if (status is not Status.None)
                {
                    return;
                }

                status = Status.Execute;
                DOExecute().StartCoroutine(OnComplate);
            }

            private void OnComplate()
            {
                if (subscriber is not null)
                {
                    subscriber.Execute(this);
                }

            }

            public void Dispose()
            {
                url = String.Empty;
                _data = default;
                progress = 0;
                name = String.Empty;
                upload = default;
                header = null;
                subscriber?.Dispose();
                subscriber = null;
            }

            public void Subscribe(ISubscriber subscriber)
            {
                if (this.subscriber is null)
                {
                    this.subscriber = subscriber;
                    return;
                }

                this.subscriber.Merge(subscriber);
            }

            private IEnumerator DOExecute()
            {
                if (request is null)
                {
                    Engine.Console.Error(new NullReferenceException("request"));
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

                yield return request.SendWebRequest();
                Engine.Console.Log(request.url, request.result);
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    Engine.Console.Error(request.error);
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
            }
        }
    }
}