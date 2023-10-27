using System;

namespace ZEngine.VFS
{
    public interface IWriteFileResult : IDisposable
    {
        string name { get; }
        byte[] bytes { get; }
        int version { get; }

        internal static IWriteFileResult Create(string name, byte[] bytes, int version)
        {
            InternalVfsWriteFileResult internalVfsWriteFile = Activator.CreateInstance<InternalVfsWriteFileResult>();
            internalVfsWriteFile.name = name;
            internalVfsWriteFile.bytes = bytes;
            internalVfsWriteFile.version = version;
            return internalVfsWriteFile;
        }

        class InternalVfsWriteFileResult : IWriteFileResult
        {
            public string name { get; set; }
            public byte[] bytes { get; set; }
            public int version { get; set; }

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