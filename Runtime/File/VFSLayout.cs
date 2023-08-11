namespace ZEngine.VFS
{
    public enum VFSLayout
    {
        /// <summary>
        /// 空间优先，则文件可能存在于多个文件中
        /// </summary>
        Szie,

        /// <summary>
        /// 读写优先，文件不分片，小文件合并，但是大文件则单独写入一个文件中
        /// </summary>
        Speed,
    }
}