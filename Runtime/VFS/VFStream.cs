using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.FileSystem
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
            return Time.realtimeSinceStartup - time < BasicConfig.instance.resTimeout;
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

        public ResultStatus Write(int scrOffset, byte[] bytes, int offset, int lenght)
        {
            try
            {
                fileStream.Seek(scrOffset, SeekOrigin.Begin);
                fileStream.Write(bytes, offset, lenght);
                time = Time.realtimeSinceStartup + BasicConfig.instance.resTimeout;
                fileStream.Flush();
                return ResultStatus.Success;
            }
            catch (Exception e)
            {
                WorkApi.Logger.LogError(e);
                return ResultStatus.Fail;
            }
        }

        public async UniTask<ResultStatus> WriteAsync(int scrOffset, byte[] bytes, int offset, int lenght, CancellationToken cancellationToken)
        {
            try
            {
                fileStream.Seek(scrOffset, SeekOrigin.Begin);
                await fileStream.WriteAsync(bytes, offset, lenght, cancellationToken);
                time = Time.realtimeSinceStartup + BasicConfig.instance.resTimeout;
                await fileStream.FlushAsync();
                return ResultStatus.Success;
            }
            catch (Exception e)
            {
                WorkApi.Logger.LogError(e);
                return ResultStatus.Fail;
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
            time = Time.realtimeSinceStartup + BasicConfig.instance.resTimeout;
        }

        public async UniTask<ResultStatus> ReadAsync(int scrOffset, byte[] bytes, int offset, int lenght, CancellationToken cancellationToken)
        {
            // WorkApi.Logger.Log("chunk offset:" + scrOffset + " scr offset:" + offset + " chunk size:" + lenght);
            if (bytes.Length < offset + lenght)
            {
                Debug.LogError(new IndexOutOfRangeException());
                return ResultStatus.Fail;
            }

            try
            {
                fileStream.Seek(scrOffset, SeekOrigin.Begin);
                await fileStream.ReadAsync(bytes, offset, lenght, cancellationToken);
                time = Time.realtimeSinceStartup + BasicConfig.instance.resTimeout;
                return ResultStatus.Success;
            }
            catch (Exception e)
            {
                WorkApi.Logger.LogError(e);
                return ResultStatus.Fail;
            }
        }
    }
}