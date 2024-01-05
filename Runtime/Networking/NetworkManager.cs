using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.FileSystem;

namespace ZGame.Networking
{
    /// <summary>
    /// 网络管理器
    /// </summary>
    public class NetworkManager : Singleton<NetworkManager>
    {
        private Dictionary<uint, IMessageHandler> _dispatchers = new();
        private Dictionary<string, IChannel> channels = new();

        protected override void OnDestroy()
        {
            foreach (var variable in channels.Values)
            {
                variable.Dispose();
            }

            channels.Clear();
            foreach (var variable in _dispatchers.Values)
            {
                variable.Dispose();
            }

            _dispatchers.Clear();
        }

        /// <summary>
        /// 连接远程地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async UniTask<IChannel> Connect(string address)
        {
            if (channels.TryGetValue(address, out IChannel channel))
            {
                return channel;
            }

            if (address.StartsWith("ws"))
            {
                // channel = new WebChannel();
            }
            else
            {
                channel = new TcpChannel();
            }

            await channel.Connect(address);
            return channel;
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

            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);
            writer.Write(Crc32.GetCRC32Str(message.GetType().FullName));
            Serializer.Serialize(memoryStream, message);
            channel.WriteAndFlush(memoryStream.ToArray());
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
            UniTaskCompletionSource<T> taskCompletionSource = new UniTaskCompletionSource<T>();
            uint crc = Crc32.GetCRC32Str(typeof(T).FullName);
            if (_dispatchers.TryGetValue(crc, out IMessageHandler dispatcher) is false)
            {
                _dispatchers.Add(crc, dispatcher = new MessageReceiverHandle<T>());
            }

            MessageReceiverHandle<T> messageReceiverHandle = (MessageReceiverHandle<T>)dispatcher;

            void OnCompletion(T message)
            {
                taskCompletionSource.TrySetResult(message);
                messageReceiverHandle.Remove(OnCompletion);
            }

            messageReceiverHandle.Add(OnCompletion);
            WriteAndFlush(address, message);
            return await taskCompletionSource.Task;
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
        /// 注册消息回调
        /// </summary>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public void RegisterMessageHandle<T>(Action<T> callback) where T : IMessage
        {
            uint crc = Crc32.GetCRC32Str(typeof(T).FullName);
            if (_dispatchers.TryGetValue(crc, out IMessageHandler dispatcher) is false)
            {
                _dispatchers.Add(crc, dispatcher = new MessageReceiverHandle<T>());
            }

            ((MessageReceiverHandle<T>)dispatcher).Add(callback);
        }

        /// <summary>
        /// 取消注册消息回调
        /// </summary>
        /// <param name="callback"></param>
        /// <typeparam name="T"></typeparam>
        public void UnregisterMessageHandle<T>(Action<T> callback) where T : IMessage
        {
            uint crc = Crc32.GetCRC32Str(typeof(T).FullName);
            if (_dispatchers.TryGetValue(crc, out IMessageHandler dispatcher) is false)
            {
                return;
            }

            ((MessageReceiverHandle<T>)dispatcher).Remove(callback);
        }

        internal void Receiver(IChannel channel, byte[] bytes)
        {
            if (bytes is null || bytes.Length == 0)
            {
                return;
            }

            MemoryStream memoryStream = new MemoryStream(bytes);
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            uint opcode = reader.ReadUInt32();
            if (_dispatchers.TryGetValue(opcode, out IMessageHandler dispatcher) is false)
            {
                Debug.LogError("未知消息类型：" + opcode);
                return;
            }

            dispatcher.ReceiveHandle(channel, opcode, memoryStream);
        }

        public static async UniTask<T> Get<T>(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                Debug.Log($"{url}");
                request.useHttpContinue = true;
                request.SetRequestHeader("Content-Type", "application/json");
                await request.SendWebRequest().ToUniTask();
                Debug.Log($"GET:{url} result:{request.downloadHandler.text}");
                T result = request.GetData<T>();
                request.Dispose();
                return result;
            }
        }

        public static async UniTask<T> Post<T>(string url, object data, Dictionary<string, object> headers = null)
        {
            Debug.Log(url);
            string str = data is string ? data as string : JsonConvert.SerializeObject(data);
            using (UnityWebRequest request = UnityWebRequest.Post(url, str))
            {
                request.useHttpContinue = true;
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
                Debug.Log($"POST:{url} result:{request.downloadHandler.text}");
                T result = request.GetData<T>();
                request.Dispose();
                return result;
            }
        }

        public static async UniTask<string> Head(string url, string headName)
        {
            using (UnityWebRequest request = UnityWebRequest.Head(url))
            {
                request.useHttpContinue = true;
                await request.SendWebRequest().ToUniTask();
                Debug.Log($"HEAD:{url} result:{request.downloadHandler.text}");
                string result = request.GetResponseHeader(headName);
                request.Dispose();
                return result;
            }
        }

        public static async UniTask<AudioClip> GetAudioClip(string url)
        {
            Debug.Log("AUDIO:" + url);
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG))
            {
                request.useHttpContinue = true;
                await request.SendWebRequest().ToUniTask();
                AudioClip result = DownloadHandlerAudioClip.GetContent(request);
                request.Dispose();
                return result;
            }
        }

        public static async UniTask<T> GetStreamingAsset<T>(string url)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                Debug.Log(url);
                request.useHttpContinue = true;
                await request.SendWebRequest().ToUniTask();
                Debug.Log($"GET:{url} result:{request.downloadHandler.text}");
                T result = JsonConvert.DeserializeObject<T>(DownloadHandlerBuffer.GetContent(request));
                return result;
            }
        }
    }
}