using System.IO;

namespace ZEngine.VFS
{
    public interface IReadFileExecuteHandle : IGameExecuteHandle<ReadFileResult>
    {
    }

    class GameReadFileExecuteHandle : IReadFileExecuteHandle
    {
        public float progress { get; }
        public ReadFileResult result { get; }
        public ExecuteStatus status { get; }

        public void Execute(params object[] args)
        {
            
        }

        public bool EnsureExecuteSuccessfuly()
        {
            throw new System.NotImplementedException();
        }

        public void Release()
        {
            throw new System.NotImplementedException();
        }
    }
}