using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UI;
using UnityEngine;
using ZGame;
using ZGame.Game;
using ZGame.Resource;
using ZGame.State;
using ZGame.Window;

public class Startup : MonoBehaviour
{
    private async void Start()
    {
        CameraManager.instance.SetMainCamera();
        CameraManager.instance.NewCamera("test", 0, "Default");
        ILoading loading = UIManager.instance.Open<ILoading>();
        loading.SetTitle("正在获取配置信息...");
        if (GlobalConfig.instance.curEntry is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }

        await ResourceManager.instance.CheckUpdateResourcePackageList(GlobalConfig.instance.curEntry.module);
        await ResourceManager.instance.LoadingResourcePackageList(GlobalConfig.instance.curEntry.module);
        await GameManager.instance.EntryGame(GlobalConfig.instance.curEntry);
    }
}