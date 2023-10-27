using System;
using System.IO;
using System.Linq;

namespace ZGame.FileSystem
{
    public interface IReadFileResult : IDisposable
    {
        string name { get; }
        long time { get; }
        byte[] bytes { get; }
        int version { get; }

        internal static IReadFileResult Create(string name, byte[] bytes, long time, int version)
        {
            InternalVfsReadResult internalVfsReadResult = Activator.CreateInstance<InternalVfsReadResult>();
            internalVfsReadResult.name = name;
            internalVfsReadResult.version = version;
            internalVfsReadResult.bytes = bytes;
            internalVfsReadResult.time = time;
            return internalVfsReadResult;
        }

        class InternalVfsReadResult : IReadFileResult
        {
            public string name { get; set; }
            public long time { get; set; }
            public byte[] bytes { get; set; }
            public int version { get; set; }

            public void Dispose()
            {
                version = 0;
                name = String.Empty;
                bytes = Array.Empty<byte>();
                time = 0;
                GC.SuppressFinalize(this);
            }
        }
    }
}