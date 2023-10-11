using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;

namespace ZEngine.Game
{
    /// <summary>
    /// 游戏管理器
    /// </summary>
    class GameManager : Singleton<GameManager>
    {
        public UniTask<IGameLogicLoadResult> LoadGameLogicAssembly(GameEntryOptions gameEntryOptions)
        {
            UniTaskCompletionSource<IGameLogicLoadResult> uniTaskCompletionSource = new UniTaskCompletionSource<IGameLogicLoadResult>();
            IGameLogicLoadResult gameLogicLoadResult = IGameLogicLoadResult.Create(gameEntryOptions, uniTaskCompletionSource);
            return uniTaskCompletionSource.Task;
        }
    }
}