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
        ICheckResourceUpdateScheduleHandle checkUpdateScheduleHandle = Engine.Resource.CheckModuleResourceUpdate(HotfixOptions.instance.preloads.ToArray());
        checkUpdateScheduleHandle.SubscribeProgressChange(ISubscriber.Create<float>(Engine.Window.GetWindow<Loading>().SetProgress));
        checkUpdateScheduleHandle.Subscribe(ISubscriber.Create<ICheckResourceUpdateScheduleHandle>(ResourceChekcUpdateComplete));
    }

    private void ResourceChekcUpdateComplete(ICheckResourceUpdateScheduleHandle checkResourceUpdateScheduleHandle)
    {
        checkResourceUpdateScheduleHandle.Dispose();
        Engine.Window.GetWindow<Loading>().SetInfo("初始化默认资源").SetProgress(0);
        IResourceModuleLoaderScheduleHandle resourceModuleLoaderScheduleHandle = Engine.Resource.LoaderResourceModule(HotfixOptions.instance.preloads.ToArray());
        resourceModuleLoaderScheduleHandle.SubscribeProgressChange(ISubscriber.Create<float>(Engine.Window.GetWindow<Loading>().SetProgress));
        resourceModuleLoaderScheduleHandle.Subscribe(ISubscriber.Create<IResourceModuleLoaderScheduleHandle>(ResourcePreloadComplete));
    }


    private void ResourcePreloadComplete(IResourceModuleLoaderScheduleHandle resourcePreloadScheduleHandle)
    {
        if (resourcePreloadScheduleHandle.status is not Status.Success)
        {
            Engine.Window.MsgBox("Tips", "资源加载失败！", Application.Quit);
            return;
        }

        resourcePreloadScheduleHandle.Dispose();
        Engine.Console.Log("进入游戏");
        Engine.Game.OpenWorld(new WorldOptions() { name = "Test" });
        Engine.Window.MsgBox("Tips", "Loading Game Fail", Engine.Quit);
        GameEntryOptions options = HotfixOptions.instance.entryList.Find(x => x.isOn == Switch.On);
        Engine.Game.LoadGameLogic(options).Subscribe(ISubscriber.Create<IGameLogicLoadResult>(args =>
        {
            IRequestAssetObjectSchedule<Sprite> requestAssetObjectSchedule = Engine.Resource.LoadAsset<Sprite>("Assets/Test/New Folder/077659b2.90daa6b4-0592-4046-aabb-daeb013cc6c9.png");
            Engine.Console.Log(requestAssetObjectSchedule.result);
            requestAssetObjectSchedule.Release();
            if (args.status is not Status.Success)
            {
                Engine.Window.MsgBox("Error", "进入游戏失败", () => { });
                return;
            }
        }));
    }
}