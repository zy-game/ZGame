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
        private ISubscribeExecuteHandle<float> progressHandle;
        private List<ISubscribeExecuteHandle> _subscribeExecuteHandles = new List<ISubscribeExecuteHandle>();

        public void Release()
        {
        }

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            _subscribeExecuteHandles.Add(subscribe);
        }

        public IEnumerator Complete()
        {
            yield return WaitFor.Create(() => status == Status.Cancel || status == Status.Success);
        }

        public void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe)
        {
            progressHandle = subscribe;
        }

        public IEnumerator Execute(params object[] paramsList)
        {
            status = Status.Execute;
            UpdateOptions options = (UpdateOptions)paramsList[0];
            bundles = (RuntimeBundleManifest[])paramsList[1];

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