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
        public UniTask<ILogicModuleLoadResult> LoadGameLogicAssembly(GameEntryOptions gameEntryOptions)
        {
            UniTaskCompletionSource<ILogicModuleLoadResult> uniTaskCompletionSource = new UniTaskCompletionSource<ILogicModuleLoadResult>();
            ILogicModuleLoadResult gameLogicModuleLoadResult = ILogicModuleLoadResult.Create(gameEntryOptions, uniTaskCompletionSource);
            return uniTaskCompletionSource.Task;
        }
    }
}