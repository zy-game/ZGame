using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using ZGame;
using ZGame.Game;
using ZGame.Resource;
using ZGame.UI;

public class Startup : MonoBehaviour
{
    public string args;

    private async void Start()
    {
        GameManager.instance.Initialized();
        UILoading.SetTitle("正在获取配置信息...");
        UILoading.SetProgress(0);
        if (BasicConfig.instance.curEntry is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }

        await PackageManifestManager.instance.Setup(BasicConfig.instance.curEntry.module);
        await ResourceManager.instance.PerloadingResourcePackageList(BasicConfig.instance.curEntry);
        await GameManager.instance.EntryGame(BasicConfig.instance.curEntry, args.Split(';'));
    }
}