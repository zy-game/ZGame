using System;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.Window;

namespace ZGame.Resource
{
    class DefaultNetworkResourceLoadingHandle : IResourceLoadingHandle
    {
        private string handleName = "NETWORK_RESOURCES";

        public DefaultNetworkResourceLoadingHandle()
        {
            ResourceManager.instance.AddResourcePackageHandle(new ResPackageHandle(handleName, true));
        }

        public bool Contains(string path)
        {
            if (path.StartsWith("http"))
            {
                return true;
            }

            return false;
        }

        public void Dispose()
        {
            ResourceManager.instance.RemoveResourcePackageHandle(handleName);
            GC.SuppressFinalize(this);
        }

        public ResHandle LoadAsset(string path)
        {
            throw new NotImplementedException();
        }

        public async UniTask<ResHandle> LoadAssetAsync(string path)
        {
            if (path.StartsWith("http") is false)
            {
                return default;
            }

            ResPackageHandle _handle = ResourceManager.instance.GetResourcePackageHandle(handleName);
            if (_handle is null)
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
                    await request.SendWebRequest().ToUniTask();
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
                    await request.SendWebRequest().ToUniTask();
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        return default;
                    }

                    asset = DownloadHandlerAudioClip.GetContent(request);
                    break;
                case ".txt":
                case ".json":
                    request = UnityWebRequest.Get(path);
                    await request.SendWebRequest().ToUniTask();
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        return default;
                    }

                    asset = new TextAsset(request.downloadHandler.text);
                    break;
            }

            _handle.Setup(resHandle = ResHandle.OnCreate(_handle, asset, path));
            return resHandle;
        }

        public void Release(string handle)
        {
        }
    }
}