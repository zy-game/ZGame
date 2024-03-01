using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame;
using ZGame.Config;
using ZGame.Game;
using ZGame.Resource;
using ZGame.UI;

public class Startup : MonoBehaviour
{
    private async UniTaskVoid Start()
    {
        Debug.Log(GameManager.DefaultWorld.name);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Localliztion.instance.Switch(BasicConfig.instance.language);
        UILoading.SetTitle(Localliztion.instance.Query("正在获取配置信息..."));
        UILoading.SetProgress(0);
        if (BasicConfig.instance.curEntry is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }

        await PackageManifestManager.instance.SetupPackageManifest(BasicConfig.instance.curEntry.module);
        await ResourceManager.instance.PreloadingResourcePackageList(BasicConfig.instance.curEntry);
        await GameManager.instance.EntrySubGame(BasicConfig.instance.curEntry);
    }
}