using System;
using System.IO;
using System.Threading.Tasks;

namespace ZEngine.VFS
{
    public class VFSData
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string name;

        /// <summary>
        /// 起始偏移
        /// </summary>
        public int offset;

        /// <summary>
        /// 数据长度
        /// </summary>
        public int length;

        /// <summary>
        /// 文件长度
        /// </summary>
        public int fileLenght;

        /// <summary>
        /// 文件序号
        /// </summary>
        public int sort;

        /// <summary>
        /// 所在vfs文件
        /// </summary>
        public string vfs;

        /// <summary>
        /// 文件最后访问时间
        /// </summary>
        public long time;

        /// <summary>
        /// 文件版本
        /// </summary>
        public VersionOptions version;


        public void Write(byte[] bytes, int offset, int lenght, VersionOptions version, int sort = 0)
        {
            FileStream stream = VFSManager.instance.GetFileStream(vfs);
            if (stream is null)
            {
                Engine.Console.Error(GameEngineException.Create(new FileNotFoundException(vfs)));
                return;
            }

            stream.Seek(this.offset, SeekOrigin.Begin);
            stream.Write(bytes, offset, lenght);
            time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            fileLenght = bytes.Length;
            this.sort = sort;
            this.version = version;
        }


        public void Read(byte[] bytes, int offset, int lenght)
        {
            FileStream stream = VFSManager.instance.GetFileStream(vfs);
            if (stream is null)
            {
                Engine.Console.Error(GameEngineException.Create(new FileNotFoundException(vfs)));
                return;
            }

            if (bytes.Length < offset + length)
            {
                Engine.Console.Error(GameEngineException.Create(new IndexOutOfRangeException()));
                return;
            }

            stream.Seek(this.offset, SeekOrigin.Begin);
            stream.Read(bytes, this.offset, lenght);
            time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void Free()
        {
            sort = 0;
            time = 0;
            fileLenght = 0;
            name = String.Empty;
            version = VersionOptions.None;
        }
    }
}