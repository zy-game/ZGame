using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZEngine.Network;

namespace ZEngine.Resource
{
    class DefaultCheckUpdateExecuteHandle : IExecuteHandle<ICheckUpdateExecuteHandle>, ICheckUpdateExecuteHandle
    {
        public Status status { get; set; }
        public float progress { get; set; }
        public RuntimeBundleManifest[] bundles { get; private set; }

        private ISubscribeExecuteHandle<float> progressListener;
        private List<ISubscribeExecuteHandle> completeSubscribe = new List<ISubscribeExecuteHandle>();

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            completeSubscribe.Add(subscribe);
        }

        public void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe)
        {
            progressListener = subscribe;
        }

        public IEnumerator Complete()
        {
            return WaitFor.Create(() => status == Status.Failed || status == Status.Success);
        }

        public void Release()
        {
            status = Status.None;
            progress = 0;
            Engine.Class.Release(progressListener);
            completeSubscribe.ForEach(Engine.Class.Release);
            completeSubscribe.Clear();
            progressListener = null;
            bundles = null;
        }

        public IEnumerator Execute(params object[] paramsList)
        {
            //todo 如果在编辑器模式下，并且未使用热更模式，直接跳过
            status = Status.Execute;
            UpdateOptions options = (UpdateOptions)paramsList[0];
            if (options is null || options.url is null || options.url.state == Switch.Off)
            {
                status = Status.Failed;
                progressListener?.Execute(1);
                completeSubscribe.ForEach(x => x.Execute(this));
                yield break;
            }

            string moduleFilePath = $"{options.url.address}/{Engine.Custom.GetPlatfrom()}/{options.moduleName}";
            INetworkRequestExecuteHandle<RuntimeModuleManifest> networkRequestExecuteHandle = Engine.Network.Get<RuntimeModuleManifest>(moduleFilePath);
            yield return networkRequestExecuteHandle.Complete();
            RuntimeModuleManifest compers = ResourceManager.instance.GetModuleManifest(networkRequestExecuteHandle.name);
            bundles = ResourceManager.instance.GetDifferenceBundleManifest(networkRequestExecuteHandle.result, compers);
            completeSubscribe.ForEach(x => x.Execute(this));
            status = Status.Success;
        }
    }
}