using System;

namespace ZGame.FileSystem
{
    public interface IFileDataWriter : IDisposable
    {
        string name { get; }
        byte[] bytes { get; }
        uint version { get; }

        internal static IFileDataWriter Create(string name, byte[] bytes, uint version)
        {
            FileDataWritePipeline fileWriter = Activator.CreateInstance<FileDataWritePipeline>();
            fileWriter.name = name;
            fileWriter.bytes = bytes;
            fileWriter.version = version;
            return fileWriter;
        }

        class FileDataWritePipeline : IFileDataWriter
        {
            public string name { get; set; }
            public byte[] bytes { get; set; }
            public uint version { get; set; }

            public void Dispose()
            {
                name = String.Empty;
                bytes = Array.Empty<byte>();
                version = 0;
                GC.SuppressFinalize(this);
            }
        }
    }
}