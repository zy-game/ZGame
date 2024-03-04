using System;

namespace ZGame
{
    [Serializable]
    public class VFSConfig
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
    }
}