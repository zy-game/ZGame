using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using HybridCLR;
using UnityEngine;
using ZEngine.Network;
using ZEngine.Resource;

namespace ZEngine.World
{
    public interface ILoaderGameLogicModuleExecuteHandle : IExecuteHandle<ILoaderGameLogicModuleExecuteHandle>
    {
    }

    class InternalLaunchGameLogicModuleExecuteHandle : ExecuteHandle, ILoaderGameLogicModuleExecuteHandle
    {
        private GameEntryOptions gameEntryOptions;

        public IEnumerator LoadAssembly()
        {
            IRequestAssetExecuteResult<TextAsset> requestAssetExecuteResult = default;
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
                yield break;
            }

            try
            {
                Assembly assembly = Assembly.Load(requestAssetExecuteResult.asset.bytes);
                assembly.InvokeMethod("Main", new object[] { });
                status = Status.Success;
            }
            catch (Exception e)
            {
                Engine.Console.Error(e);
                Engine.Window.MsgBox("Loading Game Fail", Engine.Custom.Quit);
                status = Status.Failed;
            }
        }

        public override void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            this.gameEntryOptions = (GameEntryOptions)paramsList[0];
            this.StartCoroutine(LoadAssembly());
        }
    }
}