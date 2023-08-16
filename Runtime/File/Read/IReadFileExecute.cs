using System;
using System.IO;
using System.Linq;

namespace ZEngine.VFS
{
    public interface IReadFileExecuteResult : IReference
    {
        string name { get; }
        long time { get; }
        byte[] bytes { get; }
        VersionOptions version { get; }
    }

    class ReadFileExecuteResult : IReadFileExecuteResult
    {
        public void Release()
        {
            version = null;
            name = String.Empty;
            bytes = Array.Empty<byte>();
            time = 0;
            GC.SuppressFinalize(this);
        }

        public string name { get; set; }
        public long time { get; set; }
        public byte[] bytes { get; set; }
        public VersionOptions version { get; set; }
    }

    class DefaultReadFileExecute : IExecute
    {
        public ReadFileExecuteResult result { get; set; }

        public void Release()
        {
            result = null;
            GC.SuppressFinalize(this);
        }


        public void Execute(params object[] args)
        {
            result = Engine.Class.Loader<ReadFileExecuteResult>();
            result.name = args[0].ToString();
            result.version = args[1] is null ? VersionOptions.None : (VersionOptions)args[1];
            VFSData[] vfsDatas = VFSManager.instance.GetFileData(result.name);
            if (vfsDatas is null || vfsDatas.Length is 0 || vfsDatas[0].version != result.version)
            {
                return;
            }

            result.bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            result.version = vfsDatas[0].version;
            long time = vfsDatas[0].time;
            int offset = 0;
            for (int i = 0; i < vfsDatas.Length; i++)
            {
                vfsDatas[i].Read(result.bytes, offset, vfsDatas[i].fileLenght);
                offset += vfsDatas[i].fileLenght;
            }
        }
    }
}