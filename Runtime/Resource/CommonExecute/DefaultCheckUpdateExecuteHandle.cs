using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZEngine.Network;
using ZEngine.Window;

namespace ZEngine.Resource
{
    public interface ICheckResourceUpdateExecuteHandle : IExecuteHandle<ICheckResourceUpdateExecuteHandle>
    {
        ModuleOptions[] options { get; }
        RuntimeBundleManifest[] bundles { get; }

        void SubscribeProgressChange(ISubscriber<float> subscribe);

        internal static ICheckResourceUpdateExecuteHandle Create(params ModuleOptions[] options)
        {
            InternalCheckResourceUpdateExecuteHandle internalCheckResourceUpdateExecuteHandle = Activator.CreateInstance<InternalCheckResourceUpdateExecuteHandle>();
            internalCheckResourceUpdateExecuteHandle.options = options;
            return internalCheckResourceUpdateExecuteHandle;
        }

        class InternalCheckResourceUpdateExecuteHandle : AbstractExecuteHandle, IExecuteHandle<ICheckResourceUpdateExecuteHandle>, ICheckResourceUpdateExecuteHandle
        {
            public ModuleOptions[] options { get; set; }
            public RuntimeBundleManifest[] bundles { get; set; }
            private ISubscriber<float> progressSubsceibeHandle;

            class UpdateItem
            {
                public URLOptions options;
                public RuntimeModuleManifest module;
                public RuntimeBundleManifest bundle;
            }

            public override void Dispose()
            {
                base.Dispose();
                progressSubsceibeHandle?.Dispose();
                progressSubsceibeHandle = null;
                bundles = Array.Empty<RuntimeBundleManifest>();
                options = Array.Empty<ModuleOptions>();
            }

            public void SubscribeProgressChange(ISubscriber<float> subscribe)
            {
                this.progressSubsceibeHandle = subscribe;
            }

            protected override IEnumerator OnExecute()
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
                List<RuntimeModuleManifest> moduleManifests = new List<RuntimeModuleManifest>();
                for (int i = 0; i < options.Length; i++)
                {
                    string moduleFilePath = Engine.Custom.GetHotfixPath(options[i].url.address, options[i].moduleName + ".ini");
                    IWebRequestExecuteHandle<RuntimeModuleManifest> webRequestExecuteHandle = Engine.Network.Get<RuntimeModuleManifest>(moduleFilePath);

                    yield return WaitFor.Create(() => webRequestExecuteHandle.status == Status.Success || webRequestExecuteHandle.status == Status.Failed);

                    foreach (var VARIABLE in webRequestExecuteHandle.result.bundleList)
                    {
                        VersionOptions localVersion = Engine.FileSystem.GetFileVersion(VARIABLE.name);
                        if (localVersion is not null && localVersion == VARIABLE.version)
                        {
                            continue;
                        }

                        updateBundleList.Add(new UpdateItem()
                        {
                            options = options[i].url,
                            bundle = VARIABLE,
                            module = webRequestExecuteHandle.result
                        });
                    }

                    moduleManifests.Add(webRequestExecuteHandle.result);
                }

                bundles = updateBundleList.Select(x => x.bundle).ToArray();
                if (updateBundleList.Count is 0)
                {
                    moduleManifests.ForEach(ResourceManager.instance.AddModuleManifest);
                    status = Status.Success;
                    yield break;
                }


                string message = $"检测到有{(updateBundleList.Sum(x => x.bundle.length) / 1024f / 1024f).ToString("N")} MB资源更新，是否更新资源";
                MsgBox msgBox = Engine.Window.MsgBox(message);
                yield return msgBox.GetCoroutine();
                if (msgBox.result.Equals(false))
                {
                    status = Status.Success;
                    Engine.Custom.Quit();
                    yield break;
                }

                IEnumerable<DownloadOptions> optionsList = updateBundleList.Select(x => new DownloadOptions()
                {
                    url = $"{x.options.address}/{Engine.Custom.GetPlatfrom()}/{x.bundle.name}",
                    userData = x,
                    version = x.bundle.version
                });
                IDownloadExecuteHandle downloadExecuteHandle = Engine.Network.Download(optionsList.ToArray());
                downloadExecuteHandle.SubscribeProgressChange(progressSubsceibeHandle);
                yield return WaitFor.Create(() => downloadExecuteHandle.status == Status.Success || downloadExecuteHandle.status == Status.Failed);

                if (downloadExecuteHandle.status is not Status.Success || downloadExecuteHandle.Handles is null || downloadExecuteHandle.Handles.Length is 0)
                {
                    status = Status.Failed;
                    yield break;
                }

                moduleManifests.ForEach(ResourceManager.instance.AddModuleManifest);
                status = Status.Success;
            }
        }
    }
}