using Cysharp.Threading.Tasks;

namespace ZGame
{
    public interface IProcedureAsync : IProcedureAsync<object>
    {
    }

    public interface IProcedureAsync<T> : IReference
    {
        UniTask<T> Execute(params object[] args);
    }
}