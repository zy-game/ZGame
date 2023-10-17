using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using ZEngine.Network;
using ZEngine.VFS;

namespace ZEngine.Resource
{
    public interface IRequestNetworkResourceBundleResult : IRequestResourceBundleResult
    {
        public static UniTask<IRequestNetworkResourceBundleResult> Create(IProgressHandle progressHandle, params string[] args)
        {
            UniTaskCompletionSource<IRequestNetworkResourceBundleResult> uniTaskCompletionSource = new UniTaskCompletionSource<IRequestNetworkResourceBundleResult>();
            RequestNetworkResourceBundleHandle requestNetworkResourceBundleHandle = new RequestNetworkResourceBundleHandle();
            requestNetworkResourceBundleHandle.Execute(progressHandle, uniTaskCompletionSource, args);
            return uniTaskCompletionSource.Task;
        }

        class RequestNetworkResourceBundleHandle : IRequestNetworkResourceBundleResult
        {
            public string name { get; set; }
            public string module { get; set; }
            public int version { get; set; }
            public Status status { get; set; }
            public UniTaskCompletionSource<IRequestNetworkResourceBundleResult> completionSource;
            public string[] urlList;

            public async void Execute(IProgressHandle progressHandle, UniTaskCompletionSource<IRequestNetworkResourceBundleResult> uniTaskCompletionSource, params string[] args)
            {
                this.urlList = args;
                this.completionSource = uniTaskCompletionSource;
                DownloadOptions[] options = args.Select(x => new DownloadOptions() { url = x, version = 0 }).ToArray();
                IDownloadResult download = await ZGame.Network.Download(progressHandle, options);
                foreach (var VARIABLE in download.Handles)
                {
                    AssetBundle assetBundle = await AssetBundle.LoadFromMemoryAsync(VARIABLE.bytes);
                    if (assetBundle is null)
                    {
                        status = Status.Failed;
                        uniTaskCompletionSource.TrySetResult(this);
                        return;
                    }

                    ZGame.Console.Log("Load Asset Bundle:", VARIABLE.url);
                    ZGame.Data.Add(RuntimeAssetBundleHandle.Create(null, assetBundle));
                }

                status = Status.Success;
                uniTaskCompletionSource.TrySetResult(this);
            }

            public void Dispose()
            {
            }
        }
    }
}