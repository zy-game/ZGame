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
            VersionOptions version = (VersionOptions)args[2];
            VFSData vfsData = default;
            //todo 根据vfs布局写入文件
            if (VFSOptions.instance.layout == VFSLayout.ReadWritePriority || VFSOptions.instance.vfsState == Switch.Off)
            {
                //todo 如果单个文件小于等于一个VFS数据块，则写入VFS，如果大于单个VFS数据块，则写入单独的VFS中
                vfsData = VFSManager.instance.GetVFSData(Mathf.Max(VFSOptions.instance.sgementLenght, bytes.Length));
                if (vfsData is null)
                {
                    return this;
                }

                vfsData.Write(bytes, 0, bytes.Length, version);
                return this;
            }

            int count = bytes.Length.SpiltCount(VFSOptions.instance.sgementLenght);
            int offset = 0;
            int length = 0;
            for (int i = 0; i < count; i++)
            {
                //todo 不用理会文件是否连续
                vfsData = VFSManager.instance.GetVFSData();
                if (vfsData is null)
                {
                    return this;
                }

                offset += i * length;
                length = Mathf.Min(VFSOptions.instance.sgementLenght, bytes.Length - offset);
                vfsData.Write(bytes, offset, length, version);
            }

            VFSManager.instance.SaveVFSData();
            return this;
        }
    }
}