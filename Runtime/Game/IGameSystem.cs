using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Game
{
    public interface IEntryGameResult : IRequest
    {
    }

    public interface IGameEntryOptions : IOptions
    {
    }

    public interface IGameSystem : IManager
    {
        UniTask<IEntryGameResult> EntryGame(IGameEntryOptions options);
    }
}