using System.Collections;
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

        public void Release()
        {
            throw new System.NotImplementedException();
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

            yield return new WaitUntil(() =>
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

            yield return new WaitUntil(() =>
            {
                int count = writeFileExecuteHandles.Where(x => x.status is not Status.Execute || x.status == Status.None).Count();
                progressHandle?.Execute((float)count / (float)writeFileExecuteHandles.Length);
                return count == 0;
            });
        }

        public void Subscribe(ISubscribeExecuteHandle subscribe)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator Complete()
        {
            throw new System.NotImplementedException();
        }

        public void OnPorgressChange(ISubscribeExecuteHandle<float> subscribe)
        {
            throw new System.NotImplementedException();
        }
    }
}