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
        private Dictionary<string, IChannel> channels = new();

        protected override void OnAwake()
        {
            BehaviourScriptable.instance.SetupGameObjectDestroyEvent(Clear);
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
        public async UniTask Connect<T>(string address) where T : ISerialize
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
    }
}