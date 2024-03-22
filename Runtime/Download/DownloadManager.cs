using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.FileSystem;
using ZGame.Web;
using Object = UnityEngine.Object;

namespace ZGame.Download
{
    public class DownloadManager : GameFrameworkModule
    {
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
            using (UnityWebRequest request = DownloadHelper.CreateStreamingAssetObjectRequest(url, extension))
            {
                request.certificateHandler = new HttpClient.CertificateController();
                await request.SendWebRequest().ToUniTask(callback);

                if (request.GetResponseHeader("Content-Type") == "application/json")
                {
                    Debug.Log(request.downloadHandler.text);
                }

                if (request.result is UnityWebRequest.Result.Success)
                {
                    result = DownloadHelper.GetStreamingAssetObject(extension, request);
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
            using (UnityWebRequest request = DownloadHelper.CreateStreamingAssetObjectRequest(url, ".byte"))
            {
                request.certificateHandler = new HttpClient.CertificateController();
                await request.SendWebRequest().ToUniTask(callback);
                if (request.GetResponseHeader("Content-Type") == "application/json")
                {
                    Debug.Log(request.downloadHandler.text);
                }

                if (request.result is UnityWebRequest.Result.Success)
                {
                    result = new byte[request.downloadHandler.data.Length];
                    Array.Copy(request.downloadHandler.data, 0, result, 0, result.Length);
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
            // byte[] bytes = await GetStreamingAssetBinary(url, callback);
            // ResultStatus status = GameFrameworkEntry.VFS.Write(fileName, bytes, version);
            // Debug.Log($"GET STRWAMING ASSETS:{url} state:{status} time:{Extension.GetSampleTime()} Lenght:{bytes.Length}");
            // return status == ResultStatus.Success;
            bool state = false;
            using (UnityWebRequest request = DownloadHelper.CreateStreamingAssetObjectRequest(url, ""))
            {
                request.certificateHandler = new HttpClient.CertificateController();
                await request.SendWebRequest().ToUniTask(callback);
                if (request.GetResponseHeader("Content-Type") == "application/json")
                {
                    Debug.Log(request.downloadHandler.text);
                }

                if (request.result is UnityWebRequest.Result.Success)
                {
                    GameFrameworkEntry.VFS.Write(fileName, request.downloadHandler.data, version);
                    state = true;
                }

                Debug.Log($"DOWNLOAD ASSETS:{url} state:{request.result} time:{Extension.GetSampleTime()} Lenght:{request.downloadHandler.data.Length}");
            }

            return state;
        }
    }
}