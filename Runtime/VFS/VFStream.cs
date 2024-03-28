using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.VFS
{
    /// <summary>
    /// 文件流
    /// </summary>
    internal class VFStream : IDisposable
    {
        private float time;
        public string name;
        private FileStream fileStream;

        public VFStream(string path)
        {
            name = Path.GetFileName(path);
            time = Time.realtimeSinceStartup;
            fileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public bool CheckIsTimeout()
        {
            return Time.realtimeSinceStartup - time < VFSConfig.instance.timeout;
        }

        public void Dispose()
        {
            time = 0;
            Debug.Log("Dispose File: " + name);
            name = String.Empty;
            fileStream.Flush();
            fileStream.Close();
            fileStream.Dispose();
        }

        public Status Write(int scrOffset, byte[] bytes, int offset, int lenght)
        {
            try
            {
                fileStream.Seek(scrOffset, SeekOrigin.Begin);
                fileStream.Write(bytes, offset, lenght);
                time = Time.realtimeSinceStartup + VFSConfig.instance.timeout;
                fileStream.Flush();
                return Status.Success;
            }
            catch (Exception e)
            {
                GameFrameworkEntry.Logger.LogError(e);
                return Status.Fail;
            }
        }

        public async UniTask<Status> WriteAsync(int scrOffset, byte[] bytes, int offset, int lenght, CancellationToken cancellationToken)
        {
            try
            {
                fileStream.Seek(scrOffset, SeekOrigin.Begin);
                await fileStream.WriteAsync(bytes, offset, lenght, cancellationToken);
                time = Time.realtimeSinceStartup + VFSConfig.instance.timeout;
                await fileStream.FlushAsync();
                return Status.Success;
            }
            catch (Exception e)
            {
                GameFrameworkEntry.Logger.LogError(e);
                return Status.Fail;
            }
        }

        public void Read(int scrOffset, byte[] bytes, int offset, int lenght)
        {
            if (bytes.Length < offset + lenght)
            {
                Debug.LogError(new IndexOutOfRangeException());
                return;
            }

            fileStream.Seek(scrOffset, SeekOrigin.Begin);
            fileStream.Read(bytes, offset, lenght);
            time = Time.realtimeSinceStartup + VFSConfig.instance.timeout;
        }

        public async UniTask<Status> ReadAsync(int scrOffset, byte[] bytes, int offset, int lenght, CancellationToken cancellationToken)
        {
            // WorkApi.Logger.Log("chunk offset:" + scrOffset + " scr offset:" + offset + " chunk size:" + lenght);
            if (bytes.Length < offset + lenght)
            {
                Debug.LogError(new IndexOutOfRangeException());
                return Status.Fail;
            }

            try
            {
                fileStream.Seek(scrOffset, SeekOrigin.Begin);
                await fileStream.ReadAsync(bytes, offset, lenght, cancellationToken);
                time = Time.realtimeSinceStartup + VFSConfig.instance.timeout;
                return Status.Success;
            }
            catch (Exception e)
            {
                GameFrameworkEntry.Logger.LogError(e);
                return Status.Fail;
            }
        }
    }
}