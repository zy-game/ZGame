using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.FileSystem
{
    public class VFSData
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
        /// 文件长度
        /// </summary>
        public int fileLenght;

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
        public int version;

        /// <summary>
        /// 是否在使用
        /// </summary>
        public bool use;

        public VFSData(string vfs, int length, int offset)
        {
            this.offset = offset;
            this.vfs = vfs;
            this.length = length;
        }

        public void Use(string name, int useLenght, int sort, long time, int version)
        {
            this.name = name;
            this.fileLenght = useLenght;
            this.sort = sort;
            this.time = time;
            this.version = version;
            this.use = use;
        }

        public void Free()
        {
            sort = 0;
            time = 0;
            use = false;
            fileLenght = 0;
            name = String.Empty;
            version = 0;
        }
    }
}