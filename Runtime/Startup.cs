using System;
using UnityEngine;
using ZGame;
using ZGame.Config;
using ZGame.Game;
using ZGame.Resource;
using ZGame.UI;

public class Startup : MonoBehaviour
{
    private async void Start()
    {
        Debug.Log(GameManager.DefaultWorld.name);
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Localliztion.instance.Switch(BasicConfig.instance.language);
        BehaviourScriptable.instance.SetupKeyDownEvent(KeyCode.Escape, keyEvent => { UIMsgBox.Show(Localliztion.instance.Query("是否退出"), GameManager.instance.QuitGame); });
        UILoading.SetTitle(Localliztion.instance.Query("正在获取配置信息..."));
        UILoading.SetProgress(0);
        if (BasicConfig.instance.curEntry is null)
        {
            Debug.LogError(new EntryPointNotFoundException());
            return;
        }

        if (Application.version.Equals(BasicConfig.instance.curEntry))
        {
            
        }

        await PackageManifestManager.instance.Setup(BasicConfig.instance.curEntry.module);
        await ResourceManager.instance.PreloadingResourcePackageList(BasicConfig.instance.curEntry);
        await GameManager.instance.JoinGame(BasicConfig.instance.curEntry);
    }
}