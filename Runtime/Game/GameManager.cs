using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace ZEngine.Game
{
    /// <summary>
    /// 游戏管理器
    /// </summary>
    class GameManager : Singleton<GameManager>
    {
        public Camera main { get; set; }

        public void Initialize()
        {
            GameObject.DontDestroyOnLoad(Camera.main.gameObject);
            this.main = Camera.main;
        }

        public UniTask<ILogicModuleLoadResult> LoadGameLogicAssembly(GameEntryOptions gameEntryOptions)
        {
            UniTaskCompletionSource<ILogicModuleLoadResult> uniTaskCompletionSource = new UniTaskCompletionSource<ILogicModuleLoadResult>();
            ILogicModuleLoadResult gameLogicModuleLoadResult = ILogicModuleLoadResult.Create(gameEntryOptions, uniTaskCompletionSource);
            return uniTaskCompletionSource.Task;
        }
    }
}