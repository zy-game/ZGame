using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
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
    private async void Start()
    {
        GameObject.DontDestroyOnLoad(Camera.main.gameObject);
        ZGame.Window.OpenWindow<Loading>().SetInfo("检查资源更新").SetProgress(0);
        HotfixOptions.instance.preloads.ForEach(x => x.url = HotfixOptions.instance.address.Find(x => x.state == Switch.On));
        IRequestResourceModuleUpdateResult requestResourceModuleUpdateResult = await ZGame.Resource.CheckModuleResourceUpdate(ZGame.Window.GetWindow<Loading>(), HotfixOptions.instance.preloads.ToArray());
        if (requestResourceModuleUpdateResult.status is not Status.Success)
        {
            requestResourceModuleUpdateResult.Dispose();
            return;
        }

        requestResourceModuleUpdateResult.Dispose();
        ZGame.Window.GetWindow<Loading>().SetInfo("初始化默认资源").SetProgress(0);
        IRequestResourceModuleLoadResult requestResourceModuleResult = await ZGame.Resource.LoaderResourceModule(ZGame.Window.GetWindow<Loading>(), HotfixOptions.instance.preloads.ToArray());
        if (requestResourceModuleResult.status is not Status.Success)
        {
            requestResourceModuleResult.Dispose();
            ZGame.Window.MsgBox("Tips", "资源加载失败！", Application.Quit);
            return;
        }

        requestResourceModuleResult.Dispose();
        ZGame.Console.Log("进入游戏");
        GameEntryOptions options = HotfixOptions.instance.entryList.Find(x => x.isOn == Switch.On);
        ILogicLoadResult gameLogicLoadResult = await ZGame.Game.LoadGameLogic(options);
        if (gameLogicLoadResult.status is Status.Success)
        {
            gameLogicLoadResult.Dispose();
            return;
        }

        string address = "127.0.0.1:28090";
        gameLogicLoadResult.Dispose();
        ZGame.Network.SubscribeMessageRecvieHandle<Recvier>();
        IChannel channel = await ZGame.Network.Connect<TCPSocket>(address);

        while (true)
        {
            ZGame.Network.WriteAndFlush(address, new TestMessage());
            await UniTask.Delay(10000);
        }

        // Launche.Window.MsgBox("Error", "进入游戏失败", Launche.Quit);
    }
}

class Recvier : IMessageRecvierHandle
{
    public void Dispose()
    {
    }

    public void OnHandleMessage(string address, uint opcode, MemoryStream bodyData)
    {
        ZGame.Console.Log(opcode);
    }
}

[Serializable]
class TestMessage : IMessaged
{
    public void Dispose()
    {
    }
}