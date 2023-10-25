using System;

namespace ZGame.Game
{
    public interface IEntryGameResult : IRequest
    {
    }

    public interface IGameEntryOptions : IOptions
    {
        
    }

    public interface IGameSystem : ISystem
    {
        void EntryGame(IGameEntryOptions options, Action<IEntryGameResult> pipeline);
    }
}