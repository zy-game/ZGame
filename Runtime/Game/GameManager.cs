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
        public UniTask<ILogicLoadResult> LoadGameLogicAssembly(GameEntryOptions gameEntryOptions)
        {
            UniTaskCompletionSource<ILogicLoadResult> uniTaskCompletionSource = new UniTaskCompletionSource<ILogicLoadResult>();
            ILogicLoadResult gameLogicLoadResult = ILogicLoadResult.Create(gameEntryOptions, uniTaskCompletionSource);
            return uniTaskCompletionSource.Task;
        }
    }
}