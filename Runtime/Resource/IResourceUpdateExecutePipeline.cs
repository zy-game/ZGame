using ZGame.Execute;
using ZGame.Game;
using ZGame.Window;

namespace ZGame.Resource
{
    public interface IResourceUpdateExecutePipeline : IExecutePipeline
    {
    }

    class DefaultResourceUpdateExecutePipeline : IResourceUpdateExecutePipeline
    {
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public string guid { get; }

        public async void Execute(params object[] args)
        {
            // ICheckResourceGroupResult checkResourceGroupStatusResult = await CoreApi.resourceSystem.CheckResourceGroupStatus(args[0].ToString());
            // if (checkResourceGroupStatusResult.EnsureRequestSuccessfuly() is false)
            // {
            //     IMsgBox.Create("检测资源失败", CoreApi.Quit);
            //     return;
            // }
            //
            // IUpdateResourceGroupResult updateResourceGroupResult = await CoreApi.resourceSystem.UpdateResourceGroup(checkResourceGroupStatusResult);
            // if (updateResourceGroupResult.EnsureRequestSuccessfuly() is false)
            // {
            //     IMsgBox.Create("更新失败", CoreApi.Quit);
            //     return;
            // }
            //
            // IEntryGameResult entryGameResult = await CoreApi.gameSystem.EntryGame(IOptions.Requery<IGameEntryOptions>());
            // if (entryGameResult.EnsureRequestSuccessfuly())
            // {
            //     return;
            // }
            //
            // IMsgBox.Create("进入游戏失败", CoreApi.Quit);
        }
    }
}