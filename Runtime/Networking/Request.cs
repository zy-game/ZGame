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
using Random = UnityEngine.Random;

namespace ZGame.Networking
{
    public class Request
    {
        public static UniTask<T> PostData<T>(string url, object data)
        {
            return PostData<T>(url, data, null);
        }

        public static async UniTask<T> PostDataForm<T>(string url, Dictionary<string, string> map, Dictionary<string, object> headers)
        {
            Debug.Log("POST FORM:" + url);
            // WWWForm form = new WWWForm();
            // foreach (var VARIABLE in map)
            // {
            //     form.AddField(VARIABLE.Key, VARIABLE.Value);
            // }
            //
            // byte[] boundary = new byte[40];
            // for (int index = 0; index < 40; ++index)
            // {
            //     int num = Random.Range(48, 110);
            //     if (num > 57)
            //         num += 7;
            //     if (num > 90)
            //         num += 6;
            //     boundary[index] = (byte)num;
            // }
            //
            // using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            // {
            //     // request.timeout = 5;
            //     request.useHttpContinue = true;
            //     request.disposeUploadHandlerOnDispose = true;
            //     request.disposeDownloadHandlerOnDispose = true;
            //     request.uploadHandler = new UploadHandlerRaw(form.data);
            //     request.SetRequestHeader("Content-Type", "multipart/form-data; boundary=" + Encoding.UTF8.GetString(boundary, 0, boundary.Length));
            //     if (headers is not null)
            //     {
            //         foreach (var VARIABLE in headers)
            //         {
            //             request.SetRequestHeader(VARIABLE.Key, VARIABLE.Value.ToString());
            //         }
            //     }
            //
            //     await request.SendWebRequest().ToUniTask();
            //     Debug.Log(request.downloadHandler.text);
            //     return request.GetData<T>();
            // }

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
                request.uploadHandler = new UploadHandlerRaw(UTF8Encoding.UTF8.GetBytes(str));
                request.SetRequestHeader("Content-Type", "application/json");
                if (headers is not null)
                {
                    foreach (var VARIABLE in headers)
                    {
                        request.SetRequestHeader(VARIABLE.Key, VARIABLE.Value.ToString());
                    }
                }

                await request.SendWebRequest().ToUniTask();
                return request.GetData<T>();
            }
        }

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

                Debug.Log($"GET:{url} result:{request.downloadHandler.text}");
                return request.GetData<T>();
            }
        }

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

        public static async UniTask<AudioClip> GetAudioClip(string url)
        {
            Debug.Log($"GET AUDIO:{url}");
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                request.timeout = 5;
                request.useHttpContinue = true;
                request.disposeUploadHandlerOnDispose = true;
                request.disposeDownloadHandlerOnDispose = true;
                AudioClip result = default;

                await request.SendWebRequest().ToUniTask();
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    return default;
                }

                result = DownloadHandlerAudioClip.GetContent(request);
                result.name = url;
                return result;
            }
        }

        public static async UniTask<byte[]> GetStreamingAsset(string url)
        {
            Debug.Log($"GET STRWAMING ASSETS:{url}");
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                Debug.Log(url);
                request.timeout = 5;
                request.useHttpContinue = true;
                request.disposeUploadHandlerOnDispose = true;
                request.disposeDownloadHandlerOnDispose = true;
                await request.SendWebRequest().ToUniTask();
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    return default;
                }

                byte[] result = new byte[request.downloadHandler.data.Length];
                System.Array.Copy(request.downloadHandler.data, result, result.Length);
                return result;
            }
        }

        public static async UniTask<T> GetStreamingAsset<T>(string url)
        {
            byte[] result = await GetStreamingAsset(url);
            if (result is null)
            {
                return default;
            }

            return JsonUtility.FromJson<T>(Encoding.UTF8.GetString(result));
        }
    }
}