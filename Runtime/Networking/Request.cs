using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public class Request
    {
        /// <summary>
        /// 发起一个POST请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="data">消息数据</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public static UniTask<T> PostData<T>(string url, object data)
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
        public static async UniTask<T> PostData<T>(string url, object data, Dictionary<string, object> headers)
        {
            Debug.Log("POST:" + url);
            object _data = default;
            string str = data is string ? data as string : JsonConvert.SerializeObject(data);
            using (UnityWebRequest request = UnityWebRequest.Post(url, str))
            {
                request.useHttpContinue = true;
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
                using (request.uploadHandler = new UploadHandlerRaw(UTF8Encoding.UTF8.GetBytes(str)))
                {
                    await request.SendWebRequest().ToUniTask();
                    Debug.Log(request.downloadHandler.text);
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

        private static T GetResultData<T>(UnityWebRequest request)
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
        /// 提交一个表单数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="map">消息数据</param>
        /// <param name="headers">标头</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public static async UniTask<T> PostDataForm<T>(string url, Dictionary<string, object> map, Dictionary<string, object> headers)
        {
            Debug.Log("POST FORM:" + url);
            object _data = default;

            WWWForm form = await CreateWWWForm(map);
            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                request.useHttpContinue = true;
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
                    Debug.Log(request.downloadHandler.text);
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

        private static async UniTask<WWWForm> CreateWWWForm(Dictionary<string, object> map)
        {
            var form = new WWWForm();
            if (map is null || map.Count == 0)
            {
                return form;
            }

            foreach (var VARIABLE in map)
            {
                switch (VARIABLE.Value)
                {
                    case byte[] bytes:
                        form.AddBinaryData(VARIABLE.Key, VARIABLE.Value as byte[]);
                        break;
                    case string str:
                        if (str.Contains("http"))
                        {
                            form.AddBinaryData(VARIABLE.Key, await GetStreamingAsset(str), "icon.png");
                        }
                        else
                        {
                            form.AddField(VARIABLE.Key, VARIABLE.Value.ToString());
                        }

                        break;
                    case AudioClip audioClip:
                        form.AddBinaryData(VARIABLE.Key, WavUtility.FromAudioClip(audioClip), VARIABLE.Key + ".wav");
                        break;
                    default:
                        form.AddField(VARIABLE.Key, VARIABLE.Value.ToString());
                        break;
                }
            }

            return form;
        }

        /// <summary>
        /// 发起一个GET请求
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public static async UniTask<T> GetData<T>(string url)
        {
            Debug.Log($"GET:{url}");
            object _data = default;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Content-Type", "application/json");
                await request.SendWebRequest().ToUniTask();
                Debug.Log(request.downloadHandler.text);
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
        public static async UniTask<string> GetHead(string url, string headName)
        {
            Debug.Log($"HEAD:{url}");
            string result = "";
            using (UnityWebRequest request = UnityWebRequest.Head(url))
            {
                await request.SendWebRequest().ToUniTask();
                Debug.Log(request.downloadHandler.text);
                if (request.result is UnityWebRequest.Result.Success)
                {
                    result = request.GetResponseHeader(headName);
                }

                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();
            }

            return result;
        }

        public static async UniTask<byte[]> GetStreamingAsset(string url, IProgress<float> callback = null)
        {
            Debug.Log($"GET STRWAMING ASSETS:{url}");
            byte[] result = Array.Empty<byte>();
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                await request.SendWebRequest().ToUniTask(callback);
                if (request.result is UnityWebRequest.Result.Success)
                {
                    result = new byte[request.downloadHandler.data.Length];
                    System.Array.Copy(request.downloadHandler.data, result, result.Length);
                }

                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();
                return result;
            }
        }

        public static async UniTask<T> GetStreamingAsset<T>(string url, IProgress<float> callback = null)
        {
            byte[] result = await GetStreamingAsset(url);
            if (result is null || result.Length == 0)
            {
                return default;
            }


            return JsonUtility.FromJson<T>(Encoding.UTF8.GetString(result));
        }
    }
}