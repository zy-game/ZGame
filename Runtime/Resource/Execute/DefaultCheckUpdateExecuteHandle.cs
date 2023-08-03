using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ZEngine.Network;

namespace ZEngine.Resource
{
    class DefaultCheckUpdateExecuteHandle : IExecuteHandle<ICheckUpdateExecuteHandle>, ICheckUpdateExecuteHandle
    {
        public ulong length { get; set; }
        public float progress { get; set; }
        public string[] files { get; set; }
        public Status status { get; set; }


        private IUpdateResourceDialogExecuteHandle updateResourceDialogExecuteHandle;
        private List<ISubscribeExecuteHandle> completeSubscribe = new List<ISubscribeExecuteHandle>();
        private List<ISubscribeExecuteHandle<float>> progressListener = new List<ISubscribeExecuteHandle<float>>();

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            completeSubscribe.Add(subscribe);
        }

        public void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe)
        {
            progressListener.Add(subscribe);
        }

        public void OnUpdateDialog(IUpdateResourceDialogExecuteHandle dialogExecuteHandle)
        {
            updateResourceDialogExecuteHandle = dialogExecuteHandle;
        }

        public void Release()
        {
        }

        public void Execute(params object[] paramsList)
        {
            //todo 如果在编辑器模式下，并且未使用热更模式，直接跳过
            ResourceUpdateOptions options = (ResourceUpdateOptions)paramsList[0];
            if (options is null || options.url is null || options.url.state == Switch.Off)
            {
                status = Status.Failed;
                progressListener.ForEach(x => x.Execute(1));
                completeSubscribe.ForEach(x => x.Execute(this));
                return;
            }

            status = Status.Execute;
            OnStartCheckUpdate(options).StartCoroutine();
        }

        private IEnumerator OnStartCheckUpdate(ResourceUpdateOptions options)
        {
            INetworkRequestExecuteHandle<RuntimeModuleManifest> networkRequestExecuteHandle = Engine.Network.Get<RuntimeModuleManifest>($"{options.url.address}/{options.moduleName}");
            yield return networkRequestExecuteHandle.Complete();
            RuntimeModuleManifest remoteModuleManifest = networkRequestExecuteHandle.result;
            RuntimeModuleManifest localModuleManifest = ResourceManager.instance.GetModuleManifest(options.moduleName);
            List<RuntimeBundleManifest> updateList = new List<RuntimeBundleManifest>();
            if (localModuleManifest is null || localModuleManifest.version != remoteModuleManifest.version)
            {
                updateList.AddRange(remoteModuleManifest.bundleList);
            }
            else
            {
                for (int i = 0; i < remoteModuleManifest.bundleList.Count; i++)
                {
                    RuntimeBundleManifest manifest = localModuleManifest.bundleList.Find(x => x.Equals(remoteModuleManifest.bundleList[i]));
                    if (manifest is not null)
                    {
                        continue;
                    }

                    updateList.Add(manifest);
                }
            }

            updateResourceDialogExecuteHandle.Execute(updateList);
            yield return updateResourceDialogExecuteHandle.Complete();

            if (updateResourceDialogExecuteHandle.isUpdate == Switch.Off)
            {
                yield break;
            }

            List<DownloadItem> downloadItems = new List<DownloadItem>();
            //todo 下载资源
            foreach (var VARIABLE in updateList)
            {
                DownloadItem item = new DownloadItem() { manifest = VARIABLE };
                downloadItems.Add(item);
                OnStartDownloadBundle(item).StartCoroutine();
            }
        }

        class DownloadItem
        {
            public Status status;
            public RuntimeBundleManifest manifest;
        }

        private IEnumerator OnStartDownloadBundle(DownloadItem item)
        {
            yield break;
        }
    }
}