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
        LayerManager.instance.NewCamera("UICamera", 999, "UI");
        Loading loading = UIManager.instance.TryOpen<Loading>();
        loading.TextMeshProUGUI_TextTMP.Setup("正在获取配置信息...");
        if (GlobalConfig.current is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }

        await ResourceManager.instance.CheckUpdateResourcePackageList(loading.SetupProgress, GlobalConfig.current.module);
        await ResourceManager.instance.LoadingResourcePackageList(loading.SetupProgress, GlobalConfig.current.module);
        await GameManager.instance.EntryGame(GlobalConfig.current);
    }
}