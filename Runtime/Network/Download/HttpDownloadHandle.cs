using System;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using ZEngine.VFS;

namespace ZEngine.Network
{
    public class HttpDownloadHandle : IDisposable
    {
        public string name;
        public string url;
        public int index;
        public Status status;
        public int version;

        public float progress => request == null ? 0 : request.downloadProgress;
        public byte[] bytes => request.downloadHandler.data;


        private long startTime;
        private TimeSpan useTime;
        private UnityWebRequest request;

        public void OnStart()
        {
            StartDownload().StartCoroutine();
        }

        private IEnumerator StartDownload()
        {
            startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string path = ZGame.GetLocalFilePath(name);
            request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            useTime = DateTime.Now - DateTimeOffset.FromUnixTimeMilliseconds(startTime);
            if (request.result is not UnityWebRequest.Result.Success)
            {
                ZGame.Console.Error(request.error);
                status = Status.Failed;
                yield break;
            }

            ZGame.Console.Log(request.url, version, request.result, "time:" + useTime.Seconds);
            ZGame.FileSystem.WriteFile(name, request.downloadHandler.data, version);
            status = Status.Success;
            ZGame.Console.Log($"write {name} complate");
        }

        public bool IsComplete()
        {
            return status == Status.Failed || status == Status.Success;
        }

        public void Dispose()
        {
            name = String.Empty;
            url = String.Empty;
            index = 0;
            status = Status.None;
            version = 0;
        }

        public static HttpDownloadHandle Create(string url, int index, int ver)
        {
            HttpDownloadHandle downloadHandle = Activator.CreateInstance<HttpDownloadHandle>();
            downloadHandle.url = url;
            downloadHandle.version = ver;
            downloadHandle.index = index;
            downloadHandle.status = Status.None;
            downloadHandle.name = Path.GetFileName(downloadHandle.url);
            return downloadHandle;
        }
    }
}