using System;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using ZEngine.VFS;

namespace ZEngine.Network
{
    public class DownloadHandle : IReference
    {
        public string name;
        public string url;
        public int index;
        public Status status;
        public VersionOptions version;

        public float progress => request.downloadProgress;
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
            status = Status.Execute;
            startTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            string path = Engine.Custom.GetLocalFilePath(name);
            request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();
            useTime = DateTime.Now - DateTimeOffset.FromUnixTimeMilliseconds(startTime);
            if (request.result is not UnityWebRequest.Result.Success)
            {
                Engine.Console.Error(request.error);
                status = Status.Failed;
                yield break;
            }

            Engine.Console.Log(request.url, version, request.result, "time:" + useTime.Seconds);
            IWriteFileExecuteHandle writeFileExecuteHandle = Engine.FileSystem.WriteFileAsync(name, request.downloadHandler.data, version);
            writeFileExecuteHandle.Subscribe(() => { status = Status.Success; });
        }

        public bool IsComplete()
        {
            return status == Status.Failed || status == Status.Success;
        }

        public void Release()
        {
            name = String.Empty;
            url = String.Empty;
            index = 0;
            status = Status.None;
            version = VersionOptions.None;
        }

        public static DownloadHandle Create(string url, int index, VersionOptions ver)
        {
            DownloadHandle downloadHandle = Engine.Class.Loader<DownloadHandle>();
            downloadHandle.url = url;
            downloadHandle.version = ver;
            downloadHandle.index = index;
            downloadHandle.status = Status.None;
            downloadHandle.name = Path.GetFileName(downloadHandle.url);
            return downloadHandle;
        }
    }
}