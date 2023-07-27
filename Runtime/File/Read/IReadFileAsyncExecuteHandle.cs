namespace ZEngine.VFS
{
    public interface IReadFileAsyncExecuteHandle : IGameAsyncExecuteHandle<ReadFileResult>
    {
    }

    class GameReadFileAsyncExecuteHandle : IReadFileAsyncExecuteHandle
    {
        public ReadFileResult result { get; }
        public float progress { get; }
        public ExecuteStatus status { get; }

        public void Execute(params object[] args)
        {
            throw new System.NotImplementedException();
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