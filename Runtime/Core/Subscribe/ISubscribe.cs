using System.Linq;

namespace ZEngine
{
    public interface ISubscribe : IReference
    {
        void Execute(params object[] args);
    }

    public interface ISubscribe<T> : ISubscribe
    {
        void Execute(T args);
    }
}