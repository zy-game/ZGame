using Cysharp.Threading.Tasks;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public sealed class HttpHead : IProcedureAsync<string>
    {
        /// <summary>
        /// 发起一个获取Head的请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headName"></param>
        /// <returns></returns>
        public static async UniTask<string> Head(string url, string headName)
        {
            return await AppCore.Procedure.Execute<string, HttpHead>(AppCore.Procedure, url, headName);
        }

        public async UniTask<string> Execute(params object[] args)
        {
            string url = (string)args[0];
            string headName = (string)args[1];
#if UNITY_EDITOR
            using (Watcher watcher = Watcher.Start(url))
            {
#endif
                AppCore.Logger.Log($"GET HEAD:{url}");
                string result = "";
                using (UnityWebRequest request = UnityWebRequest.Head(url))
                {
                    request.SetIgnoreCertificate();
                    request.SetRequestHeaderWithCors(null);
                    await request.SendWebRequest().ToUniTask();

                    if (request.result is UnityWebRequest.Result.Success)
                    {
                        result = request.GetResponseHeader(headName);
                    }

                    request.downloadHandler?.Dispose();
                    request.uploadHandler?.Dispose();
                }

                return result;
#if UNITY_EDITOR
            }
#endif
        }

        public void Release()
        {
        }
    }
}