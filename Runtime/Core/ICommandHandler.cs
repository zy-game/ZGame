using System;
using Cysharp.Threading.Tasks;

namespace ZGame
{
    public interface ICommandHandler : IReference
    {
        void OnExecute(params object[] args);
    }

    public interface ICommandHandler<T> : IReference
    {
        T OnExecute(params object[] args);
    }

    public interface ICommandHandlerAsync<T> : IReference
    {
        UniTask<T> OnExecute(params object[] args);
    }
}