using System;
using System.Collections;
using System.Linq;

namespace ZEngine.Network
{
    public interface IDownloadHandle : IScheduleHandle<IDownloadHandle>
    {
        HttpDownloadHandle[] Handles { get; }
        DownloadOptions[] options { get; }

        void SubscribeProgressChange(ISubscriber<float> subscribe);

        internal static IDownloadHandle Create(params DownloadOptions[] options)
        {
            InternalDownloadScheduleHandle internalDownloadScheduleHandle = Activator.CreateInstance<InternalDownloadScheduleHandle>();
            internalDownloadScheduleHandle.options = options;
            return internalDownloadScheduleHandle;
        }

        class InternalDownloadScheduleHandle : IDownloadHandle
        {
            public HttpDownloadHandle[] Handles { get; set; }
            public DownloadOptions[] options { get; set; }
            public Status status { get; set; }
            public IDownloadHandle result => this;

            private ISubscriber<float> progerss;
            private ISubscriber subscriber;

            private IEnumerator DOExecute()
            {
                while (true)
                {
                    this.progerss?.Execute(Handles.Sum(x => x.progress) / (float)Handles.Length);
                    if (Handles.Where(x => x.IsComplete() is false).Count() <= 0)
                    {
                        break;
                    }

                    yield return WaitFor.Create(0.05f);
                }

                status = Handles.Where(x => x.status == Status.Failed).Count() > 0 ? Status.Failed : Status.Success;
                Engine.Console.Log("download ", status);
            }

            public void SubscribeProgressChange(ISubscriber<float> subscribe)
            {
                this.progerss = subscribe;
            }

            public void Execute(params object[] args)
            {
                if (status is not Status.None)
                {
                    return;
                }

                status = Status.Execute;
                Handles = new HttpDownloadHandle[options.Length];
                for (int i = 0; i < options.Length; i++)
                {
                    DownloadOptions downloadOptions = options[i];
                    Handles[i] = HttpDownloadHandle.Create(downloadOptions.url, i, downloadOptions.version);
                    Handles[i].OnStart();
                }

                DOExecute().StartCoroutine(OnComplate);
            }

            private void OnComplate()
            {
                if (subscriber is not null)
                {
                    subscriber.Execute(this);
                }

            }

            public void Dispose()
            {
                foreach (HttpDownloadHandle downloadHandle in Handles)
                {
                    downloadHandle.Dispose();
                }

                status = Status.None;
                progerss?.Dispose();
                progerss = null;
                subscriber?.Dispose();
                subscriber = null;
                Handles = Array.Empty<HttpDownloadHandle>();
                GC.SuppressFinalize(this);
            }

            public void Subscribe(ISubscriber subscriber)
            {
                if (this.subscriber is null)
                {
                    this.subscriber = this.subscriber;
                    return;
                }

                this.subscriber.Merge(subscriber);
            }
        }
    }
}