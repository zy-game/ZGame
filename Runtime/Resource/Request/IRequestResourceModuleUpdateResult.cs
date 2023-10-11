using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using ZEngine.Network;
using ZEngine.Window;

namespace ZEngine.Resource
{
    public interface IRequestResourceModuleUpdateResult : IDisposable
    {
        Status status { get; }
        ModuleOptions[] options { get; }
        GameAssetBundleManifest[] bundles { get; }

        internal static UniTask<IRequestResourceModuleUpdateResult> Create(IGameProgressHandle gameProgressHandle, params ModuleOptions[] options)
        {
            UniTaskCompletionSource<IRequestResourceModuleUpdateResult> uniTaskCompletionSource = new UniTaskCompletionSource<IRequestResourceModuleUpdateResult>();
            InternalRequestResourceModuleUpdateResult internalRequestResourceModuleUpdateResult = Activator.CreateInstance<InternalRequestResourceModuleUpdateResult>();
            internalRequestResourceModuleUpdateResult.taskCompletionSource = uniTaskCompletionSource;
            internalRequestResourceModuleUpdateResult.gameProgressHandle = gameProgressHandle;
            internalRequestResourceModuleUpdateResult.options = options;
            internalRequestResourceModuleUpdateResult.Execute();
            return uniTaskCompletionSource.Task;
        }

        class InternalRequestResourceModuleUpdateResult : IRequestResourceModuleUpdateResult
        {
            class UpdateItem
            {
                public URLOptions options;
                public GameResourceModuleManifest module;
                public GameAssetBundleManifest bundle;
            }

            public Status status { get; set; }
            public ModuleOptions[] options { get; set; }
            public GameAssetBundleManifest[] bundles { get; set; }
            public IGameProgressHandle gameProgressHandle;
            public UniTaskCompletionSource<IRequestResourceModuleUpdateResult> taskCompletionSource;

            public async void Execute()
            {
                //todo 如果在编辑器模式下，并且未使用热更模式，直接跳过
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset is Switch.Off)
                {
                    OnComplate(Status.Success);
                    return;
                }
#endif
                if (options is null || options.Length is 0)
                {
                    OnComplate(Status.Failed);
                    return;
                }

                List<UpdateItem> updateBundleList = new List<UpdateItem>();
                List<GameResourceModuleManifest> moduleManifests = new List<GameResourceModuleManifest>();
                for (int i = 0; i < options.Length; i++)
                {
                    string moduleFilePath = Launche.GetHotfixPath(options[i].url.address, options[i].moduleName + ".ini");
                    IWebRequestResult<GameResourceModuleManifest> webRequestResult = await Launche.Network.Get<GameResourceModuleManifest>(moduleFilePath);
                    foreach (var VARIABLE in webRequestResult.result.bundleList)
                    {
                        int localVersion = Launche.FileSystem.GetFileVersion(VARIABLE.name);
                        if (localVersion == VARIABLE.version)
                        {
                            continue;
                        }

                        updateBundleList.Add(new UpdateItem()
                        {
                            options = options[i].url,
                            bundle = VARIABLE,
                            module = webRequestResult.result
                        });
                    }

                    moduleManifests.Add(webRequestResult.result);
                    webRequestResult.Dispose();
                }

                bundles = updateBundleList.Select(x => x.bundle).ToArray();
                if (updateBundleList.Count is 0)
                {
                    moduleManifests.ForEach(ResourceManager.instance.AddModuleManifest);
                    OnComplate(Status.Success);
                    return;
                }

                float size = updateBundleList.Sum(x => x.bundle.length) / 1024f / 1024f;
                string message = $"检测到有{size.ToString("N")} MB资源更新，是否更新资源?";
                if (await Launche.Window.MsgBoxAsync(message) is false)
                {
                    OnComplate(Status.Failed);
                    return;
                }

                IEnumerable<DownloadOptions> optionsList = updateBundleList.Select(x => new DownloadOptions()
                {
                    url = $"{x.options.address}/{Launche.GetPlatfrom()}/{x.bundle.name}",
                    userData = x,
                    version = x.bundle.version
                });
                IDownloadResult downloadResult = await Launche.Network.Download(gameProgressHandle, optionsList.ToArray());

                if (downloadResult.status is not Status.Success || downloadResult.Handles is null || downloadResult.Handles.Length is 0)
                {
                    downloadResult.Dispose();
                    OnComplate(Status.Failed);
                    Launche.Console.Log(status);
                    return;
                }

                downloadResult.Dispose();
                moduleManifests.ForEach(ResourceManager.instance.AddModuleManifest);
                OnComplate(Status.Success);
                Launche.Console.Log(status);
            }

            private void OnComplate(Status status)
            {
                this.status = status;
                gameProgressHandle.SetTextInfo("检查完成");
                gameProgressHandle.SetProgress(1);
                taskCompletionSource.TrySetResult(this);
            }

            public void Dispose()
            {
                gameProgressHandle = null;
                bundles = Array.Empty<GameAssetBundleManifest>();
                options = Array.Empty<ModuleOptions>();
            }
        }
    }
}