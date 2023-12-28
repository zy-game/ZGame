using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public class DownloadHandle : IDisposable, IProgress<float>
    {
        private DownloadData data;
        public float progress;

        public DownloadHandle(DownloadData data)
        {
            this.data = data;
        }

        public async UniTask OnStart()
        {
            string etag = await NetworkManager.Head(data.url, "eTag");
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
}