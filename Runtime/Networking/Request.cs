using System;
using System.Collections.Generic;
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
            string str = data is string ? data as string : JsonConvert.SerializeObject(data);
            using (UnityWebRequest request = UnityWebRequest.Post(url, str))
            {
                request.timeout = 5;
                request.useHttpContinue = true;
                request.disposeUploadHandlerOnDispose = true;
                request.disposeDownloadHandlerOnDispose = true;
                using (UploadHandlerRaw handler = new UploadHandlerRaw(UTF8Encoding.UTF8.GetBytes(str)))
                {
                    request.uploadHandler = handler;
                    request.SetRequestHeader("Content-Type", "application/json");
                    if (headers is not null)
                    {
                        foreach (var VARIABLE in headers)
                        {
                            request.SetRequestHeader(VARIABLE.Key, VARIABLE.Value.ToString());
                        }
                    }

                    await request.SendWebRequest().ToUniTask();
                    if (request.result is not UnityWebRequest.Result.Success)
                    {
                        return default;
                    }

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

        /// <summary>
        /// 提交一个表单数据
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="map">消息数据</param>
        /// <param name="headers">标头</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public static async UniTask<T> PostDataForm<T>(string url, Dictionary<string, string> map, Dictionary<string, object> headers)
        {
            Debug.Log("POST FORM:" + url);
            var client = new HttpClient();
            MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = multipartFormDataContent,
            };
            foreach (var (key, value) in map)
            {
                multipartFormDataContent.Add(new StringContent(value)
                {
                    Headers =
                    {
                        ContentDisposition = new ContentDispositionHeaderValue("form-data")
                        {
                            Name = key,
                        }
                    }
                });
            }

            using (var response = await client.SendAsync(request))
            {
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                Debug.Log(body);
            }

            return default;
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
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.timeout = 5;
                request.useHttpContinue = true;
                request.disposeUploadHandlerOnDispose = true;
                request.disposeDownloadHandlerOnDispose = true;
                request.SetRequestHeader("Content-Type", "application/json");
                await request.SendWebRequest().ToUniTask();
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    return default;
                }

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

        /// <summary>
        /// 获取响应头
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headName"></param>
        /// <returns></returns>
        public static async UniTask<string> GetHead(string url, string headName)
        {
            Debug.Log($"HEAD:{url}");
            using (UnityWebRequest request = UnityWebRequest.Head(url))
            {
                request.timeout = 5;
                request.useHttpContinue = true;
                request.disposeUploadHandlerOnDispose = true;
                request.disposeDownloadHandlerOnDispose = true;
                await request.SendWebRequest().ToUniTask();
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    return default;
                }

                return request.GetResponseHeader(headName);
            }
        }

        // public static async UniTask<AudioClip> GetAudioClip(string url)
        // {
        //     Debug.Log($"GET AUDIO:{url}");
        //     using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
        //     {
        //         request.timeout = 5;
        //         request.useHttpContinue = true;
        //         request.disposeUploadHandlerOnDispose = true;
        //         request.disposeDownloadHandlerOnDispose = true;
        //         AudioClip result = default;
        //
        //         await request.SendWebRequest().ToUniTask();
        //         if (request.result is not UnityWebRequest.Result.Success)
        //         {
        //             return default;
        //         }
        //
        //         result = DownloadHandlerAudioClip.GetContent(request);
        //         result.name = url;
        //         return result;
        //     }
        // }

        public static async UniTask<byte[]> GetStreamingAsset(string url, IProgress<float> callback = null)
        {
            Debug.Log($"GET STRWAMING ASSETS:{url}");
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                Debug.Log(url);
                request.timeout = 5;
                request.useHttpContinue = true;
                request.disposeUploadHandlerOnDispose = true;
                request.disposeDownloadHandlerOnDispose = true;
                await request.SendWebRequest().ToUniTask(callback);
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    return default;
                }

                byte[] result = new byte[request.downloadHandler.data.Length];
                System.Array.Copy(request.downloadHandler.data, result, result.Length);
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