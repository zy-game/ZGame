using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZEngine.Network;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    class DefaultUpdateResourceExecuteHandle : IUpdateResourceExecuteHandle
    {
        public Status status { get; set; }
        public float progress { get; set; }
        public RuntimeBundleManifest[] bundles { get; set; }

        private URLOptions options;
        private ISubscribeHandle<float> progressHandle;
        private List<ISubscribeHandle> _subscribeExecuteHandles = new List<ISubscribeHandle>();

        public void Release()
        {
        }

        public void Subscribe(ISubscribeHandle subscribe)
        {
            _subscribeExecuteHandles.Add(subscribe);
        }

        public IEnumerator ExecuteComplete()
        {
            yield return WaitFor.Create(() => status == Status.Failed || status == Status.Success);
        }

        public void OnPorgressChange(ISubscribeHandle<float> subscribe)
        {
            progressHandle = subscribe;
        }

        public void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            options = (URLOptions)paramsList[0];
            bundles = (RuntimeBundleManifest[])paramsList[1];
            OnStart().StartCoroutine();
        }

        private IEnumerator OnStart()
        {
            MultiDownloadOptions multiDownloadOptions = new MultiDownloadOptions();
            for (int i = 0; i < bundles.Length; i++)
            {
                multiDownloadOptions.AddUrl($"{options.address}/{Engine.Custom.GetPlatfrom()}/{bundles[i].name}");
            }

            INetworkMultiDownloadExecuteHandle networkMultiDownloadExecuteHandle = Engine.Network.MultiDownload(multiDownloadOptions);
            yield return networkMultiDownloadExecuteHandle.ExecuteComplete();
            status = Status.Success;
            _subscribeExecuteHandles.ForEach(x => x.Execute(this));
        }
    }
}