using System;
using System.Linq;
using System.Reflection;

namespace ZEngine.Game
{
    public interface IGame : IDisposable
    {
        string name { get; }
        IGameScene scene { get; }

        void OpenScene(ISceneOptions options);
        IPlayer CreatePlayer(IPlayerOptions options);
        void RemovePlayer(int guid);
        IPlayer FindPlayer(int guid);
    }
}