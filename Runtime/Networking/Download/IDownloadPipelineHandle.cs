using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public interface IDownloadPipelineHandle : IRequest
    {
        IDownloadData[] datas { get; }

        public static void Create(Action<float> prgress, UniTaskCompletionSource<IDownloadPipelineHandle> taskCompletionSource, params string[] args)
        {
            DownloadPipelineHandle downloadPipelineHandle = new DownloadPipelineHandle();
            if (args is null || args.Length == 0)
            {
                taskCompletionSource.TrySetResult(downloadPipelineHandle);
                return;
            }

            downloadPipelineHandle.progressCallback = prgress;
            downloadPipelineHandle.datas = new IDownloadData[args.Length];
            for (int i = 0; i < downloadPipelineHandle.datas.Length; i++)
            {
                downloadPipelineHandle.datas[i] = IDownloadData.Create(args[i], false, Array.Empty<byte>());
            }

            downloadPipelineHandle.taskCompletionSource = taskCompletionSource;
            downloadPipelineHandle.OnStart();
        }

        class DownloadPipelineHandle : IDownloadPipelineHandle
        {
            public string guid { get; } = ID.New();
            public IError error { get; set; }
            public IDownloadData[] datas { get; set; }
            public Action<float> progressCallback { get; set; }
            public UniTaskCompletionSource<IDownloadPipelineHandle> taskCompletionSource;

            public async void OnStart()
            {
                Behaviour.instance.StartCoroutine(UpdateProgress());
                for (int i = 0; i < datas.Length; i++)
                {
                    Behaviour.instance.StartCoroutine(Download(datas[i]));
                }
            }

            private IEnumerator UpdateProgress()
            {
                while (datas.Where(x => x.isDone == false).Count() > 0)
                {
                    this.progressCallback?.Invoke(datas.Sum(x => x.progress));
                    yield return new WaitForSeconds(1);
                }
            }

            private IEnumerator Download(IDownloadData downloadData)
            {
                UnityWebRequest request = UnityWebRequest.Get(downloadData.url);
                request.SendWebRequest();
                while (!request.isDone)
                {
                    downloadData.progress = request.downloadProgress;
                    yield return new WaitForSeconds(0.01f);
                }

                downloadData.isDone = true;
                downloadData.bytes = request.downloadHandler.data;
            }

            public void Dispose()
            {
            }
        }
    }
}