using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ZGame.Resource
{
    /// <summary>
    /// 文件流
    /// </summary>
    class VFSystem : IReference
    {
        public string name;
        private FileStream fileStream;

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="destOffset">文件流偏移</param>
        /// <param name="bytes">数据缓冲区</param>
        /// <param name="offset">数据缓冲区偏移</param>
        /// <param name="length">写入长度</param>
        /// <returns></returns>
        public Status Write(int destOffset, byte[] bytes, int offset, int length)
        {
            try
            {
                fileStream.Seek(destOffset, SeekOrigin.Begin);
                fileStream.Write(bytes, offset, length);
                fileStream.Flush();
                return Status.Success;
            }
            catch (Exception e)
            {
                AppCore.Logger.LogError(e);
                return Status.Fail;
            }
        }

        /// <summary>
        /// 写入文件数据
        /// </summary>
        /// <param name="destOffset">文件流偏移</param>
        /// <param name="bytes">数据缓冲区</param>
        /// <param name="offset">数据缓冲区偏移</param>
        /// <param name="length">写入长度</param>
        /// <param name="cancellationToken">取消句柄</param>
        /// <returns></returns>
        public async UniTask<Status> WriteAsync(int destOffset, byte[] bytes, int offset, int length, CancellationToken cancellationToken = default)
        {
            try
            {
                fileStream.Seek(destOffset, SeekOrigin.Begin);
                await fileStream.WriteAsync(bytes, offset, length, cancellationToken);
                await fileStream.FlushAsync();
                return Status.Success;
            }
            catch (Exception e)
            {
                AppCore.Logger.LogError(e);
                return Status.Fail;
            }
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="srcOffset">文件流偏移</param>
        /// <param name="bytes">数据缓冲区</param>
        /// <param name="offset">数据缓冲区偏移</param>
        /// <param name="length">读取长度</param>
        /// <returns></returns>
        public Status Read(int srcOffset, byte[] bytes, int offset, int length)
        {
            try
            {
                fileStream.Seek(srcOffset, SeekOrigin.Begin);
                fileStream.Read(bytes, offset, length);
                return Status.Success;
            }
            catch (Exception e)
            {
                AppCore.Logger.LogError(e);
                return Status.Fail;
            }
        }

        /// <summary>
        /// 读取文件数据
        /// </summary>
        /// <param name="srcOffset">文件流偏移</param>
        /// <param name="bytes">数据缓冲区</param>
        /// <param name="offset">数据缓冲区偏移</param>
        /// <param name="length">读取长度</param>
        /// <param name="cancellationToken">取消的句柄</param>
        /// <returns></returns>
        public async UniTask<Status> ReadAsync(int srcOffset, byte[] bytes, int offset, int length, CancellationToken cancellationToken = default)
        {
            try
            {
                fileStream.Seek(srcOffset, SeekOrigin.Begin);
                await fileStream.ReadAsync(bytes, offset, length, cancellationToken);
                return Status.Success;
            }
            catch (Exception e)
            {
                AppCore.Logger.LogError(e);
                return Status.Fail;
            }
        }

        public void Release()
        {
            AppCore.Logger.Log("Dispose File Stream:" + name);
            fileStream.Dispose();
            fileStream = null;
            name = String.Empty;
        }

        public static VFSystem Create(string vfsName)
        {
            VFSystem vfs = RefPooled.Alloc<VFSystem>();
            vfs.name = vfsName;
            vfs.fileStream = new FileStream(AppCore.GetCachePath(vfsName), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            return vfs;
        }
    }
}