using System;
using UnityEngine;
using ZGame;
using ZGame.Game;
using ZGame.Localization;
using ZGame.Options;
using ZGame.Resource;
using ZGame.Window;

public class Startup : MonoBehaviour
{
    private void Start()
    {
        GlobalOptions globalOptions = ISystem.Require<IOptionsSystem>().GetOptions<GlobalOptions>();
        if (globalOptions is null)
        {
            ISystem.Require<IWindowSystem>().Open<IMessageBox>().Setup("加载公共配置失败", IEvent.Builder(ISystem.Quit));
            return;
        }

        ISystem.Require<IResourceSystem>().CheckResourceGroupStatus(globalOptions.startboot, IEvent.Builder<ICheckResourceGroupStatusResult>(CheckResourceGroupStatusComplation));
    }

    private void CheckResourceGroupStatusComplation(ICheckResourceGroupStatusResult checkResourceGroupStatusResult)
    {
        if (checkResourceGroupStatusResult.EnsureRequestSuccessfuly() is false)
        {
            ISystem.Require<IWindowSystem>().Open<IMessageBox>().Setup("检测资源失败", IEvent.Builder(ISystem.Quit));
            return;
        }

        ISystem.Require<IResourceSystem>().UpdateResourceGroup(checkResourceGroupStatusResult, IEvent.Builder<IUpdateResourceGroupResult>(UpdateResourceGroupComlation));
    }

    private void UpdateResourceGroupComlation(IUpdateResourceGroupResult updateResourceGroupResult)
    {
        if (updateResourceGroupResult.EnsureRequestSuccessfuly() is false)
        {
            ISystem.Require<IWindowSystem>().Open<IMessageBox>().Setup("更新失败", IEvent.Builder(ISystem.Quit));
            return;
        }

        ISystem.Require<IGameSystem>().EntryGame(ISystem.Require<IOptionsSystem>().GetOptions<IGameEntryOptions>(), IEvent.Builder<IEntryGameResult>(EntryGameComplations));
    }

    private void EntryGameComplations(IEntryGameResult entryGameResult)
    {
        if (entryGameResult.EnsureRequestSuccessfuly())
        {
            return;
        }

        ISystem.Require<IWindowSystem>().Open<IMessageBox>().Setup("进入游戏失败", IEvent.Builder(ISystem.Quit));
    }
}