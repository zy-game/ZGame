using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using ProtoBuf.Meta;

namespace ZGame.Networking
{
    public interface INetworkManager : IManager
    {
        /// <summary>
        /// 链接网络地址
        /// </summary>
        /// <param name="address"></param>
        /// <param name="id"></param>
        /// <param name="complation"></param>
        UniTask<IChannel> Connect(string address);

        /// <summary>
        /// 写入消息数据
        /// </summary>
        /// <param name="address"></param>
        /// <param name="message"></param>
        bool WriteAndFlush(string address, IMessage message);

        /// <summary>
        /// 写入数据并等待返回消息
        /// </summary>
        /// <param name="address"></param>
        /// <param name="message"></param>
        /// <param name="complation"></param>
        /// <typeparam name="T"></typeparam>
        UniTask<T> WriteAndFlushAsync<T>(string address, IMessage message) where T : IMessage;

        /// <summary>
        /// 关闭网络连接
        /// </summary>
        /// <param name="address"></param>
        /// <param name="complation"></param>
        UniTask<IChannel> Close(string address);

        /// <summary>
        /// 从指定的地址获取数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="complation"></param>
        /// <typeparam name="T"></typeparam>
        UniTask<T> Get<T>(string url);

        /// <summary>
        /// 提交数据到指定的地址
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="complation"></param>
        /// <typeparam name="T"></typeparam>
        UniTask<T> Post<T>(string url, object data, Dictionary<string, string> header = null);

        /// <summary>
        /// 网络下载
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="complation"></param>
        /// <param name="args"></param>
        UniTask<IDownloadPipelineHandle> Download(Action<float> progress, params string[] args);

        /// <summary>
        /// 注册消息回调管道
        /// </summary>
        /// <param name="type"></param>
        void SubscribeMessageRecvier(Type type);

        /// <summary>
        /// 取消消息订阅管道
        /// </summary>
        /// <param name="type"></param>
        void UnsubscribeMessageRecvier(Type type);

        /// <summary>
        /// 注册消息回调管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void SubscribeMessageRecvier<T>() where T : IDispatcher
            => SubscribeMessageRecvier(typeof(T));

        /// <summary>
        /// 取消消息订阅管道
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnsubscribeMessageRecvier<T>() where T : IDispatcher
            => UnsubscribeMessageRecvier(typeof(T));
    }
}