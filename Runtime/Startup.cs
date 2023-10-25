using System;
using UnityEngine;
using ZGame;
using ZGame.Game;
using ZGame.Localization;
using ZGame.Resource;
using ZGame.Window;

public class Startup : MonoBehaviour
{
    public string module;
    public string dataSystem;
    public string gameSystem;
    public string networkSystem;
    public string objectSystem;
    public string resourceSystem;
    public string windowSystem;
    public string localizetionSystem;

    private void Start()
    {
        if (string.IsNullOrEmpty(module))
        {
            IMessageBox.Create("加载公共配置失败", SystemManager.Quit);
            return;
        }

        SystemManager.resourceSystem.CheckResourceGroupStatus(module, CheckResourceGroupStatusComplation);
    }

    private void CheckResourceGroupStatusComplation(ICheckResourceGroupStatusResult checkResourceGroupStatusResult)
    {
        if (checkResourceGroupStatusResult.EnsureRequestSuccessfuly() is false)
        {
            IMessageBox.Create("检测资源失败", SystemManager.Quit);
            return;
        }

        SystemManager.resourceSystem.UpdateResourceGroup(checkResourceGroupStatusResult, UpdateResourceGroupComlation);
    }

    private void UpdateResourceGroupComlation(IUpdateResourceGroupResult updateResourceGroupResult)
    {
        if (updateResourceGroupResult.EnsureRequestSuccessfuly() is false)
        {
            IMessageBox.Create("更新失败", SystemManager.Quit);
            return;
        }

        SystemManager.gameSystem.EntryGame(IOptions.Requery<IGameEntryOptions>(), EntryGameComplations);
    }

    private void EntryGameComplations(IEntryGameResult entryGameResult)
    {
        if (entryGameResult.EnsureRequestSuccessfuly())
        {
            return;
        }

        IMessageBox.Create("进入游戏失败", SystemManager.Quit);
    }
}