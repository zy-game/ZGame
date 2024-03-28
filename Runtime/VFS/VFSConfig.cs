using System;
using System.Linq;
using UnityEngine;

namespace ZGame.VFS
{
    public class VFSConfig : BaseConfig<VFSConfig>
    {
        /// <summary>
        /// 是否启用VFS
        /// </summary>
        public bool enable = true;

        /// <summary>
        /// VFS分块大小
        /// </summary>
        public int chunkSize = 1024 * 1024;

        /// <summary>
        /// VFS数量
        /// </summary>
        public int chunkCount = 1024;

        /// <summary>
        /// 缓存超时时长
        /// </summary>
        public float timeout;
    }
}