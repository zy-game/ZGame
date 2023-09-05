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

        internal static IWebRequestExecuteHandle<T> Create(string url, object data, Dictionary<string, object> header, NetworkRequestMethod method)
        {
            InternalWebRequestExecuteHandle<T> internalWebRequestExecuteHandle = Engine.Class.Loader<InternalWebRequestExecuteHandle<T>>();
            internalWebRequestExecuteHandle.url = url;
            internalWebRequestExecuteHandle.name = Path.GetFileName(url);
            internalWebRequestExecuteHandle.upload = data;
            internalWebRequestExecuteHandle.header = header;
            internalWebRequestExecuteHandle.method = method;
            return internalWebRequestExecuteHandle;
        }

        class InternalWebRequestExecuteHandle<T> : AbstractExecuteHandle, IExecuteHandle<IWebRequestExecuteHandle<T>>, IWebRequestExecuteHandle<T>
        {
            public T result => (T)_data;
            public string url { get; set; }
            public string name { get; set; }
            public float progress { get; set; }
            public NetworkRequestMethod method { get; set; }

            public object _data;
            public object upload;
            public Dictionary<string, object> header;

            public override void Release()
            {
                method = NetworkRequestMethod.NONE;
                url = String.Empty;
                _data = default;
                progress = 0;
                base.Release();
            }

            protected override IEnumerator ExecuteCoroutine()
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