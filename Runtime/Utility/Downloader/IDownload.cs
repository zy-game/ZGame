﻿using System.ComponentModel;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Downloader
{
    public interface IDownload : IDisposable
    {
        public string Url { get; }
        public string Folder { get; }
        public string Filename { get; }
        public long DownloadedFileSize { get; }
        public long TotalFileSize { get; }
        public DownloadPackage Package { get; }
        public DownloadStatus Status { get; }

        public event EventHandler<DownloadProgressChangedEventArgs> ChunkDownloadProgressChanged;
        public event EventHandler<AsyncCompletedEventArgs> DownloadFileCompleted;
        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged;
        public event EventHandler<DownloadStartedEventArgs> DownloadStarted;

        public Task<Stream> StartAsync(CancellationToken cancellationToken = default);
        public void Stop();
        public void Pause();
        public void Resume();
    }
}
