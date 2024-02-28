using System;
using System.IO;
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
            fileStream.Close();
            fileStream.Dispose();
        }

        public void Write(int scrOffset, byte[] bytes, int offset, int lenght)
        {
            fileStream.Seek(scrOffset, SeekOrigin.Begin);
            fileStream.Write(bytes, offset, lenght);
            time = Time.realtimeSinceStartup + BasicConfig.instance.resTimeout;
            fileStream.Flush();
        }

        public async UniTask WriteAsync(int scrOffset, byte[] bytes, int offset, int lenght)
        {
            fileStream.Seek(scrOffset, SeekOrigin.Begin);
            await fileStream.WriteAsync(bytes, offset, lenght);
            time = Time.realtimeSinceStartup + BasicConfig.instance.resTimeout;
            await fileStream.FlushAsync();
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

        public async UniTask ReadAsync(int scrOffset, byte[] bytes, int offset, int lenght)
        {
            if (bytes.Length < offset + lenght)
            {
                Debug.LogError(new IndexOutOfRangeException());
                return;
            }

            fileStream.Seek(scrOffset, SeekOrigin.Begin);
            await fileStream.ReadAsync(bytes, offset, lenght);
            time = Time.realtimeSinceStartup + BasicConfig.instance.resTimeout;
        }
    }
}