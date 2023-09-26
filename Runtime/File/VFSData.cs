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
        public int version;

        /// <summary>
        /// 是否在使用
        /// </summary>
        public Switch use { get; private set; }

        public void Write(byte[] bytes, int offset, int lenght, int version, int sort = 0)
        {
            VFSManager.VFSHandle handle = VFSManager.instance.GetVFSHandle(vfs);
            if (handle is null)
            {
                Engine.Console.Error(new FileNotFoundException(vfs));
                return;
            }

            use = Switch.On;
            handle.fileStream.Seek(this.offset, SeekOrigin.Begin);
            handle.fileStream.Write(bytes, offset, lenght);
            time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            fileLenght = lenght;
            this.sort = sort;
            this.version = version;
        }


        public void Read(byte[] bytes, int offset, int lenght)
        {
            VFSManager.VFSHandle handle = VFSManager.instance.GetVFSHandle(vfs);
            if (handle is null)
            {
                Engine.Console.Error(new FileNotFoundException(vfs));
                return;
            }

            if (bytes.Length < offset + lenght)
            {
                Engine.Console.Error(new IndexOutOfRangeException());
                return;
            }

            handle.fileStream.Seek(this.offset, SeekOrigin.Begin);
            handle.fileStream.Read(bytes, this.offset, lenght);
            time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public void Free()
        {
            sort = 0;
            time = 0;
            use = Switch.Off;
            fileLenght = 0;
            name = String.Empty;
            version = 0;
        }
    }
}