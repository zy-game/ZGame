using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZGame.Language;
using ZGame.UI;

namespace ZGame.VFS.Command
{
    class LoadingResPackageCommand : ICommandHandler<Status>
    {
        public Status OnExecute(params object[] args)
        {
            ResourcePackageManifest[] manifests = args.Cast<ResourcePackageManifest>().ToArray();
            if (manifests is null || manifests.Length == 0)
            {
                return Status.Success;
            }
#if UNITY_EDITOR
            using (ProfileWatcher watcher = ProfileWatcher.StartProfileWatcher(nameof(LoadingResPackageCommand)))
            {
#endif
                for (int i = 0; i < manifests.Length; i++)
                {
                    if (LoadingAssetBundleSync(manifests[i]) is not Status.Success)
                    {
                        manifests.Select(x => x.name).ToList().ForEach(x => ResPackage.Unload(x));
                        return Status.Fail;
                    }
                }

                ResPackage.InitializedDependenciesPackage(manifests);
                UILoading.SetTitle(CoreAPI.Language.Query(CommonLanguage.ResLoadComplete));
                UILoading.SetProgress(1);
                return Status.Success;
#if UNITY_EDITOR
            }
#endif
        }

        internal static Status LoadingAssetBundleSync(ResourcePackageManifest manifest)
        {
            if (ResPackage.TryGetValue(manifest.name, out ResPackage _))
            {
                Debug.Log("资源包已加载：" + manifest.name);
                return Status.Success;
            }

            byte[] bytes = CoreAPI.VFS.Read(manifest.name);
            if (bytes == null)
            {
                return Status.Fail;
            }

            Debug.Log("资源包加载完成：" + manifest.name);
            return ResPackage.Create(AssetBundle.LoadFromMemory(bytes)).IsSuccess() ? Status.Success : Status.Fail;
        }

        public void Release()
        {
        }
    }

    class LoadingResPackageAsyncCommand : ICommandHandlerAsync<Status>
    {
        public async UniTask<Status> OnExecute(params object[] args)
        {
            ResourcePackageManifest[] manifests = args.Cast<ResourcePackageManifest>().ToArray();
            if (manifests is null || manifests.Length == 0)
            {
                return Status.Success;
            }

#if UNITY_EDITOR
            using (ProfileWatcher watcher = ProfileWatcher.StartProfileWatcher(nameof(LoadingResPackageAsyncCommand)))
            {
#endif
                UniTask<Status>[] loadAssetBundleTaskList = new UniTask<Status>[manifests.Length];
                for (int i = 0; i < manifests.Length; i++)
                {
                    loadAssetBundleTaskList[i] = LoadingAssetBundleAsync(manifests[i]);
                }

                var result = await UniTask.WhenAll(loadAssetBundleTaskList);
                if (result.Where(x => x is Status.Fail).Count() > 0)
                {
                    manifests.Select(x => x.name).ToList().ForEach(x => ResPackage.Unload(x));
                    return Status.Fail;
                }

                ResPackage.InitializedDependenciesPackage(manifests);
                UILoading.SetTitle(CoreAPI.Language.Query(CommonLanguage.ResLoadComplete));
                UILoading.SetProgress(1);
                return Status.Success;
#if UNITY_EDITOR
            }
#endif
        }

        internal static async UniTask<Status> LoadingAssetBundleAsync(ResourcePackageManifest manifest)
        {
            if (ResPackage.TryGetValue(manifest.name, out ResPackage _))
            {
                CoreAPI.Logger.Log("资源包已加载：" + manifest.name);
                return Status.Success;
            }

            AssetBundle bundle = default;
#if UNITY_WEBGL
            using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(ResConfig.instance.GetFilePath(manifest.name), manifest.version))
            {
                await request.SendWebRequest();
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    return Status.Fail;
                }

                bundle = DownloadHandlerAssetBundle.GetContent(request);
            }
#else
            byte[] bytes = await CoreAPI.VFS.ReadAsync(manifest.name);
            if (bytes == null || bytes.Length == 0)
            {
                Debug.Log("资源包不存在：" + manifest.name);
                return Status.Fail;
            }

            bundle = await AssetBundle.LoadFromMemoryAsync(bytes);
#endif
            Debug.Log("资源包加载完成：" + manifest.name);
            return ResPackage.Create(bundle).IsSuccess() ? Status.Success : Status.Fail;
        }


        public void Release()
        {
        }
    }
}