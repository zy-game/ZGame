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
    }

    class DefaultCheckResourceUpdateExecuteHandle : AbstractExecuteHandle, IExecuteHandle<ICheckResourceUpdateExecuteHandle>, ICheckResourceUpdateExecuteHandle
    {
        public ModuleOptions[] options { get; set; }
        public RuntimeBundleManifest[] bundles { get; set; }

        class UpdateItem
        {
            public URLOptions options;
            public RuntimeModuleManifest module;
            public RuntimeBundleManifest bundle;
        }

        protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
        {
            options = paramsList.Cast<ModuleOptions>().ToArray();
            //todo 如果在编辑器模式下，并且未使用热更模式，直接跳过
            if (options is null || options.Length is 0)
            {
                status = Status.Failed;
                OnProgress(1);
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
            MsgBox msgBox = Engine.Window.MsgBox(message, () => { }, Engine.Custom.Quit);
            yield return msgBox.GetCoroutine();
            if (msgBox.result.Equals(false))
            {
                status = Status.Success;
                yield break;
            }

            IEnumerable<DownloadOptions> optionsList = updateBundleList.Select(x => new DownloadOptions()
            {
                url = $"{x.options.address}/{Engine.Custom.GetPlatfrom()}/{x.bundle.name}",
                userData = x,
                version = x.bundle.version
            });
            IDownloadExecuteHandle downloadExecuteHandle = Engine.Network.Download(optionsList.ToArray());
            downloadExecuteHandle.Subscribe(IProgressSubscribeHandle.Create(OnProgress));
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