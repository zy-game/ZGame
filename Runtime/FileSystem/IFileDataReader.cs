using System;
using System.IO;
using System.Linq;

namespace ZGame.FileSystem
{
    public interface IFileDataReader : IDisposable
    {
        string name { get; }
        long time { get; }
        byte[] bytes { get; }
        uint version { get; }

        internal static IFileDataReader Create(string name, byte[] bytes, long time, uint version)
        {
            FileReaderPipeline fileReaderPipeline = Activator.CreateInstance<FileReaderPipeline>();
            fileReaderPipeline.name = name;
            fileReaderPipeline.version = version;
            fileReaderPipeline.bytes = bytes;
            fileReaderPipeline.time = time;
            return fileReaderPipeline;
        }

        class FileReaderPipeline : IFileDataReader
        {
            public string name { get; set; }
            public long time { get; set; }
            public byte[] bytes { get; set; }
            public uint version { get; set; }

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