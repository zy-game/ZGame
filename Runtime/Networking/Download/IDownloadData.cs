using System;

namespace ZGame.Networking
{
    public interface IDownloadData : IEntity
    {
        string url { get; set; }
        byte[] bytes { get; set; }
        bool isDone { get; set; }
        float progress { get; set; }

        public static IDownloadData Create(string url, bool state, byte[] bytes)
        {
            return new DefaultDownloadData()
            {
                url = url,
                bytes = bytes,
                isDone = state
            };
        }

        class DefaultDownloadData : IDownloadData
        {
            public void Dispose()
            {
                guid = String.Empty;
                url = String.Empty;
                bytes = Array.Empty<byte>();
                GC.SuppressFinalize(this);
            }

            public float progress { get; set; }
            public bool isDone { get; set; }
            public string guid { get; set; } = ID.New();
            public string url { get; set; }
            public byte[] bytes { get; set; }
        }
    }
}