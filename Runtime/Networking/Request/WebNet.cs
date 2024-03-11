using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.Module;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace ZGame.Networking
{
    public class WebNet : IModule
    {
        public void Dispose()
        {
        }

        public void OnAwake()
        {
        }

        public class CertificateController : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }

        /// <summary>
        /// 发起一个POST请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">消息数据</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public UniTask<T> PostData<T>(string url, object data)
        {
            return PostData<T>(url, data, null);
        }

        /// <summary>
        /// 发起一个POST请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">消息数据</param>
        /// <param name="headers">标头</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public async UniTask<T> PostData<T>(string url, object data, Dictionary<string, object> headers)
        {
            Extension.StartSample();
            object _data = default;
            Debug.Log($"POST:{url}");
            if (data is not string postData)
            {
                postData = JsonConvert.SerializeObject(data);
            }

            using (UnityWebRequest request = UnityWebRequest.Post(url, postData))
            {
                request.useHttpContinue = true;
                request.certificateHandler = new CertificateController();
                request.SetRequestHeader("Content-Type", "application/json");
                if (headers is not null)
                {
                    foreach (var VARIABLE in headers)
                    {
                        request.SetRequestHeader(VARIABLE.Key, VARIABLE.Value.ToString());
                    }
                }

                request.uploadHandler.Dispose();
                request.uploadHandler = null;
                using (request.uploadHandler = new UploadHandlerRaw(UTF8Encoding.UTF8.GetBytes(postData)))
                {
                    await request.SendWebRequest().ToUniTask();
                    UnityEngine.Debug.Log($"POST DATA:{url} parmas:{(postData).ToString()} state:{request.result} time:{Extension.GetSampleTime()}");
                    if (request.result is UnityWebRequest.Result.Success)
                    {
                        Debug.Log(request.downloadHandler.text);
                        _data = GetResultData<T>(request);
                    }

                    request.downloadHandler?.Dispose();
                    request.uploadHandler?.Dispose();
                }
            }

            return (T)_data;
        }

        /// <summary>
        /// 提交一个表单数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="map">消息数据</param>
        /// <param name="headers">标头</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public async UniTask<T> PostDataForm<T>(string url, WWWForm form, Dictionary<string, object> headers)
        {
            object _data = default;
            Extension.StartSample();
            Debug.Log($"POST:{url}");
            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                request.useHttpContinue = true;
                request.certificateHandler = new CertificateController();
                if (headers is not null)
                {
                    foreach (var VARIABLE in headers)
                    {
                        request.SetRequestHeader(VARIABLE.Key, VARIABLE.Value.ToString());
                    }
                }

                request.uploadHandler.Dispose();
                request.uploadHandler = null;
                using (request.uploadHandler = new UploadHandlerRaw(form.data))
                {
                    await request.SendWebRequest().ToUniTask();
                    Debug.Log($"POST FORM:{url} state:{request.result} time:{Extension.GetSampleTime()}");
                    if (request.result is UnityWebRequest.Result.Success)
                    {
                        _data = GetResultData<T>(request);
                    }

                    request.downloadHandler?.Dispose();
                    request.uploadHandler?.Dispose();
                }
            }

            return (T)_data;
        }

        /// <summary>
        /// 发起一个GET请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public async UniTask<T> GetData<T>(string url, bool isJson = true)
        {
            Extension.StartSample();
            Debug.Log($"GET:{url}");
            object _data = default;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.certificateHandler = new CertificateController();
                if (isJson)
                {
                    request.SetRequestHeader("Content-Type", "application/json");
                }

                await request.SendWebRequest().ToUniTask();
                Debug.Log($"GET:{url} state:{request.result} time:{Extension.GetSampleTime()}");
                if (request.result is UnityWebRequest.Result.Success)
                {
                    _data = GetResultData<T>(request);
                }

                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();
            }

            return (T)_data;
        }

        /// <summary>
        /// 获取响应头
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headName"></param>
        /// <returns></returns>
        public async UniTask<string> GetHead(string url, string headName)
        {
            Extension.StartSample();
            string result = "";
            Debug.Log($"HEAD:{url}");
            using (UnityWebRequest request = UnityWebRequest.Head(url))
            {
                request.certificateHandler = new CertificateController();
                await request.SendWebRequest().ToUniTask();
                Debug.Log($"HEAD:{url} state:{request.result} time:{Extension.GetSampleTime()}");
                if (request.result is UnityWebRequest.Result.Success)
                {
                    result = request.GetResponseHeader(headName);
                }

                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();
            }

            return result;
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
            Debug.Log($"GET STRWAMING:{url}");
            using (UnityWebRequest request = CreateStreamingAssetObjectRequest(url, extension))
            {
                request.certificateHandler = new CertificateController();
                await request.SendWebRequest().ToUniTask(callback);
                Debug.Log($"GET STRWAMING ASSETS:{url} state:{request.result} time:{Extension.GetSampleTime()}");
                if (request.GetResponseHeader("Content-Type") == "application/json")
                {
                    Debug.Log(request.downloadHandler.text);
                }

                if (request.result is UnityWebRequest.Result.Success)
                {
                    result = GetStreamingAssetObject(extension, request);
                }

                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();
                return result;
            }
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
                    break;
                case ".mp3":
                case ".wav":
                case ".ogg":
                    return DownloadHandlerAudioClip.GetContent(request);
                    break;
                case ".assetbundle":
                    return DownloadHandlerAssetBundle.GetContent(request);
                    break;
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
                default:
                    throw new NotSupportedException("不支持的资源类型");
            }
        }

        private T GetResultData<T>(UnityWebRequest request)
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
    }
}