namespace ZEngine
{
    public interface ISubscribe : IReference
    {
        void Execute(params object[] args);
    }
}