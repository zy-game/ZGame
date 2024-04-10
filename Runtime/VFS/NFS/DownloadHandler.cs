using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using Cysharp.Threading.Tasks;
using Downloader;
using UnityEngine.Networking;
using ZGame.Networking;
using DownloadProgressChangedEventArgs = Downloader.DownloadProgressChangedEventArgs;

namespace ZGame.VFS
{
    public class DownloadHandler : IReferenceObject, IProgress<float>
    {
        private DownloadConfiguration downloadOpt = new DownloadConfiguration()
        {
            // usually, hosts support max to 8000 bytes, default value is 8000
            BufferBlockSize = 10240,
            // file parts to download, the default value is 1
            ChunkCount = 8,
            // download speed limited to 2MB/s, default values is zero or unlimited
            MaximumBytesPerSecond = 1024 * 1024 * 2,
            // the maximum number of times to fail
            MaxTryAgainOnFailover = 5,
            // release memory buffer after each 50 MB
            MaximumMemoryBufferBytes = 1024 * 1024 * 50,
            // download parts of the file as parallel or not. The default value is false
            ParallelDownload = true,
            // number of parallel downloads. The default value is the same as the chunk count
            ParallelCount = 4,
            // timeout (millisecond) per stream block reader, default values is 1000
            Timeout = 1000,
            // set true if you want to download just a specific range of bytes of a large file
            RangeDownload = false,
            // floor offset of download range of a large file
            RangeLow = 0,
            // ceiling offset of download range of a large file
            RangeHigh = 0,
            // clear package chunks data when download completed with failure, default value is false
            ClearPackageOnCompletionWithFailure = true,
            // minimum size of chunking to download a file in multiple parts, the default value is 512
            MinimumSizeOfChunking = 1024,
            // Before starting the download, reserve the storage space of the file as file size, the default value is false
            ReserveStorageSpaceBeforeStartingDownload = true,
            // config and customize request headers
            RequestConfiguration = new()
            {
                Accept = "*/*",
                Headers = new WebHeaderCollection(), // { your custom headers }
                KeepAlive = true, // default value is false
                ProtocolVersion = HttpVersion.Version11, // default value is HTTP 1.1
                UseDefaultCredentials = false,
                // your custom user agent or your_app_name/app_version.
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)",
            }
        };

        private Stream stream;
        private Action<DownloadHandler> stateChangeCallback;
        public string name { get; private set; }
        public string url { get; private set; }
        public uint version { get; private set; }
        public float progress { get; private set; }
        public byte[] bytes { get; private set; }
        public Status status { get; private set; }

        // public async UniTask<Status> StartAsync()
        // {
        //     var downloader = new DownloadService(downloadOpt);
        //     downloader.DownloadProgressChanged += DownloaderOnDownloadProgressChanged;
        //     GameFrameworkEntry.Logger.Log("Start Download:" + url);
        //     status = Status.Runing;
        //     stream = await downloader.DownloadFileTaskAsync(url);
        //     status = Status.Fail;
        //     if (downloader.Status is DownloadStatus.Completed)
        //     {
        //         using (MemoryStream ms = new MemoryStream())
        //         {
        //             await stream.CopyToAsync(ms);
        //             bytes = ms.ToArray();
        //         }
        //
        //         status = Status.Success;
        //     }
        //
        //     GameFrameworkEntry.Logger.Log($"Download Complete: {url}");
        //     return status;
        // }
        public void Report(float value)
        {
            progress = value;
            stateChangeCallback?.Invoke(this);
        }

        public async UniTask<Status> StartAsync()
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeaderWithCors();
                status = Status.Runing;
                await request.SendWebRequest().ToUniTask(this);
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    GameFrameworkEntry.Logger.LogError($"Error: {request.error}");
                    status = Status.Fail;
                }
                else
                {
                    bytes = request.downloadHandler.data;
                    status = Status.Success;
                }
            }

            GameFrameworkEntry.Logger.Log($"Download Complete: {url}");
            return status;
        }

        private void DownloaderOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            progress = (float)(e.ProgressPercentage / 100f);
            stateChangeCallback?.Invoke(this);
        }


        public void Release()
        {
            stream?.Dispose();
            stateChangeCallback = null;
            url = null;
            version = 0;
            progress = 0;
            bytes = null;
            downloadOpt = null;
        }

        public static DownloadHandler Create(string url, uint version, Action<DownloadHandler> startCallback)
        {
            DownloadHandler downloadHandler = GameFrameworkFactory.Spawner<DownloadHandler>();
            downloadHandler.url = url;
            downloadHandler.name = Path.GetFileName(url);
            downloadHandler.version = version;
            downloadHandler.stateChangeCallback = startCallback;
            return downloadHandler;
        }
    }
}