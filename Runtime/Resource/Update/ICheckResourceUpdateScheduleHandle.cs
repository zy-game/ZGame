using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using ZEngine.Network;
using ZEngine.Window;

namespace ZEngine.Resource
{
    public interface ICheckResourceUpdateScheduleHandle : IScheduleHandle<ICheckResourceUpdateScheduleHandle>
    {
        ModuleOptions[] options { get; }
        GameAssetBundleManifest[] bundles { get; }

        void SubscribeProgressChange(ISubscriber<float> subscribe);

        internal static ICheckResourceUpdateScheduleHandle Create(params ModuleOptions[] options)
        {
            InternalCheckResourceUpdateScheduleHandle internalCheckResourceUpdateScheduleHandle = Activator.CreateInstance<InternalCheckResourceUpdateScheduleHandle>();
            internalCheckResourceUpdateScheduleHandle.options = options;
            return internalCheckResourceUpdateScheduleHandle;
        }

        class InternalCheckResourceUpdateScheduleHandle : ICheckResourceUpdateScheduleHandle
        {
            class UpdateItem
            {
                public URLOptions options;
                public GameResourceModuleManifest module;
                public GameAssetBundleManifest bundle;
            }

            public ModuleOptions[] options { get; set; }
            public GameAssetBundleManifest[] bundles { get; set; }
            private ISubscriber<float> progressSubsceibeHandle;
            public Status status { get; set; }
            public ICheckResourceUpdateScheduleHandle result => this;

            private ISubscriber subscriber;

            public void Subscribe(ISubscriber subscriber)
            {
                if (this.subscriber is null)
                {
                    this.subscriber = subscriber;
                    return;
                }

                this.subscriber.Merge(subscriber);
            }

            public void Execute(params object[] args)
            {
                if (status is not Status.None)
                {
                    return;
                }

                status = Status.Execute;
                DOExecute().StartCoroutine(OnComplate);
            }

            private void OnComplate()
            {
                if (subscriber is not null)
                {
                    subscriber.Execute(this);
                }
            }

            public void Dispose()
            {
                progressSubsceibeHandle?.Dispose();
                progressSubsceibeHandle = null;
                subscriber?.Dispose();
                subscriber = null;
                bundles = Array.Empty<GameAssetBundleManifest>();
                options = Array.Empty<ModuleOptions>();
            }

            public void SubscribeProgressChange(ISubscriber<float> subscribe)
            {
                this.progressSubsceibeHandle = subscribe;
            }

            private IEnumerator DOExecute()
            {
                //todo 如果在编辑器模式下，并且未使用热更模式，直接跳过
#if UNITY_EDITOR
                if (HotfixOptions.instance.useHotfix is Switch.Off || HotfixOptions.instance.useAsset is Switch.Off)
                {
                    status = Status.Success;
                    yield break;
                }
#endif
                if (options is null || options.Length is 0)
                {
                    status = Status.Failed;
                    progressSubsceibeHandle?.Execute(1);
                    yield break;
                }

                List<UpdateItem> updateBundleList = new List<UpdateItem>();
                List<GameResourceModuleManifest> moduleManifests = new List<GameResourceModuleManifest>();
                for (int i = 0; i < options.Length; i++)
                {
                    string moduleFilePath = Engine.GetHotfixPath(options[i].url.address, options[i].moduleName + ".ini");
                    IWebRequestleHandle<GameResourceModuleManifest> webRequestScheduleHandle = Engine.Network.Get<GameResourceModuleManifest>(moduleFilePath);
                    yield return webRequestScheduleHandle.WaitTo();
                    foreach (var VARIABLE in webRequestScheduleHandle.result.bundleList)
                    {
                        int localVersion = Engine.FileSystem.GetFileVersion(VARIABLE.name);
                        if (localVersion == VARIABLE.version)
                        {
                            continue;
                        }

                        updateBundleList.Add(new UpdateItem()
                        {
                            options = options[i].url,
                            bundle = VARIABLE,
                            module = webRequestScheduleHandle.result
                        });
                    }

                    moduleManifests.Add(webRequestScheduleHandle.result);
                    webRequestScheduleHandle.Dispose();
                }

                bundles = updateBundleList.Select(x => x.bundle).ToArray();
                if (updateBundleList.Count is 0)
                {
                    moduleManifests.ForEach(ResourceManager.instance.AddModuleManifest);
                    progressSubsceibeHandle?.Execute(1);
                    status = Status.Success;
                    yield break;
                }

                float size = updateBundleList.Sum(x => x.bundle.length) / 1024f / 1024f;
                string message = $"检测到有{size.ToString("N")} MB资源更新，是否更新资源?";
                MsgBox msgBox = Engine.Window.MsgBox(message);
                yield return msgBox.GetCoroutine();
                if (msgBox.result.Equals(false))
                {
                    progressSubsceibeHandle?.Execute(1);
                    status = Status.Success;
                    Engine.Quit();
                    yield break;
                }

                IEnumerable<DownloadOptions> optionsList = updateBundleList.Select(x => new DownloadOptions()
                {
                    url = $"{x.options.address}/{Engine.GetPlatfrom()}/{x.bundle.name}",
                    userData = x,
                    version = x.bundle.version
                });
                IDownloadHandle downloadScheduleHandle = Engine.Network.Download(optionsList.ToArray());
                downloadScheduleHandle.SubscribeProgressChange(progressSubsceibeHandle);
                yield return new WaitUntil(() => downloadScheduleHandle.status == Status.Success || downloadScheduleHandle.status == Status.Failed);

                if (downloadScheduleHandle.status is not Status.Success || downloadScheduleHandle.Handles is null || downloadScheduleHandle.Handles.Length is 0)
                {
                    downloadScheduleHandle.Dispose();
                    progressSubsceibeHandle?.Execute(1);
                    status = Status.Failed;
                    Engine.Console.Log(status);
                    yield break;
                }

                downloadScheduleHandle.Dispose();
                moduleManifests.ForEach(ResourceManager.instance.AddModuleManifest);
                progressSubsceibeHandle?.Execute(1);
                status = Status.Success;
                Engine.Console.Log(status);
            }
        }
    }
}