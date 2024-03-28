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
        class CertificateController : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }

        public static void SetRequestHeaderWithNotCors(UnityWebRequest request, Dictionary<string, object> headers)
        {
            if (request == null)
            {
                return;
            }

            request.certificateHandler = new CertificateController();
            request.useHttpContinue = true;
            if (headers is null || headers.Count == 0)
            {
                return;
            }

            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value.ToString());
            }
        }

        public static void SetRequestHeaderWithCors(UnityWebRequest request, Dictionary<string, object> headers)
        {
            if (request == null)
            {
                return;
            }

            request.SetRequestHeader("Access-Control-Allow-Headers", "Content-Type");
            request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
            SetRequestHeaderWithNotCors(request, headers);
        }

        public static T GetResultData<T>(UnityWebRequest request)
        {
            object _data = default;
            if (typeof(T) == typeof(string))
            {
                _data = request.downloadHandler.text;
            }
            else if (typeof(T) == typeof(byte[]))
            {
                _data = request.downloadHandler.data;
            }
            else if (typeof(T) is JObject)
            {
                _data = JObject.Parse(request.downloadHandler.text);
            }
            else
            {
                _data = JsonConvert.DeserializeObject<T>(request.downloadHandler.text);
            }

            return (T)_data;
        }

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