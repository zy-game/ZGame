using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.VFS;
using Object = UnityEngine.Object;

namespace ZGame.Networking
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    public class NetworkManager : GameFrameworkModule
    {
        private Dictionary<string, IChannel> channels = new();

        public override void OnAwake(params object[] args)
        {
        }

        public override void Release()
        {
            Clear();
            GC.SuppressFinalize(this);
        }

        public void Clear()
        {
            foreach (var VARIABLE in channels.Values)
            {
                VARIABLE.Dispose();
            }

            channels.Clear();
        }

        /// <summary>
        /// 连接远程地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async UniTask Connect<T>(string address) where T : ISerialize, new()
        {
            if (channels.TryGetValue(address, out IChannel channel))
            {
                return;
            }

            if (address.StartsWith("ws"))
            {
                channel = new WebChannel<T>();
            }
            else
            {
                channel = new TcpChannel<T>();
            }

            await channel.Connect(address);
            if (channel.connected is false)
            {
                return;
            }

            channels.Add(address, channel);
        }

        /// <summary>
        /// 写入消息
        /// </summary>
        /// <param name="address"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool WriteAndFlush(string address, IMessage message)
        {
            if (channels.TryGetValue(address, out IChannel channel) is false)
            {
                return false;
            }

            // MemoryStream memoryStream = new MemoryStream();
            // BinaryWriter writer = new BinaryWriter(memoryStream);
            // writer.Write(Crc32.GetCRC32Str(message.GetType().FullName));
            // Serializer.Serialize(memoryStream, message);
            channel.WriteAndFlush(message);
            return true;
        }

        /// <summary>
        /// 写入消息并等待响应
        /// </summary>
        /// <param name="address"></param>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async UniTask<T> WriteAndFlushAsync<T>(string address, IMessage message) where T : IMessage
        {
            if (channels.TryGetValue(address, out IChannel channel) is false)
            {
                return default;
            }

            return await channel.WriteAndFlushAsync<T>(message);
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async UniTask<IChannel> Close(string address)
        {
            if (channels.TryGetValue(address, out IChannel channel) is false)
            {
                return default;
            }

            await channel.Close();
            return channel;
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
            if (data is not string postData)
            {
                postData = JsonConvert.SerializeObject(data);
            }

            using (UnityWebRequest request = UnityWebRequest.Post(url, postData))
            {
                headers = headers ?? new Dictionary<string, object>();
                headers.TryAdd("Content-Type", "application/json");
                request.SetRequestHeaderWithCors(headers);
                request.uploadHandler.Dispose();
                request.uploadHandler = null;
                using (request.uploadHandler = new UploadHandlerRaw(UTF8Encoding.UTF8.GetBytes(postData)))
                {
                    await request.SendWebRequest().ToUniTask();

                    if (request.result is UnityWebRequest.Result.Success)
                    {
                        _data = request.GetResultData<T>();
                    }
                }

                UnityEngine.Debug.Log($"POST DATA:{url} parmas:{(postData).ToString()} state:{request.result} time:{Extension.GetSampleTime()}");
                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();
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
            using (UnityWebRequest request = UnityWebRequest.Post(url, form))
            {
                headers = headers ?? new Dictionary<string, object>();

                request.SetRequestHeaderWithCors(headers);
                request.uploadHandler.Dispose();
                request.uploadHandler = null;
                using (request.uploadHandler = new UploadHandlerRaw(form.data))
                {
                    await request.SendWebRequest().ToUniTask();

                    if (request.result is UnityWebRequest.Result.Success)
                    {
                        _data = request.GetResultData<T>();
                    }
                }

                Debug.Log($"POST FORM:{url} state:{request.result} time:{Extension.GetSampleTime()}");
                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();
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
            object _data = default;
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                Dictionary<string, object> headers = new Dictionary<string, object>();
                if (isJson)
                {
                    headers.Add("Content-Type", "application/json");
                }

                request.SetRequestHeaderWithCors(headers);
                await request.SendWebRequest().ToUniTask();
                if (request.result is UnityWebRequest.Result.Success)
                {
                    _data = request.GetResultData<T>();
                }

                Debug.Log($"GET:{url} state:{request.result} time:{Extension.GetSampleTime()} data: {request.downloadHandler?.text}");
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
            using (UnityWebRequest request = UnityWebRequest.Head(url))
            {
                request.SetRequestHeaderWithCors(null);
                await request.SendWebRequest().ToUniTask();

                if (request.result is UnityWebRequest.Result.Success)
                {
                    result = request.GetResponseHeader(headName);
                }

                Debug.Log($"HEAD:{url} state:{request.result} time:{Extension.GetSampleTime()}");
                request.downloadHandler?.Dispose();
                request.uploadHandler?.Dispose();
            }

            return result;
        }
    }
}