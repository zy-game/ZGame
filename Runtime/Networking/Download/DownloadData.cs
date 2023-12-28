using System;

namespace ZGame.Networking
{
    public class DownloadData : IDisposable
    {
        public string name;
        public string url;
        public bool isDone;
        public byte[] bytes;
        public uint crc;

        public void Dispose()
        {
            // TODO 在此释放托管资源
        }
    }
}