using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public class MultipDownloadHandle
    {
        private Action<float> progressCallback;
        public DownloadData[] DownloadDatas { get; private set; }
        private DownloadHandle[] _handles;

        public static async UniTask<DownloadData[]> Download(Action<float> progressCallback, params string[] args)
        {
            MultipDownloadHandle multipDownloadHandle = new MultipDownloadHandle();
            multipDownloadHandle.progressCallback = progressCallback;
            multipDownloadHandle.DownloadDatas = new DownloadData[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                DownloadData downloadData = new DownloadData();
                downloadData.name = Path.GetFileName(args[i]);
                downloadData.url = args[i];
                downloadData.isDone = false;
                downloadData.bytes = null;
                downloadData.crc = Crc32.GetCRC32Str(downloadData.name);
                multipDownloadHandle.DownloadDatas[i] = downloadData;
            }

            await multipDownloadHandle.Start();
            return multipDownloadHandle.DownloadDatas;
        }

        private void OnProgress()
        {
            if (progressCallback is not null)
            {
                progressCallback(GetProgress());
            }
        }

        private float GetProgress()
        {
            return _handles.Sum(x => x.progress) / _handles.Length;
        }

        public async UniTask Start()
        {
            UniTask.Run(OnProgress);
            _handles = new DownloadHandle[DownloadDatas.Length];
            for (int i = 0; i < DownloadDatas.Length; i++)
            {
                DownloadData downloadData = DownloadDatas[i];
                _handles[i] = new DownloadHandle(downloadData);
            }

            await UniTask.WhenAll(_handles.Select(x => x.OnStart()));
            //释放downloadhandle
            for (int i = 0; i < _handles.Length; i++)
            {
                _handles[i].Dispose();
            }
        }

        class DownloadHandle : IDisposable, IProgress<float>
        {
            private DownloadData data;
            public float progress;

            public DownloadHandle(DownloadData data)
            {
                this.data = data;
            }

            public async UniTask OnStart()
            {
                string etag = await NetworkRequest.Head(data.url, "ETag");
                data.crc = Crc32.GetCRC32Str(etag);
                UnityWebRequest request = UnityWebRequest.Get(data.url);
                request.timeout = 5;
                request.useHttpContinue = true;
                await request.SendWebRequest().ToUniTask(this);
                progress = 1;
                data.isDone = request.isDone;
                if (request.isDone)
                {
                    data.bytes = request.downloadHandler.data;
                }

                request.Dispose();
            }

            public void Dispose()
            {
                data = null;
                progress = 0;
            }

            public void Report(float value)
            {
                progress = value;
            }
        }

        public class DownloadData : IDisposable
        {
            public string name;
            public string url;
            public bool isDone;
            public byte[] bytes;
            public uint crc;

            public void Dispose()
            {
                // TODO 在此释放托管资源
            }
        }
    }
}