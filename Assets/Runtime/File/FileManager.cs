using System;
using System.IO;
using Cysharp.Threading.Tasks;

namespace ZEngine.File
{
    [Serializable]
    public sealed class FileSystemOptions
    {
        public string extension;
        public uint fileCount;
    }

    public class FileManager : Single<FileManager>
    {
        public bool Exist(string fileName)
        {
            return false;
        }

        public IWriteFileHandle WriteFile(string fileName, MemoryStream stream)
        {
            return default;
        }

        public UniTask<IWriteFileHandle> WriteFileAsync(string fileName, MemoryStream stream)
        {
            return default;
        }

        public IReadFileHandle ReadFile(string fileName)
        {
            return default;
        }

        public UniTask<IReadFileHandle> ReadFileAsync(string fileName)
        {
            return default;
        }
    }
}