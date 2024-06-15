using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

namespace ZGame.Networking
{
    class UnityWebRequestHelper
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
                    throw new NotSupportedException("不支持的资源类型:" + extension);
            }
        }

        /// <summary>
        /// 创建资源对象请求
        /// </summary>
        /// <param name="path"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static UnityWebRequest CreateStreamingAssetObjectRequest(string path, string extension, uint crc)
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
                    if (crc != 0)
                    {
                        return UnityWebRequestAssetBundle.GetAssetBundle(path, crc);
                    }

                    return UnityWebRequestAssetBundle.GetAssetBundle(path);
                default:
                    return UnityWebRequest.Get(path);
            }
        }
    }
}