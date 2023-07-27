namespace ZEngine
{
    public enum Status : byte
    {
        On,
        Off,
    }

    public interface ISubscribe : IReference
    {
        void Execute(params object[] args);
    }
}