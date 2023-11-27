using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.FileSystem
{
    internal class VFSStream : IDisposable
    {
        public float time;
        public string name;
        public FileStream fileStream;

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
            time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }

        public async UniTask WriteAsync(int scrOffset, byte[] bytes, int offset, int lenght)
        {
            fileStream.Seek(scrOffset, SeekOrigin.Begin);
            await fileStream.WriteAsync(bytes, offset, lenght);
            time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
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
            time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
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
            time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
        }
    }
}