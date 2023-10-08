using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

namespace ZEngine.Game
{
    /// <summary>
    /// 游戏管理器
    /// </summary>
    class GameManager : Singleton<GameManager>
    {
        public IGameLoadHandle LoadGameLogicAssembly(GameEntryOptions gameEntryOptions)
        {
            IGameLoadHandle gameLoadHandle = IGameLoadHandle.Create(gameEntryOptions);
            gameLoadHandle.Execute();
            return gameLoadHandle;
        }
    }
}