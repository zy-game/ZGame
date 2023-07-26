namespace ZEngine
{
    public interface IGameExecuteHandle : IReference
    {
        float progress { get; }
        ExecuteStatus status { get; }
        void Execute(params object[] args);
    }
}