using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZEngine.Network
{
    public interface IDownloadExecuteHandle : IExecuteHandle<DownloadHandleResult[]>
    {
    }


    class DefaultDownloadExecuteHandle : ExecuteHandle<DownloadHandleResult[]>, IDownloadExecuteHandle
    {
        private DownloadHandle[] handles;


        public override void Release()
        {
            handles = null;
        }

        public override void Execute(params object[] paramsList)
        {
            status = Status.Execute;
            handles = new DownloadHandle[paramsList.Length];
            for (int i = 0; i < paramsList.Length; i++)
            {
                DownloadOptions downloadOptions = (DownloadOptions)paramsList[i];
                handles[i] = DownloadHandle.Create(downloadOptions.url, i, downloadOptions.version);
                handles[i].OnStart();
            }

            this.StartCoroutine(OnStart());
        }

        private IEnumerator OnStart()
        {
            yield return WaitFor.Create(() =>
            {
                OnProgress(handles.Sum(x => x.progress) / (float)handles.Length);
                return handles.Where(x => x.IsComplete() is false).Count() is 0;
            });

            result = new DownloadHandleResult[handles.Length];
            for (int i = 0; i < handles.Length; i++)
            {
                result[i] = handles[i].GetDownloadResult();
            }

            status = handles.Where(x => x.status == Status.Failed).Count() > 0 ? Status.Failed : Status.Success;
            OnComplete();
        }
    }
}