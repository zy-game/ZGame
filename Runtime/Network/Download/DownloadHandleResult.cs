using System;

namespace ZEngine.Network
{
    public class DownloadHandleResult : IReference
    {
        public string url;
        public long time;
        public long useTime;
        public byte[] bytes;
        public Status status;

        internal static DownloadHandleResult Create(string url, long startTime, long usetime, byte[] bytes, Status status)
        {
            DownloadHandleResult downloadHandleResult = Engine.Class.Loader<DownloadHandleResult>();
            downloadHandleResult.url = url;
            downloadHandleResult.bytes = bytes;
            downloadHandleResult.status = status;
            downloadHandleResult.time = startTime;
            downloadHandleResult.useTime = usetime;
            return downloadHandleResult;
        }

        public void Release()
        {
            url = String.Empty;
            time = 0;
            useTime = 0;
            bytes = Array.Empty<byte>();
            status = Status.None;
        }
    }
}