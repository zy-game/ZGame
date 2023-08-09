using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine;
using ZEngine.Options;
using ZEngine.Resource;
using ZEngine.World;

public class Startup : MonoBehaviour
{
    private void Start()
    {
        ICheckUpdateExecuteHandle checkUpdateExecuteHandle = Engine.Resource.CheckUpdateResource(HotfixOptions.instance.GetPreloadOptions());
        checkUpdateExecuteHandle.Subscribe(ISubscribeHandle.Create<ICheckUpdateExecuteHandle>(CheckUpdateComplete));
    }

    private void CheckUpdateComplete(ICheckUpdateExecuteHandle checkUpdateExecuteHandle)
    {
        if (checkUpdateExecuteHandle.status is not Status.Success)
        {
            Engine.Window.MsgBox("资源检查失败!", Application.Quit);
            return;
        }

        if (checkUpdateExecuteHandle.bundles is null || checkUpdateExecuteHandle.bundles.Length is 0)
        {
            Engine.Console.Log("不需要更新资源");
            Engine.Resource.PreLoadResourceModule();
            return;
        }

        Engine.Console.Log("需要更新资源:", Engine.Json.ToJson(checkUpdateExecuteHandle.bundles));
        Engine.Window.MsgBox("是否更新资源？", () =>
        {
            IUpdateResourceExecuteHandle updateResourceExecuteHandle = Engine.Resource.UpdateResourceBundle(checkUpdateExecuteHandle.bundles);
            updateResourceExecuteHandle.Subscribe(ISubscribeHandle.Create<IUpdateResourceExecuteHandle>(UpdateResourceBundleComplete));
        }, Application.Quit);
    }

    private void UpdateResourceBundleComplete(IUpdateResourceExecuteHandle updateResourceExecuteHandle)
    {
        if (updateResourceExecuteHandle.status is not Status.Success)
        {
            Engine.Window.MsgBox("资源更新失败！", Application.Quit);
            return;
        }

        IResourcePreloadExecuteHandle resourcePreloadExecuteHandle = Engine.Resource.PreLoadResourceModule();
        resourcePreloadExecuteHandle.Subscribe(ISubscribeHandle.Create<IResourcePreloadExecuteHandle>(ResourcePreloadComplete));
    }

    private void ResourcePreloadComplete(IResourcePreloadExecuteHandle resourcePreloadExecuteHandle)
    {
        if (resourcePreloadExecuteHandle.status is not Status.Success)
        {
            Engine.Window.MsgBox("资源加载失败！", Application.Quit);
            return;
        }
        //todo 进入游戏
    }
}