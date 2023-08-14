using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZEngine.Network;
using ZEngine.Window;

namespace ZEngine.Resource
{
    /// <summary>
    /// 检查资源更新
    /// </summary>
    public sealed class CheckUpdateResult
    {
        public UpdateOptions options;
        public RuntimeBundleManifest[] bundles;
    }

    public interface ICheckResourceUpdateExecuteHandle : IExecuteHandle<CheckUpdateResult>
    {
    }

    class DefaultCheckResourceUpdateExecuteHandle : ExecuteHandle<CheckUpdateResult>, ICheckResourceUpdateExecuteHandle
    {
        public override void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            UpdateOptions options = (UpdateOptions)paramsList[0];
            OnStart(options).StartCoroutine();
        }

        IEnumerator OnStart(UpdateOptions options)
        {
            //todo 如果在编辑器模式下，并且未使用热更模式，直接跳过
            if (options is null || options.url is null || options.url.state == Switch.Off)
            {
                status = Status.Failed;
                OnProgress(1);
                OnComplete();
                yield break;
            }

            string moduleFilePath = Engine.Custom.GetHotfixPath(options.url.address, options.moduleName + ".ini");
            IWebRequestExecuteHandle<RuntimeModuleManifest> webRequestExecuteHandle = Engine.Network.Get<RuntimeModuleManifest>(moduleFilePath);
            yield return webRequestExecuteHandle.ExecuteComplete();
            List<RuntimeBundleManifest> updateBundleList = new List<RuntimeBundleManifest>();
            foreach (var VARIABLE in webRequestExecuteHandle.result.bundleList)
            {
                VersionOptions localVersion = Engine.FileSystem.GetFileVersion(VARIABLE.name);
                if (localVersion is not null && localVersion == VARIABLE.version)
                {
                    continue;
                }

                updateBundleList.Add(VARIABLE);
            }

            result = new CheckUpdateResult()
            {
                options = options,
                bundles = updateBundleList.ToArray()
            };

            if (updateBundleList.Count is 0)
            {
                ResourceManager.instance.AddModuleManifest(webRequestExecuteHandle.result);
                status = Status.Success;
                OnComplete();
                yield break;
            }

            string message = $"检测到有{(updateBundleList.Sum(x => x.length) / 1024f / 1024f).ToString("N")} MB资源更新，是否更新资源";
            UI_MsgBox msgBox = Engine.Window.MsgBox(message, () => { }, Engine.Custom.Quit);
            yield return msgBox.GetCoroutine();
            if (msgBox.result.Equals(false))
            {
                status = Status.Success;
                OnComplete();
                yield break;
            }

            IEnumerable<DownloadOptions> optionsList = updateBundleList.Select(x => new DownloadOptions()
            {
                url = $"{options.url.address}/{Engine.Custom.GetPlatfrom()}/{x.name}",
                userData = x,
                version = x.version
            });
            IDownloadExecuteHandle downloadExecuteHandle = Engine.Network.Download(optionsList.ToArray());
            downloadExecuteHandle.OnPorgressChange(ISubscribeHandle.Create<float>(OnProgress));
            yield return downloadExecuteHandle.ExecuteComplete();

            if (downloadExecuteHandle.status is not Status.Success || downloadExecuteHandle.result is null || downloadExecuteHandle.result.Length is 0)
            {
                status = Status.Failed;
                OnComplete();
                yield break;
            }

            ResourceManager.instance.AddModuleManifest(webRequestExecuteHandle.result);
            status = Status.Success;
            OnComplete();
        }
    }
}