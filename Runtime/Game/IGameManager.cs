using System;
using Cysharp.Threading.Tasks;

namespace ZGame.Game
{
    public interface IEntryGameResult : IDisposable
    {
    }

    public interface IGameEntryOptions 
    {
    }


    public sealed class GameManager : IManager
    {

        public UniTask<IEntryGameResult> EntryGame(IGameEntryOptions options)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}