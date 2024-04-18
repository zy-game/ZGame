using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace ZGame.VFS.Command
{
    public class LoadingStreamingAssetCommand : ICommandHandlerAsync<ResObject>
    {
        public async UniTask<ResObject> OnExecute(params object[] args)
        {
#if UNITY_EDITOR
            using (ProfileWatcher watcher = ProfileWatcher.StartProfileWatcher(nameof(LoadingStreamingAssetCommand)))
            {
#endif
                string url = (string)args[0];
                StreamingAssetType assetType = (StreamingAssetType)args[1];
                string assetName = (string)args[2];

                using (UnityWebRequest request = CreateUnityWebRequest(assetType, url))
                {
                    request.SetRequestHeaderWithCors();
                    await request.SendWebRequest();
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        CoreAPI.Logger.LogError(request.error);
                        return ResObject.DEFAULT;
                    }

                    return GetRequestResObject(request, assetType, assetName);
                }

#if UNITY_EDITOR
            }
#endif
        }

        private ResObject GetRequestResObject(UnityWebRequest request, StreamingAssetType type, string assetName)
        {
            object target = default;
            ResPackage parent = ResPackage.DEFAULT;
            switch (type)
            {
                case StreamingAssetType.Sprite:
                case StreamingAssetType.Texture2D:
                    Texture2D texture2D = DownloadHandlerTexture.GetContent(request);
                    if (type == StreamingAssetType.Sprite)
                    {
                        target = Sprite.Create(texture2D, new Rect(Vector2.zero, new Vector2(texture2D.width, texture2D.height)), Vector2.one / 2);
                    }
                    else
                    {
                        target = texture2D;
                    }

                    break;
                case StreamingAssetType.Audio_MPEG:
                case StreamingAssetType.Audio_WAV:
                    target = DownloadHandlerAudioClip.GetContent(request);
                    break;
                case StreamingAssetType.Briary:
                case StreamingAssetType.Text:
                    target = new TextAsset(request.downloadHandler.text);
                    break;
                case StreamingAssetType.Bundle:
                    if (assetName.IsNullOrEmpty())
                    {
                        throw new NullReferenceException(nameof(assetName));
                    }

                    AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);
                    target = bundle.LoadAsset(assetName);
                    parent = ResPackage.Create(bundle);
                    break;
            }

            return ResObject.Create(parent, target, assetName);
        }

        private UnityWebRequest CreateUnityWebRequest(StreamingAssetType type, string url)
        {
            return type switch
            {
                StreamingAssetType.Sprite => UnityWebRequestTexture.GetTexture(url),
                StreamingAssetType.Texture2D => UnityWebRequestTexture.GetTexture(url),
                StreamingAssetType.Audio_MPEG => UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG),
                StreamingAssetType.Audio_WAV => UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV),
                StreamingAssetType.Bundle => UnityWebRequestAssetBundle.GetAssetBundle(url),
                _ => UnityWebRequest.Get(url)
            };
        }

        public void Release()
        {
        }
    }
}