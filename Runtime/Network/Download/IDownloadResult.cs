using System;
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;

namespace ZEngine.Network
{
    public interface IDownloadResult : IDisposable
    {
        Status status { get; }
        HttpDownloadHandle[] Handles { get; }
        DownloadOptions[] options { get; }

        internal static IDownloadResult Create(IProgressHandle gameProgressHandle, DownloadOptions[] options, UniTaskCompletionSource<IDownloadResult> uniTaskCompletionSource)
        {
            InternalDownloadResult internalDownloadResult = Activator.CreateInstance<InternalDownloadResult>();
            internalDownloadResult.uniTaskCompletionSource = uniTaskCompletionSource;
            internalDownloadResult.gameProgressHandle = gameProgressHandle;
            internalDownloadResult.options = options;
            internalDownloadResult.Execute();
            return internalDownloadResult;
        }

        class InternalDownloadResult : IDownloadResult
        {
            public HttpDownloadHandle[] Handles { get; set; }
            public DownloadOptions[] options { get; set; }
            public Status status { get; set; }
            public IProgressHandle gameProgressHandle;
            public UniTaskCompletionSource<IDownloadResult> uniTaskCompletionSource;

            private IEnumerator DOExecute()
            {
                while (true)
                {
                    this.gameProgressHandle?.SetProgress(Handles.Sum(x => x.progress) / (float)Handles.Length);
                    if (Handles.Where(x => x.IsComplete() is false).Count() <= 0)
                    {
                        break;
                    }

                    yield return WaitFor.Create(0.05f);
                }

                status = Handles.Where(x => x.status == Status.Failed).Count() > 0 ? Status.Failed : Status.Success;
                ZGame.Console.Log("download ", status);
                uniTaskCompletionSource.TrySetResult(this);
            }

            public void Execute()
            {
                if (status is not Status.None)
                {
                    return;
                }

                Handles = new HttpDownloadHandle[options.Length];
                for (int i = 0; i < options.Length; i++)
                {
                    DownloadOptions downloadOptions = options[i];
                    Handles[i] = HttpDownloadHandle.Create(downloadOptions.url, i, downloadOptions.version);
                    Handles[i].OnStart();
                }

                DOExecute().StartCoroutine();
            }

            public void Dispose()
            {
                foreach (HttpDownloadHandle downloadHandle in Handles)
                {
                    downloadHandle.Dispose();
                }

                status = Status.None;
                gameProgressHandle = null;
                Handles = Array.Empty<HttpDownloadHandle>();
                GC.SuppressFinalize(this);
            }
        }
    }
}