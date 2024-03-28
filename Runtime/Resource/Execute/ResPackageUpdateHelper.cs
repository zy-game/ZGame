using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Resource.Config;
using ZGame.UI;

namespace ZGame.Resource
{
    class ResPackageUpdateHelper
    {
        public static async UniTask<Status> UpdateResourcePackageList(string packageName)
        {
            if (packageName.IsNullOrEmpty())
            {
                throw new ArgumentNullException("config");
            }

            UILoading.SetTitle(GameFrameworkEntry.Language.Query("正在加载资源信息..."));
            UILoading.SetProgress(0);
            ResourcePackageManifest[] manifests = GameFrameworkEntry.Resource.CheckNeedUpdatePackageList(packageName);
            if (manifests is not null && manifests.Length > 0)
            {
                if (await DownloadUpdateResourceList(manifests) is not Status.Success)
                {
                    UIMsgBox.Show("更新资源失败", GameFrameworkStartup.Quit);
                    return Status.Fail;
                }
            }

            Debug.Log("资源更新完成");
            UILoading.SetTitle(GameFrameworkEntry.Language.Query("资源更新完成..."));
            UILoading.SetProgress(1);
            return Status.Success;
        }

        private static async UniTask<Status> DownloadUpdateResourceList(params ResourcePackageManifest[] manifests)
        {
            UILoading.SetTitle(GameFrameworkEntry.Language.Query("更新资源列表中..."));
            Debug.Log("需要更新资源：" + string.Join(",", manifests.Select(x => x.name)));
            UniTask<Status>[] downloadAssetTaskList = new UniTask<Status>[manifests.Length];
            for (int i = 0; i < manifests.Length; i++)
            {
                string url = OSSConfig.instance.GetFilePath(manifests[i].name);
                downloadAssetTaskList[i] = GameFrameworkEntry.Network.DownloadStreamingAsset(url, manifests[i].name, manifests[i].version);
            }

            Extension.StartSample();
            var result = await UniTask.WhenAll(downloadAssetTaskList);
            Debug.LogFormat("更新完成，总耗时：{0}", Extension.GetSampleTime());
            return result.Where(x => x is Status.Fail).Count() == 0 ? Status.Success : Status.Fail;
        }
    }
}