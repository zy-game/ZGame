using Cysharp.Threading.Tasks;

namespace ZGame.FileSystem
{
    public interface IFileSystem : IManager
    {
        bool Exsit(string fileName);
        void Delete(string fileName);
        bool Equals(string fileName, int version);
        IReadFileResult Read(string fileName);
        UniTask<IReadFileResult> ReadAsync(string fileName);
        IWriteFileResult Write(string fileName, byte[] bytes, int version);
        UniTask<IWriteFileResult> WriteAsync(string fileName, byte[] bytes, int version);
    }
}