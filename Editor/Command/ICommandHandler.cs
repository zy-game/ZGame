using System;

namespace ZGame.Editor.Command
{
    public interface ICommandHandler : IDisposable
    {
        void OnExecute(params object[] args);
    }

    public interface ICommandHandler<T> : ICommandHandler
    {
        T OnExecute(params object[] args);
    }
}