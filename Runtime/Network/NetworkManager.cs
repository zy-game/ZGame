using System;
using System.Collections;
using System.Collections.Generic;

namespace ZEngine.Network
{
    class RequestOptions : IReference
    {
        public string url { get; private set; }
        public object data { get; private set; }
        public NetworkRequestMethod method { get; private set; }
        public Dictionary<string, object> header { get; private set; }

        public void Release()
        {
            url = String.Empty;
            data = null;
            method = NetworkRequestMethod.NONE;
            header = null;
        }

        public static RequestOptions Create(string url, object data, Dictionary<string, object> header, NetworkRequestMethod method)
        {
            RequestOptions requestOptions = Engine.Class.Loader<RequestOptions>();
            requestOptions.url = url;
            requestOptions.header = header;
            requestOptions.data = data;
            requestOptions.method = method;
            return requestOptions;
        }
    }

    public interface INetworkMultiDownloadExecuteHandle : IExecuteHandle<INetworkMultiDownloadExecuteHandle>
    {
    }

    class NetworkMultiDownloadExecuteHandle : INetworkMultiDownloadExecuteHandle
    {
        public void Release()
        {
            throw new NotImplementedException();
        }

        public Status status { get; }

        public void Execute(params object[] paramsList)
        {
            throw new NotImplementedException();
        }

        public void Subscribe(ISubscribeHandle subscribe)
        {
            throw new NotImplementedException();
        }

        public IEnumerator ExecuteComplete()
        {
            throw new NotImplementedException();
        }
    }

    public class MultiDownloadOptions
    {
        public void AddUrl(string url)
        {
            
        }
    }

    public class NetworkManager : Single<NetworkManager>
    {
        public INetworkRequestExecuteHandle<T> Request<T>(string url, object data, NetworkRequestMethod method, Dictionary<string, object> header = default)
        {
            DefaultNetworkRequestExecuteHandle<T> defaultNetworkRequestExecuteHandle = Engine.Class.Loader<DefaultNetworkRequestExecuteHandle<T>>();
            defaultNetworkRequestExecuteHandle.Execute(RequestOptions.Create(url, data, header, method));
            return defaultNetworkRequestExecuteHandle;
        }

        public INetworkMultiDownloadExecuteHandle MultiDownload(MultiDownloadOptions options)
        {
            NetworkMultiDownloadExecuteHandle networkMultiDownloadExecuteHandle = new NetworkMultiDownloadExecuteHandle();
            networkMultiDownloadExecuteHandle.Execute(options);
            return networkMultiDownloadExecuteHandle;
        }
    }
}