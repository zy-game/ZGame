using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.FileSystem;
using ZGame.Module;
using Object = UnityEngine.Object;

namespace ZGame.Networking.Download
{
    public class DownloadManager : IModule
    {
        public void OnAwake()
        {
        }

        /// <summary>
        /// 获取网络资源数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async UniTask<Object> GetStreamingAsset(string url, string extension, IProgress<float> callback = null)
        {
            Extension.StartSample();
            Object result = default;
            using (UnityWebRequest request = CreateStreamingAssetObjectRequest(url, extension))
            {
                request.certificateHandler = new WebNet.CertificateController();
                await request.SendWebRequest().ToUniTask(callback);

                if (request.GetResponseHeader("Content-Type") == "application/json")
                {
                    Debug.Log(request.downloadHandler.text);
                }

                if (request.result is UnityWebRequest.Result.Success)
                {
                    result = GetStreamingAssetObject(extension, request);
                }

                Debug.Log($"GET STRWAMING ASSETS:{url} state:{request.result} time:{Extension.GetSampleTime()}");
                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 下载网络文件的二进制数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async UniTask<byte[]> GetStreamingAssetBinary(string url, IProgress<float> callback = null)
        {
            Extension.StartSample();
            byte[] result = default;
            using (UnityWebRequest request = CreateStreamingAssetObjectRequest(url, ".byte"))
            {
                request.certificateHandler = new WebNet.CertificateController();
                await request.SendWebRequest().ToUniTask(callback);
                if (request.GetResponseHeader("Content-Type") == "application/json")
                {
                    Debug.Log(request.downloadHandler.text);
                }

                if (request.result is UnityWebRequest.Result.Success)
                {
                    result = new byte[request.downloadHandler.data.Length];
                    Buffer.BlockCopy(request.downloadHandler.data, 0, result, 0, result.Length);
                }

                Debug.Log($"GET STRWAMING ASSETS:{url} state:{request.result} time:{Extension.GetSampleTime()} Lenght:{result.Length}");
                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();
            }

            return result;
        }

        /// <summary>
        /// 下载网络文件的二进制数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public async UniTask<bool> DownloadStreamingAsset(string url, string fileName, uint version, IProgress<float> callback = null)
        {
            Extension.StartSample();
            bool state = false;
            using (UnityWebRequest request = CreateStreamingAssetObjectRequest(url, ".byte"))
            {
                request.certificateHandler = new WebNet.CertificateController();
                await request.SendWebRequest().ToUniTask(callback);
                if (request.GetResponseHeader("Content-Type") == "application/json")
                {
                    Debug.Log(request.downloadHandler.text);
                }

                if (request.result is UnityWebRequest.Result.Success)
                {
                    WorkApi.VFS.Write(fileName, request.downloadHandler.data, version);
                    state = true;
                }

                Debug.Log($"GET STRWAMING ASSETS:{url} state:{request.result} time:{Extension.GetSampleTime()} Lenght:{request.downloadHandler.data.Length}");
                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();
            }

            return state;
        }

        /// <summary>
        /// 获取资源对象
        /// </summary>
        /// <param name="extension"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        private Object GetStreamingAssetObject(string extension, UnityWebRequest request)
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
        private UnityWebRequest CreateStreamingAssetObjectRequest(string path, string extension)
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

        public void Dispose()
        {
        }
    }
}