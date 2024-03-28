using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.VFS
{
    /// <summary>
    /// 文件数据块
    /// </summary>
    public class VFSChunk
    {
        /// <summary>
        /// 起始偏移
        /// </summary>
        public int offset;

        /// <summary>
        /// 数据长度
        /// </summary>
        public int length;

        /// <summary>
        /// 所在vfs文件
        /// </summary>
        public string vfs;

        /// <summary>
        /// 实际数据占用长度
        /// </summary>
        public int useLenght;

        /// <summary>
        /// 文件序号
        /// </summary>
        public int sort;

        /// <summary>
        /// 文件名
        /// </summary>
        public string name;

        /// <summary>
        /// 文件最后访问时间
        /// </summary>
        public long time;

        /// <summary>
        /// 文件版本
        /// </summary>
        public uint version;

        /// <summary>
        /// 是否在使用
        /// </summary>
        public bool use;

        public VFSChunk(string vfs, int length, int offset)
        {
            this.offset = offset;
            this.vfs = vfs;
            this.length = length;
        }

        public void Use(string name, int useLenght, int sort, uint version)
        {
            this.name = name;
            this.useLenght = useLenght;
            this.sort = sort;
            this.version = version;
            this.use = true;
        }

        public void Free()
        {
            sort = 0;
            use = false;
            useLenght = 0;
            name = String.Empty;
            version = 0;
        }
    }
}