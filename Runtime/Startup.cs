using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using Unity.Burst;
using UnityEngine;
using UnityEngine.UI;
using ZEngine;
using ZEngine.Network;
using ZEngine.Resource;
using ZEngine.Window;
using Unity.Mathematics;
using ZEngine.Game;


public class Startup : MonoBehaviour
{
    private void Start()
    {
        GameObject.DontDestroyOnLoad(Camera.main.gameObject);
        Engine.Window.OpenWindow<Loading>().SetInfo("检查资源更新").SetProgress(0);
        HotfixOptions.instance.preloads.ForEach(x => x.url = HotfixOptions.instance.address.Find(x => x.state == Switch.On));
        ICheckResourceUpdateExecuteHandle checkUpdateExecuteHandle = Engine.Resource.CheckModuleResourceUpdate(HotfixOptions.instance.preloads.ToArray());
        checkUpdateExecuteHandle.SubscribeProgressChange(ISubscriber.Create<float>(Engine.Window.GetWindow<Loading>().SetProgress));
        checkUpdateExecuteHandle.Subscribe(ISubscriber.Create(ResourceChekcUpdateComplete));
    }

    private void ResourceChekcUpdateComplete()
    {
        Engine.Window.GetWindow<Loading>().SetInfo("初始化默认资源").SetProgress(0);
        IResourceModuleLoaderExecuteHandle resourceModuleLoaderExecuteHandle = Engine.Resource.LoaderResourceModule(HotfixOptions.instance.preloads.ToArray());
        resourceModuleLoaderExecuteHandle.SubscribeProgressChange(ISubscriber.Create<float>(Engine.Window.GetWindow<Loading>().SetProgress));
        resourceModuleLoaderExecuteHandle.Subscribe(ISubscriber.Create<IResourceModuleLoaderExecuteHandle>(ResourcePreloadComplete));
    }

    private void ResourcePreloadComplete(IResourceModuleLoaderExecuteHandle resourcePreloadExecuteHandle)
    {
        if (resourcePreloadExecuteHandle.status is not Status.Success)
        {
            Engine.Window.MsgBox("Tips", "资源加载失败！", Application.Quit);
            return;
        }

        Engine.Console.Log("进入游戏");
        Engine.Game.OpenWorld(new WorldOptions() { name = "Test" });
        Engine.Window.MsgBox("Tips", "Loading Game Fail", Engine.Quit);
        GameEntryOptions options = HotfixOptions.instance.entryList.Find(x => x.isOn == Switch.On);
        options.LoadAssembly().Subscribe(ISubscriber.Create<IExecuteHandle<Assembly>>(args =>
        {
            if (args.status is not Status.Success)
            {
                Engine.Window.MsgBox("Error", "进入游戏失败", Engine.Quit);
                return;
            }
        }));
    }
}