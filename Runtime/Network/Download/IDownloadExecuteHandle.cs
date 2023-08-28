using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZEngine.Network
{
    public interface IDownloadExecuteHandle : IExecuteHandle<IDownloadExecuteHandle>
    {
        DownloadHandle[] Handles { get; }
    }


    class DefaultDownloadExecuteHandle : AbstractExecuteHandle, IExecuteHandle<IDownloadExecuteHandle>, IDownloadExecuteHandle
    {
        public DownloadHandle[] Handles { get; set; }

        public override void Release()
        {
            foreach (DownloadHandle downloadHandle in Handles)
            {
                Engine.Class.Release(downloadHandle);
            }

            Handles = Array.Empty<DownloadHandle>();
        }

        protected override IEnumerator ExecuteCoroutine(params object[] paramsList)
        {
            Handles = new DownloadHandle[paramsList.Length];
            for (int i = 0; i < paramsList.Length; i++)
            {
                DownloadOptions downloadOptions = (DownloadOptions)paramsList[i];
                Handles[i] = DownloadHandle.Create(downloadOptions.url, i, downloadOptions.version);
                Handles[i].OnStart();
            }

            yield return WaitFor.Create(() =>
            {
                OnProgress(Handles.Sum(x => x.progress) / (float)Handles.Length);
                return Handles.Where(x => x.IsComplete() is false).Count() is 0;
            });

            status = Handles.Where(x => x.status == Status.Failed).Count() > 0 ? Status.Failed : Status.Success;
        }
    }
}