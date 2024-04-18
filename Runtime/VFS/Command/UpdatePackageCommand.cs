using System.Linq;
using Cysharp.Threading.Tasks;
using ZGame.Language;
using ZGame.UI;

namespace ZGame.VFS.Command
{
    public class UpdatePackageCommand : ICommandHandlerAsync<Status>
    {
        public async UniTask<Status> OnExecute(params object[] args)
        {
            ResourcePackageManifest[] manifests = (ResourcePackageManifest[])args;
            if (manifests is null || manifests.Length == 0)
            {
                return Status.Success;
            }

            UILoading.SetProgress(0);
            UILoading.SetTitle(CoreAPI.Language.Query(CommonLanguage.UpdateResList));
            using (DownloadGroup downloadGroup = DownloadGroup.Create(x => { UILoading.SetProgress(x.progress); }))
            {
                foreach (ResourcePackageManifest manifest in manifests)
                {
                    downloadGroup.Add(ResConfig.instance.GetFilePath(manifest.name), manifest.version, null);
                }

                if (await downloadGroup.StartAsync() is not Status.Success)
                {
                    UIMsgBox.Show("更新资源失败", GameFrameworkStartup.Quit);
                    return Status.Fail;
                }

                UniTask<Status>[] writeHandles = new UniTask<Status>[downloadGroup.items.Length];
                for (int i = 0; i < downloadGroup.items.Length; i++)
                {
                    writeHandles[i] = CoreAPI.VFS.WriteAsync(downloadGroup.items[i].name, downloadGroup.items[i].bytes, downloadGroup.items[i].version);
                }

                Status[] result = await UniTask.WhenAll(writeHandles);
                if (result.Any(x => x is not Status.Success))
                {
                    UIMsgBox.Show("更新资源失败", GameFrameworkStartup.Quit);
                    return Status.Fail;
                }

                CoreAPI.Logger.Log("资源更新完成");
                UILoading.SetTitle(CoreAPI.Language.Query(CommonLanguage.ResUpdateComplete));
                UILoading.SetProgress(1);
                return Status.Success;
            }
        }

        public void Release()
        {
        }
    }
}