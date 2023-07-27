namespace ZEngine.VFS
{
    public interface IWriteFileAsyncExecuteHandle : IGameAsyncExecuteHandle<WriteFileResult>
    {
    }

    class GameWriteFileAsyncExecuteHandle : IWriteFileAsyncExecuteHandle
    {
        public WriteFileResult result { get; }
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