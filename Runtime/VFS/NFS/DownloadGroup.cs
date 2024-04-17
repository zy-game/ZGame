using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace ZGame.VFS
{
    public class DownloadGroup : IReference
    {
        private Action<DownloadGroup> groupStateChangeCallback;
        private List<DownloadHandler> downloadHandlers = new();
        public float progress { get; private set; }
        public Status status { get; private set; }

        /// <summary>
        /// 下载器对象
        /// </summary>
        public DownloadHandler[] items => downloadHandlers.ToArray();

        public void Add(string url, uint version, Action<DownloadHandler> itemStateChangeCallback)
        {
            downloadHandlers.Add(DownloadHandler.Create(url, version, itemStateChangeCallback));
        }

        private async void OnDownloaderStateChangeed(DownloadHandler e)
        {
            progress = downloadHandlers.Sum(x => x.progress) / downloadHandlers.Count;
            groupStateChangeCallback?.Invoke(this);
        }

        public async UniTask<Status> StartAsync()
        {
            UniTask<Status>[] task = new UniTask<Status>[downloadHandlers.Count];
            for (int i = 0; i < downloadHandlers.Count; i++)
            {
                task[i] = downloadHandlers[i].StartAsync();
            }

            status = Status.Runing;
            groupStateChangeCallback?.Invoke(this);
            await UniTask.WhenAll(task);
            CoreAPI.Logger.Log("Download Complete");
            status = Status.Fail;
            if (downloadHandlers.All(x => x.status == Status.Success))
            {
                status = Status.Success;
            }

            return status;
        }

        public void Release()
        {
            downloadHandlers.ForEach(RefPooled.Release);
            downloadHandlers.Clear();
            groupStateChangeCallback = null;
            progress = 0;
        }

        public static DownloadGroup Create(Action<DownloadGroup> groupStateChangeCallback)
        {
            DownloadGroup downloadGroup = RefPooled.Spawner<DownloadGroup>();
            downloadGroup.groupStateChangeCallback = groupStateChangeCallback;
            return downloadGroup;
        }
    }
}