using System;
using System.IO;
using System.Linq;

namespace ZEngine.VFS
{
    /// <summary>
    /// 文件读取句柄
    /// </summary>
    public interface IReadFileExecuteHandle : IExecuteHandle<IReadFileExecuteHandle>
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

    class GameReadFileExecuteHandle : IReadFileExecuteHandle
    {
        public string name { get; set; }
        public long time { get; set; }
        public byte[] bytes { get; set; }
        public VersionOptions version { get; set; }
        public ExecuteStatus status { get; set; }

        public void Release()
        {
            version = null;
            status = ExecuteStatus.None;
            bytes = Array.Empty<byte>();
            time = 0;
            name = String.Empty;
            GC.SuppressFinalize(this);
        }

        public bool EnsureExecuteSuccessfuly()
        {
            return status == ExecuteStatus.Success;
        }

        public void Execute(params object[] args)
        {
            name = args[0].ToString();
            VFSData[] vfsDatas = VFSManager.instance.GetFileData(name);
            if (vfsDatas is null || vfsDatas.Length is 0)
            {
                status = ExecuteStatus.Failed;
                return;
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

            status = ExecuteStatus.Success;
        }
    }
}