using System;
using System.IO;
using System.Linq;

namespace ZEngine.VFS
{
    class DefaultReadFileExecute : IExecute<ReadFileExecuteResult>
    {
        public VersionOptions version { get; set; }
        public ReadFileExecuteResult result { get; set; } = new ReadFileExecuteResult();

        public void Release()
        {
            version = null;
            result = default;
            GC.SuppressFinalize(this);
        }


        public void Execute(params object[] args)
        {
            string name = args[0].ToString();
            VFSData[] vfsDatas = VFSManager.instance.GetFileData(name);
            VersionOptions version = args[1] is null ? VersionOptions.None : (VersionOptions)args[1];
            if (vfsDatas is null || vfsDatas.Length is 0 || vfsDatas[0].version != version)
            {
                return;
            }

            byte[] bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            version = vfsDatas[0].version;
            long time = vfsDatas[0].time;
            int offset = 0;
            for (int i = 0; i < vfsDatas.Length; i++)
            {
                vfsDatas[i].Read(bytes, offset, vfsDatas[i].fileLenght);
                offset += vfsDatas[i].fileLenght;
            }

            result = ReadFileExecuteResult.Create(name, time, bytes, version);
        }
    }
}