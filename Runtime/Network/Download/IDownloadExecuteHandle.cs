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
        DownloadOptions[] options { get; }

        internal static IDownloadExecuteHandle Create(params DownloadOptions[] options)
        {
            InternalDownloadExecuteHandle internalDownloadExecuteHandle = Engine.Class.Loader<InternalDownloadExecuteHandle>();
            internalDownloadExecuteHandle.options = options;
            return internalDownloadExecuteHandle;
        }

        class InternalDownloadExecuteHandle : AbstractExecuteHandle, IExecuteHandle<IDownloadExecuteHandle>, IDownloadExecuteHandle
        {
            public DownloadHandle[] Handles { get; set; }
            public DownloadOptions[] options { get; set; }

            public override void Release()
            {
                foreach (DownloadHandle downloadHandle in Handles)
                {
                    Engine.Class.Release(downloadHandle);
                }

                Handles = Array.Empty<DownloadHandle>();
            }

            protected override IEnumerator ExecuteCoroutine()
            {
                Handles = new DownloadHandle[options.Length];
                for (int i = 0; i < options.Length; i++)
                {
                    DownloadOptions downloadOptions = options[i];
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
}