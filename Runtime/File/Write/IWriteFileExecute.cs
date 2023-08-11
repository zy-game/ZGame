using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace ZEngine.VFS
{
    /// <summary>
    /// 文件写入句柄
    /// </summary>
    public interface IWriteFileExecute : IExecute<IWriteFileExecute>
    {
        /// <summary>
        /// 文件名
        /// </summary>
        string name { get; }

        /// <summary>
        /// 文件数据
        /// </summary>
        byte[] bytes { get; }

        /// <summary>
        /// 文件版本
        /// </summary>
        VersionOptions version { get; }
    }

    class DefaultWriteFileExecute : IWriteFileExecute
    {
        public string name { get; set; }
        public byte[] bytes { get; set; }
        public VersionOptions version { get; set; }

        public void Release()
        {
            version = VersionOptions.None;
            bytes = Array.Empty<byte>();
            name = String.Empty;
        }

        public IWriteFileExecute Execute(params object[] args)
        {
            name = (string)args[0];
            bytes = (byte[])args[1];
            version = (VersionOptions)args[2];
            VFSData[] vfsDataList = VFSManager.instance.GetVFSData(bytes.Length);
            int offset = 0;
            int index = 0;
            foreach (var VARIABLE in vfsDataList)
            {
                int length = bytes.Length - offset > VARIABLE.length ? VARIABLE.length : bytes.Length - offset;
                VARIABLE.Write(bytes, offset, length, version, index);
                offset += VARIABLE.length;
                VARIABLE.name = name;
                index++;
            }

            VFSManager.instance.SaveVFSData();
            return this;
        }
    }
}