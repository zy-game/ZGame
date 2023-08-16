using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZEngine;
using ZEngine.Resource;
using ZEngine.Window;
using ZEngine.World;

public class Startup : MonoBehaviour
{
    private void Start()
    {
        Engine.Window.Toast("Tips");
        Loading loading = Engine.Window.OpenWindow<Loading>().SetInfo("检查资源更新").SetProgress(0);
        HotfixOptions.instance.preloads.ForEach(x => x.url = HotfixOptions.instance.address.Find(x => x.state == Switch.On));
        ICheckResourceUpdateExecuteHandle checkUpdateExecuteHandle = Engine.Resource.CheckModuleResourceUpdate(HotfixOptions.instance.preloads.ToArray());
        checkUpdateExecuteHandle.OnPorgressChange(loading.GetProgressSubscribe());
        checkUpdateExecuteHandle.Subscribe(ISubscribeHandle.Create(ResourceChekcUpdateComplete));
    }

    private void ResourceChekcUpdateComplete()
    {
        Engine.Window.GetWindow<Loading>().SetInfo("初始化默认资源").SetProgress(0);
        IResourceModuleLoaderExecuteHandle resourceModuleLoaderExecuteHandle = Engine.Resource.LoaderResourceModule(HotfixOptions.instance.preloads.ToArray());
        resourceModuleLoaderExecuteHandle.OnPorgressChange(Engine.Window.GetWindow<Loading>().GetProgressSubscribe());
        resourceModuleLoaderExecuteHandle.Subscribe(ISubscribeHandle<IResourceModuleLoaderExecuteHandle>.Create(ResourcePreloadComplete));
    }

    private void ResourcePreloadComplete(IResourceModuleLoaderExecuteHandle resourcePreloadExecuteHandle)
    {
        if (resourcePreloadExecuteHandle.status is not Status.Success)
        {
            Engine.Window.MsgBox("资源加载失败！", Application.Quit);
            return;
        }

        Engine.Console.Log("进入游戏");
    }
}