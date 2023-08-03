using System;
using System.IO;
using System.Linq;

namespace ZEngine.VFS
{
    /// <summary>
    /// 文件读取句柄
    /// </summary>
    public interface IReadFileExecute : IExecute<IReadFileExecute>
    {
        /// <summary>
        /// 文件名
        /// </summary>
        string name { get; }

        /// <summary>
        /// 最后访问时间
        /// </summary>
        long time { get; }

        /// <summary>
        /// 文件数据
        /// </summary>
        byte[] bytes { get; }

        /// <summary>
        /// 文件版本
        /// </summary>
        VersionOptions version { get; }
    }

    class DefaultReadFileExecuteHandleHandle : IReadFileExecute
    {
        public string name { get; set; }
        public long time { get; set; }
        public byte[] bytes { get; set; }
        public VersionOptions version { get; set; }

        public void Release()
        {
            version = null;
            bytes = Array.Empty<byte>();
            time = 0;
            name = String.Empty;
            GC.SuppressFinalize(this);
        }


        public IReadFileExecute Execute(params object[] args)
        {
            name = args[0].ToString();
            VFSData[] vfsDatas = VFSManager.instance.GetFileData(name);
            if (vfsDatas is null || vfsDatas.Length is 0)
            {
                return this;
            }

            bytes = new byte[vfsDatas.Sum(x => x.fileLenght)];
            version = vfsDatas[0].version;
            time = vfsDatas[0].time;
            int offset = 0;
            for (int i = 0; i < vfsDatas.Length; i++)
            {
                vfsDatas[i].Read(bytes, offset, vfsDatas[i].fileLenght);
                offset += vfsDatas[i].fileLenght;
            }

            return this;
        }
    }
}