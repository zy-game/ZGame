using System;
using System.Collections;
using System.Reflection;
using HybridCLR;
using UnityEngine;
using ZEngine.Resource;

namespace ZEngine.Game
{
    public interface IGameModuleLoaderExecuteHandle : IExecuteHandle
    {
        Assembly assembly { get; }
        GameEntryOptions gameEntryOptions { get; }


        internal static IGameModuleLoaderExecuteHandle Create(GameEntryOptions gameEntryOptions)
        {
            InternalGameModuleLoaderExecuteHandle internalGameModuleLoaderExecuteHandle = Engine.Class.Loader<InternalGameModuleLoaderExecuteHandle>();
            internalGameModuleLoaderExecuteHandle.gameEntryOptions = gameEntryOptions;
            return internalGameModuleLoaderExecuteHandle;
        }

        class InternalGameModuleLoaderExecuteHandle : AbstractExecuteHandle, IGameModuleLoaderExecuteHandle
        {
            public Assembly assembly { get; set; }
            public GameEntryOptions gameEntryOptions { get; set; }

            protected override IEnumerator ExecuteCoroutine()
            {
                if (gameEntryOptions is null || gameEntryOptions.methodName.IsNullOrEmpty() || gameEntryOptions.isOn == Switch.Off)
                {
                    Engine.Console.Error("模块入口参数错误");
                    yield break;
                }

                IRequestAssetExecute<TextAsset> requestAssetExecuteResult = default;
                if (gameEntryOptions.aotList is not null && gameEntryOptions.aotList.Count > 0)
                {
                    HomologousImageMode mode = HomologousImageMode.SuperSet;
                    foreach (var item in gameEntryOptions.aotList)
                    {
                        requestAssetExecuteResult = Engine.Resource.LoadAsset<TextAsset>(item + ".bytes");
                        if (requestAssetExecuteResult.asset == null)
                        {
                            yield break;
                        }

                        LoadImageErrorCode err = RuntimeApi.LoadMetadataForAOTAssembly(requestAssetExecuteResult.asset.bytes, mode);
                        Debug.Log($"LoadMetadataForAOTAssembly:{item}. mode:{mode} ret:{err}");
                    }
                }

                requestAssetExecuteResult = Engine.Resource.LoadAsset<TextAsset>(gameEntryOptions.dllName);
                if (requestAssetExecuteResult.asset == null)
                {
                    status = Status.Failed;
                    yield break;
                }

                try
                {
                    assembly = Assembly.Load(requestAssetExecuteResult.asset.bytes);
                    Type entryType = assembly.GetType(gameEntryOptions.methodName);
                    if (entryType is null)
                    {
                        Engine.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
                        status = Status.Failed;
                        yield break;
                    }

                    string methodName = gameEntryOptions.methodName.Substring(gameEntryOptions.methodName.LastIndexOf('.') + 1);
                    MethodInfo entry = entryType.GetMethod(methodName);
                    if (entry is null)
                    {
                        Engine.Console.Error("未找到入口函数：" + gameEntryOptions.methodName);
                        status = Status.Failed;
                        yield break;
                    }

                    entry.Invoke(null, gameEntryOptions.paramsList?.ToArray());
                    status = Status.Success;
                }
                catch (Exception e)
                {
                    status = Status.Failed;
                    Engine.Console.Error(e);
                    Engine.Window.MsgBox("Tips", "Loading Game Fail", Engine.Custom.Quit);
                }
            }
        }
    }
}