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
        public RuntimeModuleManifest manifest { get; set; }
        public RuntimeBundleManifest[] bundles { get; private set; }

        private UpdateOptions options;
        private ISubscribeHandle<float> progressListener;
        private List<ISubscribeHandle> completeSubscribe = new List<ISubscribeHandle>();

        public void Subscribe(ISubscribeHandle subscribe)
        {
            completeSubscribe.Add(subscribe);
        }

        public void OnPorgressChange(ISubscribeHandle<float> subscribe)
        {
            progressListener = subscribe;
        }

        public IEnumerator ExecuteComplete()
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

        public void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            options = (UpdateOptions)paramsList[0];
            OnStart().StartCoroutine();
        }

        IEnumerator OnStart()
        {
            //todo 如果在编辑器模式下，并且未使用热更模式，直接跳过
            if (options is null || options.url is null || options.url.state == Switch.Off)
            {
                status = Status.Failed;
                progressListener?.Execute(1);
                completeSubscribe.ForEach(x => x.Execute(this));
                yield break;
            }

            string moduleFilePath = Engine.Custom.GetHotfixPath(options.url.address, options.moduleName + ".ini");
            INetworkRequestExecuteHandle<RuntimeModuleManifest> networkRequestExecuteHandle = Engine.Network.Get<RuntimeModuleManifest>(moduleFilePath);
            yield return networkRequestExecuteHandle.ExecuteComplete();
            Engine.Console.Log(Engine.Json.ToJson(networkRequestExecuteHandle.result));
            manifest = networkRequestExecuteHandle.result;
            bundles = ResourceManager.instance.GetDifferenceBundleManifest(manifest);
            status = Status.Success;
            completeSubscribe.ForEach(x => x.Execute(this));
        }
    }
}