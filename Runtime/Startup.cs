using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using ZEngine;
using ZEngine.Options;
using ZEngine.Resource;
using ZEngine.Window;
using ZEngine.World;

public class Startup : MonoBehaviour
{
    private void Start()
    {
        UI_Loading loading = Engine.Window.OpenWindow<UI_Loading>().SetInfo("检查资源更新").SetProgress(0);
        ICheckResourceUpdateExecuteHandle checkUpdateExecuteHandle = Engine.Resource.CheckUpdateResource(HotfixOptions.instance.GetPreloadOptions());
        checkUpdateExecuteHandle.OnPorgressChange(loading.GetProgressSubscribe());
        checkUpdateExecuteHandle.Subscribe(ISubscribeHandle.Create(ResourceChekcUpdateComplete));
    }

    private void ResourceChekcUpdateComplete()
    {
        Engine.Window.GetWindow<UI_Loading>().SetInfo("初始化默认资源").SetProgress(0);
        IResourceModuleLoaderExecuteHandle resourceModuleLoaderExecuteHandle = Engine.Resource.PreLoadResourceModule(HotfixOptions.instance.preloads.ToArray());
        resourceModuleLoaderExecuteHandle.OnPorgressChange(Engine.Window.GetWindow<UI_Loading>().GetProgressSubscribe());
        resourceModuleLoaderExecuteHandle.Subscribe(ISubscribeHandle.Create<IResourceModuleLoaderExecuteHandle>(ResourcePreloadComplete));
    }

    private void ResourcePreloadComplete(IResourceModuleLoaderExecuteHandle resourcePreloadExecuteHandle)
    {
        if (resourcePreloadExecuteHandle.status is not Status.Success)
        {
            Engine.Window.MsgBox("资源加载失败！", Application.Quit);
            return;
        }

        Engine.Console.Log("进入游戏");
        //todo 进入游戏
    }
}