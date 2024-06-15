using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public sealed class HttpGet : IProcedureAsync<string>
    {
        /// <summary>
        /// 发起一个Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async UniTask<string> Get(string url)
        {
            return await AppCore.Procedure.Execute<string, HttpGet>(AppCore.Procedure, url);
        }

        public async UniTask<string> Execute(params object[] args)
        {
            string url = (string)args[0];
#if UNITY_EDITOR
            using (Watcher watcher = Watcher.Start(url))
            {
#endif
                AppCore.Logger.Log($"GET {url}");
                string _data = default;
                using (UnityWebRequest request = UnityWebRequest.Get(url))
                {
                    Dictionary<string, object> headers = new Dictionary<string, object>();
                    headers.Add("Content-Type", "application/json");
                    request.SetIgnoreCertificate();
                    request.SetRequestHeaderWithCors(headers);
                    await request.SendWebRequest().ToUniTask();
                    if (request.result is UnityWebRequest.Result.Success)
                    {
                        _data = request.downloadHandler.text;
                    }

                    request.downloadHandler?.Dispose();
                    request.uploadHandler?.Dispose();
                }

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