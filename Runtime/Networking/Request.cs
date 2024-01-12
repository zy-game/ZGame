using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public class Request
    {
        public static UniTask<T> PostData<T>(string url, object data)
        {
            return PostData<T>(url, data, null);
        }

        public static async UniTask<T> PostData<T>(string url, object data, Dictionary<string, object> headers)
        {
            string str = data is string ? data as string : JsonConvert.SerializeObject(data);
            using (UnityWebRequest request = UnityWebRequest.Post(url, str))
            {
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
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                Debug.Log($"{url}");
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
            using (UnityWebRequest request = UnityWebRequest.Head(url))
            {
                request.useHttpContinue = true;
                request.disposeUploadHandlerOnDispose = true;
                request.disposeDownloadHandlerOnDispose = true;
                await request.SendWebRequest().ToUniTask();
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    return default;
                }

                Debug.Log($"HEAD:{url} result:{request.downloadHandler.text}");
                return request.GetResponseHeader(headName);
            }
        }

        public static async UniTask<AudioClip> GetAudioClip(string url)
        {
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
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
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                Debug.Log(url);
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