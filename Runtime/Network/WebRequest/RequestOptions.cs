using System;
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
}