using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using ZEngine;
using ZEngine.Network;
using ZEngine.Resource;
using ZEngine.Window;
using ZEngine.World;

public class Startup : MonoBehaviour
{
    private void Start()
    {
        GameObject.DontDestroyOnLoad(Camera.main.gameObject);
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
        Engine.Game.LoaderGameLogicModule(HotfixOptions.instance.entryList.Find(x => x.isOn == Switch.On));
        Engine.Network.WriteAndFlush<TestMessage2>("", new TestMessage())?.Subscribe(ISubscribeHandle.Create<IWriteMessageExecuteHandle<TestMessage2>>(args => { Engine.Console.Log("response"); }));
    }

    [RPCHandle]
    class TestMessage : IMessagePackage
    {
        public void Release()
        {
        }
    }

    [RPCHandle]
    class TestMessage2 : IMessagePackage
    {
        public void Release()
        {
        }
    }
}