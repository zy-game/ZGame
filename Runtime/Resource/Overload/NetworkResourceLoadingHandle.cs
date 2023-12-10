using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.Window;

namespace ZGame.Resource
{
    class NetworkResourceLoadingHandle : IResourceLoadingHandle
    {
        private ResourcePackageHandle _handle;

        public NetworkResourceLoadingHandle()
        {
            _handle = new ResourcePackageHandle("NETWORK_RESOURCES");
        }

        public void Dispose()
        {
            _handle.Dispose();
            _handle = null;
            GC.SuppressFinalize(this);
        }

        public ResHandle LoadAsset(string path)
        {
            throw new NotImplementedException();
        }

        public async UniTask<ResHandle> LoadAssetAsync(string path, ILoadingHandle loadingHandle = null)
        {
            if (path.StartsWith("http") is false)
            {
                return default;
            }

            if (_handle.TryGetValue(path, out ResHandle resHandle))
            {
                return resHandle;
            }

            UnityEngine.Object asset = default;
            string ex = Path.GetExtension(path);
            UnityWebRequest request = default;
            switch (ex)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".tga":
                    request = UnityWebRequestTexture.GetTexture(path);
                    await request.SendWebRequest().ToUniTask(loadingHandle);
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        return default;
                    }

                    asset = DownloadHandlerTexture.GetContent(request);
                    break;
                case ".mp3":
                case ".wav":
                case ".ogg":
                    request = UnityWebRequestMultimedia.GetAudioClip(path, ex switch
                    {
                        ".mp3" => AudioType.MPEG,
                        ".wav" => AudioType.WAV,
                        ".ogg" => AudioType.OGGVORBIS,
                    });
                    await request.SendWebRequest().ToUniTask(loadingHandle);
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        return default;
                    }

                    asset = DownloadHandlerAudioClip.GetContent(request);
                    break;
                case ".txt":
                case ".json":
                    request = UnityWebRequest.Get(path);
                    await request.SendWebRequest().ToUniTask(loadingHandle);
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        return default;
                    }

                    asset = new TextAsset(request.downloadHandler.text);
                    break;
            }

            _handle.Setup(resHandle = new ResHandle(_handle, asset, path));
            return resHandle;
        }

        public bool Release(ResHandle handle)
        {
            if (_handle.Contains(handle))
            {
                return false;
            }

            _handle.Release(handle);
            return true;
        }
    }
}