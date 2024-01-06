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
using ZGame.Window;

public class Startup : MonoBehaviour
{
    private async void Start()
    {
        GameManager.instance.Initialized();
        UILoading uiLoading = UIManager.instance.Open<UILoading>();
        uiLoading.SetTitle("正在获取配置信息...");
        if (BasicConfig.instance.curEntry is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }

        await PackageManifestManager.instance.Setup(BasicConfig.instance.curEntry);
        await ResourceManager.instance.CheckUpdateResourcePackageList(BasicConfig.instance.curEntry);
        await ResourceManager.instance.LoadingResourcePackageList(BasicConfig.instance.curEntry);
        await GameManager.instance.EntryGame(BasicConfig.instance.curEntry);
    }
}