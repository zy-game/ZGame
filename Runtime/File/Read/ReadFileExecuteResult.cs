using System;

namespace ZEngine.VFS
{
    public sealed class ReadFileExecuteResult : IReference
    {
        public string name;
        public long time;
        public byte[] bytes;
        public VersionOptions version;

        public static ReadFileExecuteResult Create(string name, long time, byte[] bytes, VersionOptions version)
        {
            ReadFileExecuteResult readFileExecuteResult = Engine.Class.Loader<ReadFileExecuteResult>();
            readFileExecuteResult.name = name;
            readFileExecuteResult.time = time;
            readFileExecuteResult.bytes = bytes;
            readFileExecuteResult.version = version;
            return readFileExecuteResult;
        }

        public void Release()
        {
            name = String.Empty;
            time = 0;
            bytes = Array.Empty<byte>();
            version = VersionOptions.None;
        }
    }
}