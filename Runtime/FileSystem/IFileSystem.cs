using System;

namespace ZGame.FileSystem
{
    public interface IFileSystem : ISystem
    {
        bool Exsit(string fileName);
        bool Equals(int version);
        string ReadText(string fileName);
        byte[] Read(string fileName);
        void ReadAsync(string fileName, Action<byte[]> complation);
        void ReadTextAsync(string fileName, Action<string> complation);
        void Write(string fileName, byte[] bytes, int version);
        void WriteAsync(string fileName, byte[] bytes, int version, Action callback);
    }

    public class DefaultFileManagerSystem : IFileSystem
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Startup()
        {
            throw new NotImplementedException();
        }

        public bool Exsit(string fileName)
        {
            throw new NotImplementedException();
        }

        public bool Equals(int version)
        {
            throw new NotImplementedException();
        }

        public string ReadText(string fileName)
        {
            throw new NotImplementedException();
        }

        public byte[] Read(string fileName)
        {
            throw new NotImplementedException();
        }

        public void ReadAsync(string fileName, Action<byte[]> complation)
        {
            throw new NotImplementedException();
        }

        public void ReadTextAsync(string fileName, Action<string> complation)
        {
            throw new NotImplementedException();
        }

        public void Write(string fileName, byte[] bytes, int version)
        {
            throw new NotImplementedException();
        }

        public void WriteAsync(string fileName, byte[] bytes, int version, Action callback)
        {
            throw new NotImplementedException();
        }

        public string guid { get; } = ID.New();
    }
}