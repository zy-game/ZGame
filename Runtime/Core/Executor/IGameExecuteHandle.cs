namespace ZEngine
{
    public interface IGameExecuteHandle<out T> : IReference
    {
        T result { get; }
        float progress { get; }
        ExecuteStatus status { get; }

        void Execute(params object[] args);

        bool EnsureExecuteSuccessfuly();
    }
}