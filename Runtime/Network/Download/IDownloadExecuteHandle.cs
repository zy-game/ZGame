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

        void SubscribeProgressChange(ISubscriber<float> subscribe);

        internal static IDownloadExecuteHandle Create(params DownloadOptions[] options)
        {
            InternalDownloadExecuteHandle internalDownloadExecuteHandle = Activator.CreateInstance<InternalDownloadExecuteHandle>();
            internalDownloadExecuteHandle.options = options;
            return internalDownloadExecuteHandle;
        }

        class InternalDownloadExecuteHandle : AbstractExecuteHandle, IExecuteHandle<IDownloadExecuteHandle>, IDownloadExecuteHandle
        {
            public DownloadHandle[] Handles { get; set; }
            public DownloadOptions[] options { get; set; }
            private ISubscriber<float> subscribe;

            public override void Dispose()
            {
                foreach (DownloadHandle downloadHandle in Handles)
                {
                    downloadHandle.Dispose();
                }

                subscribe.Dispose();
                subscribe = null;
                Handles = Array.Empty<DownloadHandle>();
            }

            public void SubscribeProgressChange(ISubscriber<float> subscribe)
            {
                this.subscribe = subscribe;
            }

            protected override IEnumerator OnExecute()
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
                    this.subscribe?.Execute(Handles.Sum(x => x.progress) / (float)Handles.Length);
                    return Handles.Where(x => x.IsComplete() is false).Count() is 0;
                });

                status = Handles.Where(x => x.status == Status.Failed).Count() > 0 ? Status.Failed : Status.Success;
            }
        }
    }
}