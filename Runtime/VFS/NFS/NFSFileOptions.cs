using System;

namespace ZGame.VFS
{
    [Serializable]
    class NFSFileOptions
    {
        /// <summary>
        /// 所在文件包
        /// </summary>
        public string bundle;

        /// <summary>
        /// 数据长度
        /// </summary>
        public int length;

        /// <summary>
        /// 文件块偏移
        /// </summary>
        public int offset;

        ///  <summary>
        /// 文件块索引
        /// </summary>
        public int index;

        /// <summary>
        /// 文件名
        /// </summary>
        public string name;

        /// <summary>
        /// 使用长度
        /// </summary>
        public int dataLength;

        /// <summary>
        /// 文件版本
        /// </summary>
        public uint version;

        /// <summary>
        /// 文件最后访问时间
        /// </summary>
        public long time;

        /// <summary>
        /// 文件块状态
        /// </summary>
        public byte state;


        public void Use(int index, string name, int dataLength, uint version)
        {
            this.index = index;
            this.name = name;
            this.dataLength = dataLength;
            this.version = version;
            this.time = DateTime.Now.Ticks;
            this.state = 1;
        }

        public void Unuse()
        {
            this.index = 0;
            this.name = String.Empty;
            this.dataLength = 0;
            this.version = 0;
            this.time = 0;
            this.state = 0;
        }

        public static NFSFileOptions Create(string name, int offset, int length)
        {
            NFSFileOptions options = new NFSFileOptions();
            options.bundle = name;
            options.offset = offset;
            options.length = length;
            options.Unuse();
            return options;
        }
    }
}