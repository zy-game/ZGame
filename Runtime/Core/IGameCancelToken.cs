namespace ZEngine
{
    public interface IGameCancelToken : IReference
    {
        void Cancel();
        bool TryCancel();
    }
}