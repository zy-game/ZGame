using System;
using System.Collections;
using System.Collections.Generic;

namespace ZEngine.Network
{
    public class NetworkManager : Single<NetworkManager>
    {
        public override void Dispose()
        {
            base.Dispose();
            Engine.Console.Log("关闭所有网络链接");
        }

        public IWebRequestExecuteHandle<T> Request<T>(string url, object data, NetworkRequestMethod method, Dictionary<string, object> header = default)
        {
            DefaultWebRequestExecuteHandle<T> defaultWebRequestExecuteHandle = Engine.Class.Loader<DefaultWebRequestExecuteHandle<T>>();
            defaultWebRequestExecuteHandle.Execute(RequestOptions.Create(url, data, header, method));
            return defaultWebRequestExecuteHandle;
        }

        public IDownloadExecuteHandle Download(params DownloadOptions[] urlList)
        {
            DefaultDownloadExecuteHandle downloadExecuteHandle = new DefaultDownloadExecuteHandle();
            downloadExecuteHandle.Execute(urlList);
            return downloadExecuteHandle;
        }
    }
}