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
    public interface IGameLogicLoadResult : IDisposable
    {
        Status status { get; }
        Assembly assembly { get; }
        GameEntryOptions gameEntryOptions { get; }

        public static IGameLogicLoadResult Create(GameEntryOptions gameEntryOptions, UniTaskCompletionSource<IGameLogicLoadResult> uniTaskCompletionSource)
        {
            GameLogicLoadingResult gameLogicLoadingResult = Activator.CreateInstance<GameLogicLoadingResult>();
            gameLogicLoadingResult.gameEntryOptions = gameEntryOptions;
            gameLogicLoadingResult.Execute(uniTaskCompletionSource);
            return gameLogicLoadingResult;
        }

        class GameLogicLoadingResult : IGameLogicLoadResult
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

            public async void Execute(UniTaskCompletionSource<IGameLogicLoadResult> uniTaskCompletionSource)
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
                    Launche.Console.Error("模块入口参数错误");
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
                    Launche.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
                    status = Status.Failed;
                    uniTaskCompletionSource.TrySetResult(this);
                    return;
                }

                string methodName = gameEntryOptions.methodName.Substring(gameEntryOptions.methodName.LastIndexOf('.') + 1);
                MethodInfo entry = entryType.GetMethod(methodName);
                if (entry is null)
                {
                    Launche.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
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
                IRequestAssetObjectResult<TextAsset> requestAssetObjectResult = default;
                requestAssetObjectResult = await Launche.Resource.LoadAssetAsync<TextAsset>(gameEntryOptions.dllName);
                if (requestAssetObjectResult.status is not Status.Success)
                {
                    status = Status.Failed;
                    return;
                }

                try
                {
                    assembly = Assembly.Load(requestAssetObjectResult.result.bytes);
                }
                catch (Exception e)
                {
                    Launche.Console.Error(e);
                }
            }

            public async UniTask LoadAOTDll()
            {
                IRequestAssetObjectResult<TextAsset> requestAssetObjectResult = default;
                if (gameEntryOptions.aotList is not null && gameEntryOptions.aotList.Count > 0)
                {
                    HomologousImageMode mode = HomologousImageMode.SuperSet;
                    foreach (var item in gameEntryOptions.aotList)
                    {
                        requestAssetObjectResult = await Launche.Resource.LoadAssetAsync<TextAsset>(item + ".bytes");
                        if (requestAssetObjectResult.result == null)
                        {
                            status = Status.Failed;
                            return;
                        }

                        LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(requestAssetObjectResult.result.bytes, mode);
                        Debug.Log($"LoadMetadataForAOTAssembly:{item}. mode:{mode} ret:{err}");
                    }
                }
            }
        }
    }
}