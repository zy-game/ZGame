using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using HybridCLR;
using UI;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using ZGame;
using ZGame.Game;
using ZGame.Resource;
using ZGame.Window;

public class Startup : MonoBehaviour
{
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

        Debug.Log(BasicConfig.instance.curEntry.entryName);
        await PackageManifestManager.instance.Setup(BasicConfig.instance.curEntry.module);
        await ResourceManager.instance.PerloadingResourcePackageList(BasicConfig.instance.curEntry.module);
        await GameManager.instance.EntryGame(BasicConfig.instance.curEntry);
    }

    private void Update()
    {
        // 返回键
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIMsgBox.Show("是否退出应用？", GameManager.instance.QuitGame);
        }

        // Home键
        if (Input.GetKeyDown(KeyCode.Home))
        {
        }
    }
}