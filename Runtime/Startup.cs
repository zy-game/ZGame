using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
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
        UIBind_Loading loading = UIManager.instance.Open<UIBind_Loading>();
        loading.on_setup_TextTMP("正在获取配置信息...");
        if (GlobalConfig.instance.resConfig is null || GlobalConfig.instance.gameConfig is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }

        await ResourceManager.instance.CheckUpdateResourcePackageList(GlobalConfig.instance.resConfig.module);
        await ResourceManager.instance.LoadingResourcePackageList(GlobalConfig.instance.resConfig.module);
        await GameManager.instance.EntryGame(GlobalConfig.instance.gameConfig);
    }
}