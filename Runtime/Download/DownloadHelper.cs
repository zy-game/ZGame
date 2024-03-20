using System;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace ZGame.Download
{
    class DownloadHelper
    {
        /// <summary>
        /// 获取资源对象
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static Object GetStreamingAssetObject(string extension, UnityWebRequest request)
        {
            switch (extension)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".tga":
                    return DownloadHandlerTexture.GetContent(request);
                case ".mp3":
                case ".wav":
                case ".ogg":
                    return DownloadHandlerAudioClip.GetContent(request);
                case ".assetbundle":
                    return DownloadHandlerAssetBundle.GetContent(request);
                default:
                    throw new NotSupportedException("不支持的资源类型");
            }
        }

        /// <summary>
        /// 创建资源对象请求
        /// </summary>
        /// <param name="path"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static UnityWebRequest CreateStreamingAssetObjectRequest(string path, string extension)
        {
            switch (extension)
            {
                case ".png":
                case ".jpg":
                case ".jpeg":
                case ".bmp":
                case ".tga":
                    return UnityWebRequestTexture.GetTexture(path);
                case ".mp3":
                    return UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);
                case ".wav":
                    return UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV);
                case ".ogg":
                    return UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS);
                case ".assetbundle":
                    return UnityWebRequestAssetBundle.GetAssetBundle(path);
                case ".byte":
                    return UnityWebRequest.Get(path);
                default:
                    throw new NotSupportedException("不支持的资源类型");
            }
        }
    }
}