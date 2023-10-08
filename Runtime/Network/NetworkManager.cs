using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;

namespace ZEngine.Network
{
    class NetworkManager : Singleton<NetworkManager>
    {
        private Dictionary<uint, Type> map;
        private Dictionary<string, IChannel> channels;
        private Dictionary<Type, ISubscriber> waiting;

        public NetworkManager()
        {
            map = new Dictionary<uint, Type>();
            channels = new Dictionary<string, IChannel>();
            waiting = new Dictionary<Type, ISubscriber>();
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var VARIABLE in channels.Values)
            {
                VARIABLE.Close();
                VARIABLE.Dispose();
            }

            map.Clear();
            foreach (var VARIABLE in waiting.Values)
            {
                VARIABLE.Dispose();
            }

            waiting.Clear();
            channels.Clear();
            Engine.Console.Log("关闭所有网络链接");
        }

        /// <summary>
        /// 创建一个HTTP请求
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <param name="data">参数</param>
        /// <param name="method">方法</param>
        /// <param name="header">标头</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public IWebRequestleHandle<T> Get<T>(string url, Dictionary<string, object> header = default)
        {
            IWebRequestleHandle<T> webRequestScheduleHandle = IWebRequestleHandle<T>.CreateRequest(url, header);
            webRequestScheduleHandle.Execute();
            return webRequestScheduleHandle;
        }

        /// <summary>
        /// 创建一个HTTP请求
        /// </summary>
        /// <param name="url">远程地址</param>
        /// <param name="data">参数</param>
        /// <param name="method">方法</param>
        /// <param name="header">标头</param>
        /// <typeparam name="T">返回数据类型</typeparam>
        /// <returns></returns>
        public IWebRequestleHandle<T> Post<T>(string url, object data, Dictionary<string, object> header = default)
        {
            IWebRequestleHandle<T> webRequestScheduleHandle = IWebRequestleHandle<T>.CreatePost(url, data, header);
            webRequestScheduleHandle.Execute();
            return webRequestScheduleHandle;
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="urlList">下载列表</param>
        /// <returns></returns>
        public IDownloadHandle Download(params DownloadOptions[] urlList)
        {
            IDownloadHandle downloadScheduleHandle = IDownloadHandle.Create(urlList);
            downloadScheduleHandle.Execute();
            return downloadScheduleHandle;
        }

        /// <summary>
        /// 连接远程地址
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <returns></returns>
        public void Connect(string address, int id = 0)
        {
            if (channels.TryGetValue(address, out IChannel channel))
            {
                Engine.Console.Log("链接已存在：" + address);
                return;
            }

            channel.Connect(address, id);
        }

        /// <summary>
        /// 写入网络消息，如果网络未连接则自动尝试链接，并在链接成功后写入消息
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="messagePackage">需要写入的消息</param>
        /// <returns></returns>
        public void WriteAndFlush(string address, IMessaged messagePackage)
        {
            if (!channels.TryGetValue(address, out IChannel channel))
            {
                Engine.Console.Log("未找到指定的链接", address);
                return;
            }

            if (channel.connected is false)
            {
                Engine.Console.Error("连接已断开：", address);
                return;
            }

            MemoryStream memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, messagePackage);
            channel.WriteAndFlush(memoryStream.ToArray());
        }

        /// <summary>
        /// 写入网络消息,并等待响应
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="messagePackage">需要写入的消息</param>
        /// <typeparam name="T">等待响应的消息类型</typeparam>
        /// <returns></returns>
        public IScheduleHandle<T> WriteAndFlushAsync<T>(string address, IMessaged messagePackage) where T : IMessaged
        {
            if (!channels.TryGetValue(address, out IChannel channel))
            {
                Engine.Console.Log("未找到指定的链接", address);
                return default;
            }

            if (channel.connected is false)
            {
                Engine.Console.Error("连接已断开：", address);
                return default;
            }

            MemoryStream memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, messagePackage);
            IScheduleToken<T> token = IScheduleToken<T>.Create();
            IScheduleHandle<T> subscriber = IScheduleHandle.Schedule<T>(token);
            RPCHandle.instance.Subscribe(typeof(T), token);
            channel.WriteAndFlush(memoryStream.ToArray());
            return subscriber;
        }

        /// <summary>
        /// 关闭网络连接
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <returns></returns>
        public void Close(string address)
        {
            if (!channels.TryGetValue(address, out IChannel channel))
            {
                Engine.Console.Log("未连接：", address);
                return;
            }

            channel.Close();
        }
    }
}