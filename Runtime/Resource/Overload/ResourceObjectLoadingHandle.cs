using System;
using System.Collections.Generic;
using System.IO;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ZGame.Resource
{
    public class ResourceObjectLoadingHandle : IDisposable
    {
        public ResHandle LoadAsset(string path)
        {
            if (path.IsNullOrEmpty())
            {
                throw new ArgumentException("path is null or empty");
            }

            if (path.StartsWith("http"))
            {
                throw new AggregateException("网络资源不能在同步中加载");
            }

            if (path.StartsWith("Resources/"))
            {
                return InternalResourceLoadSync(path);
            }
#if UNITY_EDITOR
            if (GameSeting.current.runtime == RuntimeMode.Editor)
            {
                return EditorResourceLoadSync(path);
            }
#endif
            return AssetBundleResourceLoadSync(path);
        }

        public async UniTask<ResHandle> LoadAssetAsync(string path)
        {
            if (path.IsNullOrEmpty())
            {
                throw new ArgumentException("path is null or empty");
            }

            if (path.StartsWith("http"))
            {
                return await NetworkResourceLoading(path);
            }

            if (path.StartsWith("Resources/"))
            {
                return InternalResourceLoadSync(path);
            }
#if UNITY_EDITOR
            if (GameSeting.current.runtime == RuntimeMode.Editor)
            {
                return EditorResourceLoadSync(path);
            }
#endif
            return await BundleResourceLoadAsync(path);
        }

        private ResHandle AssetBundleResourceLoadSync(string path)
        {
            ResHandle result = default;
            ABHandle bundleHandle = ABManager.instance.GetBundleHandle(path);
            if (bundleHandle is null)
            {
                return default;
            }

            UnityEngine.Object asset = bundleHandle.bundle.LoadAsset(path);
            if (asset == null)
            {
                throw new FileNotFoundException(path);
            }

            bundleHandle.AddRes(result = new ResHandle(bundleHandle, asset, path));
            return result;
        }


        private ResHandle InternalResourceLoadSync(string path)
        {
            path = path.Substring(10);
            ABHandle bundleHandle = ABManager.instance.GetBundleHandle("RESOURCES");
            if (bundleHandle is null)
            {
                bundleHandle = ABManager.instance.Add("RESOURCES");
            }

            ResHandle result = bundleHandle.GetRes(path);
            if (result == null)
            {
                UnityEngine.Object asset = Resources.Load(path);
                if (asset == null)
                {
                    throw new FileNotFoundException(path);
                }

                bundleHandle.AddRes(result = new ResHandle(bundleHandle, asset, path));
            }

            return result;
        }

        private ResHandle EditorResourceLoadSync(string path)
        {
#if UNITY_EDITOR
            if (GameSeting.current.runtime == RuntimeMode.Editor)
            {
                ABHandle bundleHandle = ABManager.instance.GetBundleHandle("EDITOR");
                if (bundleHandle is null)
                {
                    bundleHandle = ABManager.instance.Add("EDITOR");
                }

                ResHandle result = bundleHandle.GetRes(path);
                if (result == null)
                {
                    UnityEngine.Object asset = UnityEditor.AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
                    if (asset is null)
                    {
                        throw new FileNotFoundException(path);
                    }

                    bundleHandle.AddRes(result = new ResHandle(bundleHandle, asset, path));
                }

                return result;
            }
#endif
            return default;
        }


        private async UniTask<ResHandle> BundleResourceLoadAsync(string path)
        {
            ResHandle result = default;
            ABHandle bundleHandle = ABManager.instance.GetBundleHandle(path);
            if (bundleHandle is null)
            {
                return default;
            }

            UnityEngine.Object asset = await bundleHandle.bundle.LoadAssetAsync(path);
            if (asset == null)
            {
                throw new FileNotFoundException(path);
            }

            bundleHandle.AddRes(result = new ResHandle(bundleHandle, asset, path));
            return result;
        }

        private async UniTask<ResHandle> NetworkResourceLoading(string path)
        {
            ABHandle bundleHandle = ABManager.instance.GetBundleHandle("NETWORK");
            if (bundleHandle is null)
            {
                bundleHandle = ABManager.instance.Add("NETWORK");
            }

            ResHandle resHandle = bundleHandle.GetRes(path);
            if (resHandle is not null)
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
                    await request.SendWebRequest();
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
                    await request.SendWebRequest();
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        return default;
                    }

                    asset = DownloadHandlerAudioClip.GetContent(request);
                    break;
                case ".txt":
                case ".json":
                    request = UnityWebRequest.Get(path);
                    await request.SendWebRequest();
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        return default;
                    }

                    asset = new TextAsset(request.downloadHandler.text);
                    break;
            }

            resHandle = new ResHandle(bundleHandle, asset, path);
            bundleHandle.AddRes(resHandle);
            return resHandle;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        class ResourceLoadingHelper
        {
        }

        class AssetBundleLoadingHelper
        {
        }

        class AssetDatabaseLoadingHelper
        {
        }

        class NetworkLoadingHelper
        {
        }
    }
}