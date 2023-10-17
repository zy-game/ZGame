using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using ZEngine.Network;
using ZEngine.Resource;

namespace ZEngine.Game
{
    /// <summary>
    /// 逻辑模块加载结果
    /// </summary>
    public interface ILogicModuleLoadResult : IDisposable
    {
        Status status { get; }
        Assembly assembly { get; }
        GameEntryOptions gameEntryOptions { get; }

        internal static ILogicModuleLoadResult Create(GameEntryOptions gameEntryOptions, UniTaskCompletionSource<ILogicModuleLoadResult> uniTaskCompletionSource)
        {
            GameLogicModuleLoadingResult gameLogicModuleLoadingResult = Activator.CreateInstance<GameLogicModuleLoadingResult>();
            gameLogicModuleLoadingResult.gameEntryOptions = gameEntryOptions;
            gameLogicModuleLoadingResult.Execute(uniTaskCompletionSource);
            return gameLogicModuleLoadingResult;
        }

        class GameLogicModuleLoadingResult : ILogicModuleLoadResult
        {
            public Status status { get; set; }
            public Assembly assembly { get; set; }
            public GameEntryOptions gameEntryOptions { get; set; }

            public void Dispose()
            {
                gameEntryOptions = null;
                assembly = null;
                status = Status.None;
                GC.SuppressFinalize(this);
            }

            public async void Execute(UniTaskCompletionSource<ILogicModuleLoadResult> uniTaskCompletionSource)
            {
                if (status is not Status.None)
                {
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix == Switch.Off || HotfixOptions.instance.useScript == Switch.Off)
                {
                    status = Status.Success;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }
#endif
                if (gameEntryOptions is null || gameEntryOptions.methodName.IsNullOrEmpty() || gameEntryOptions.isOn == Switch.Off)
                {
                    ZGame.Console.Error("模块入口参数错误");
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                await LoadAOTDll();
                await LoadLogicAssembly();
                if (assembly is null)
                {
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                Type entryType = assembly.GetType(gameEntryOptions.methodName);
                if (entryType is null)
                {
                    ZGame.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                string methodName = gameEntryOptions.methodName.Substring(gameEntryOptions.methodName.LastIndexOf('.') + 1);
                MethodInfo entry = entryType.GetMethod(methodName);
                if (entry is null)
                {
                    ZGame.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                entry.Invoke(null, gameEntryOptions.paramsList?.ToArray());
                status = Status.Success;
                uniTaskCompletionSource.TrySetResult(this);
            }

            public async UniTask LoadLogicAssembly()
            {
                IRequestResourceObjectResult requestResourceObjectResult = default;
                requestResourceObjectResult = await ZGame.Resource.LoadAssetAsync(gameEntryOptions.dllName);
                if (requestResourceObjectResult.status is not Status.Success)
                {
                    status = Status.Failed;
                    return;
                }

                try
                {
                    assembly = Assembly.Load(requestResourceObjectResult.GetObject<TextAsset>().bytes);
                }
                catch (Exception e)
                {
                    ZGame.Console.Error(e);
                }
            }

            public async UniTask LoadAOTDll()
            {
                IRequestResourceObjectResult requestResourceObjectResult = default;
                if (gameEntryOptions.aotList is not null && gameEntryOptions.aotList.Count > 0)
                {
                    HomologousImageMode mode = HomologousImageMode.SuperSet;
                    foreach (var item in gameEntryOptions.aotList)
                    {
                        requestResourceObjectResult = await ZGame.Resource.LoadAssetAsync(item + ".bytes");
                        if (requestResourceObjectResult.result == null)
                        {
                            status = Status.Failed;
                            return;
                        }

                        LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(requestResourceObjectResult.GetObject<TextAsset>().bytes, mode);
                        Debug.Log($"LoadMetadataForAOTAssembly:{item}. mode:{mode} ret:{err}");
                    }
                }
            }
        }
    }
}