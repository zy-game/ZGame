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

        private UpdateOptions options;
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
            options = (UpdateOptions)paramsList[0];
            bundles = (RuntimeBundleManifest[])paramsList[1];
            OnStart().StartCoroutine();
        }

        private IEnumerator OnStart()
        {
            INetworkRequestExecuteHandle<byte[]>[] fileDownloadList = new INetworkRequestExecuteHandle<byte[]>[bundles.Length];
            string bundleFilePath = string.Empty;
            for (int i = 0; i < bundles.Length; i++)
            {
                bundleFilePath = $"{options.url.address}/{Engine.Custom.GetPlatfrom()}/{bundles[i].name}";
                fileDownloadList[i] = Engine.Network.Get<byte[]>(bundleFilePath);
            }

            yield return WaitFor.Create(() =>
            {
                progressHandle?.Execute(fileDownloadList.Sum(x => x.progress) / (float)fileDownloadList.Length);
                return fileDownloadList.Where(x => x.status == Status.Execute || x.status == Status.None).Count() == 0;
            });
            IWriteFileExecuteHandle[] writeFileExecuteHandles = new IWriteFileExecuteHandle[bundles.Length];
            for (int i = 0; i < fileDownloadList.Length; i++)
            {
                if (fileDownloadList[i].status is not Status.Success)
                {
                    yield break;
                }

                writeFileExecuteHandles[i] = Engine.FileSystem.WriteFileAsync(bundles[i].name, fileDownloadList[i].result, bundles[i].version);
            }

            yield return WaitFor.Create(() =>
            {
                int count = writeFileExecuteHandles.Where(x => x.status is not Status.Execute || x.status == Status.None).Count();
                progressHandle?.Execute((float)count / (float)writeFileExecuteHandles.Length);
                return count == 0;
            });
        }
    }
}