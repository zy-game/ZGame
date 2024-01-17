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
            PackageHandleCache.instance.Add(new PackageHandle(handleName, true));
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
            PackageHandleCache.instance.Remove(handleName);
            GC.SuppressFinalize(this);
        }

        public ResObject LoadAsset(string path)
        {
            throw new NotImplementedException();
        }

        public async UniTask<ResObject> LoadAssetAsync(string path)
        {
            if (path.StartsWith("http") is false)
            {
                return default;
            }

            if (ResObjectCache.instance.TryGetValue(handleName, path, out ResObject handle))
            {
                Debug.Log("缓存资源");
                return handle;
            }

            if (PackageHandleCache.instance.TryGetValue(handleName, out var _handle) is false)
            {
                return default;
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
                    request.useHttpContinue = true;
                    request.disposeUploadHandlerOnDispose = true;
                    request.disposeDownloadHandlerOnDispose = true;
                    await request.SendWebRequest().ToUniTask();
                    if (request.result is UnityWebRequest.Result.Success)
                    {
                        asset = DownloadHandlerTexture.GetContent(request);
                    }

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
                    request.useHttpContinue = true;
                    request.disposeUploadHandlerOnDispose = true;
                    request.disposeDownloadHandlerOnDispose = true;
                    await request.SendWebRequest().ToUniTask();
                    if (request.result is UnityWebRequest.Result.Success)
                    {
                        asset = DownloadHandlerAudioClip.GetContent(request);
                    }

                    break;
                case ".txt":
                case ".json":
                    request = UnityWebRequest.Get(path);
                    request.useHttpContinue = true;
                    request.disposeUploadHandlerOnDispose = true;
                    request.disposeDownloadHandlerOnDispose = true;
                    await request.SendWebRequest().ToUniTask();
                    if (request.result is UnityWebRequest.Result.Success)
                    {
                        asset = new TextAsset(request.downloadHandler.text);
                    }

                    break;
            }

            request.Dispose();
            return ResObject.OnCreate(_handle, asset, path);
        }

        public void Release(string handle)
        {
        }
    }
}