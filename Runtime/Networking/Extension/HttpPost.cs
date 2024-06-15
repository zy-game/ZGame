using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public sealed class HttpPost : IProcedureAsync<string>
    {
        /// <summary>
        /// 发起一个Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async UniTask<string> Post(string url, object data, Dictionary<string, object> headers)
        {
            return await AppCore.Procedure.Execute<string, HttpPost>(AppCore.Procedure, url, data, headers);
        }

        /// <summary>
        /// 发起一个Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="form"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static async UniTask<string> Post(string url, WWWForm form, Dictionary<string, object> headers)
        {
            return await AppCore.Procedure.Execute<string, HttpPost>(AppCore.Procedure, url, form, headers);
        }

        public async UniTask<string> Execute(params object[] args)
        {
            string url = (string)args[0];
            object data = (object)args[1];
            Dictionary<string, object> headers = args.Length == 3 ? (Dictionary<string, object>)args[2] : new Dictionary<string, object>();

#if UNITY_EDITOR
            using (Watcher watcher = Watcher.Start(url))
            {
#endif
                if (data is not string postData)
                {
                    postData = JsonConvert.SerializeObject(data);
                }

                using UnityWebRequest request = data is not WWWForm form ? UnityWebRequest.PostWwwForm(url, postData) : UnityWebRequest.Post(url, form);
                AppCore.Logger.Log($"POST {url}");
                string _data = default;
                headers.TryAdd("Content-Type", "application/json");
                request.SetIgnoreCertificate();
                request.SetRequestHeaderWithCors(headers);
                request.uploadHandler.Dispose();
                request.uploadHandler = null;
                using (request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(postData)))
                {
                    await request.SendWebRequest().ToUniTask();
                    if (request.result is UnityWebRequest.Result.Success)
                    {
                        _data = request.downloadHandler.text;
                    }
                }

                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();


                return _data;
#if UNITY_EDITOR
            }
#endif
        }

        public void Release()
        {
        }
    }
}