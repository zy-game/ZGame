using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using UnityEngine;

namespace ZGame.Networking
{
    public interface IDownloadData : IEntity
    {
        string url { get; }
        bool isDone { get; }
        byte[] bytes { get; }
    }

    public interface INetworkDownloadResult : IRequest
    {
        IDownloadData[] datas { get; }
    }

    public interface INetworkSystem : ISystem
    {
        /// <summary>
        /// 链接网络地址
        /// </summary>
        /// <param name="address"></param>
        /// <param name="id"></param>
        /// <param name="complation"></param>
        void Connect(string address, Action<IChannel> complation);

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
        bool WriteAndFlushAsync<T>(string address, IMessage message, Action<T> complation) where T : IMessage;

        /// <summary>
        /// 关闭网络连接
        /// </summary>
        /// <param name="address"></param>
        /// <param name="complation"></param>
        void Close(string address, Action<IChannel> complation);

        /// <summary>
        /// 从指定的地址获取数据
        /// </summary>
        /// <param name="url"></param>
        /// <param name="complation"></param>
        /// <typeparam name="T"></typeparam>
        void Get<T>(string url, Action<T> complation);

        /// <summary>
        /// 提交数据到指定的地址
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="header"></param>
        /// <param name="complation"></param>
        /// <typeparam name="T"></typeparam>
        void Post<T>(string url, object data, Dictionary<string, string> header, Action<T> complation);

        /// <summary>
        /// 网络下载
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="complation"></param>
        /// <param name="args"></param>
        void Download(Action<float> progress, Action<INetworkDownloadResult> complation, params string[] args);

        // /// <summary>
        // /// 注册消息回调管道
        // /// </summary>
        // /// <param name="type"></param>
        // void SubscribeMessageRecvier(Type type);
        //
        // /// <summary>
        // /// 取消消息订阅管道
        // /// </summary>
        // /// <param name="type"></param>
        // void UnsubscribeMessageRecvier(Type type);
        //
        // /// <summary>
        // /// 注册消息类型
        // /// </summary>
        // /// <param name="type"></param>
        // void RegisterMessageOpcode(Type type);
        //
        // /// <summary>
        // /// 取消消息注册
        // /// </summary>
        // /// <param name="type"></param>
        // void UnRegisterMessageOpcode(Type type);
        //
        // /// <summary>
        // /// 注册消息回调管道
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // public void SubscribeMessageRecvier<T>() where T : IMessageRecvierPipeline
        //     => SubscribeMessageRecvier(typeof(T));
        //
        // /// <summary>
        // /// 取消消息订阅管道
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // public void UnsubscribeMessageRecvier<T>() where T : IMessageRecvierPipeline
        //     => UnsubscribeMessageRecvier(typeof(T));
        //
        // /// <summary>
        // /// 注册消息类型
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // public void RegisterMessageOpcode<T>() where T : IMessage
        //     => RegisterMessageOpcode(typeof(T));
        //
        // /// <summary>
        // /// 取消消息注册
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // public void UnRegisterMessageOpcode<T>() where T : IMessage
        //     => UnRegisterMessageOpcode(typeof(T));
    }

    public class DefaultNetworkSystem : INetworkSystem
    {
        public string guid { get; } = ID.New();
        private Dictionary<string, IChannel> channels = new Dictionary<string, IChannel>();
        private Dictionary<string, Action<IChannel>> connectCallback = new Dictionary<string, Action<IChannel>>();
        private Dictionary<Type, Queue<IMessageRecvierPipeline>> waiting = new Dictionary<Type, Queue<IMessageRecvierPipeline>>();
        private Dictionary<Type, IMessageRecvierPipeline> messageRecvierPipelines = new Dictionary<Type, IMessageRecvierPipeline>();

        public void Startup()
        {
        }

        public void Connect(string address, Action<IChannel> complation)
        {
            if (channels.TryGetValue(address, out IChannel channel))
            {
                complation?.Invoke(channel);
                return;
            }

            if (address.StartsWith("ws"))
            {
                channel = new WebChannel(this);
            }
            else
            {
                channel = new TCPChannel(this);
            }

            connectCallback.Add(address, complation);
            channel.Connect(address);
        }

        public bool WriteAndFlush(string address, IMessage message)
        {
            if (channels.TryGetValue(address, out IChannel channel) is false)
            {
                return false;
            }

            channel.WriteAndFlush(null);
            return true;
        }

        public bool WriteAndFlushAsync<T>(string address, IMessage message, Action<T> complation) where T : IMessage
        {
            IMessageRecvierPipeline messageRecvier = IMessageRecvierPipeline.Create<T>(complation);
            if (WriteAndFlush(address, message) is false)
            {
                return false;
            }

            if (waiting.TryGetValue(typeof(T), out Queue<IMessageRecvierPipeline> recviers) is false)
            {
                waiting.Add(typeof(T), new Queue<IMessageRecvierPipeline>());
            }

            recviers.Enqueue(messageRecvier);
            return true;
        }

        public void Close(string address, Action<IChannel> complation)
        {
            throw new NotImplementedException();
        }

        public void Get<T>(string url, Action<T> complation)
        {
            throw new NotImplementedException();
        }

        public void Post<T>(string url, object data, Dictionary<string, string> header, Action<T> complation)
        {
            throw new NotImplementedException();
        }

        public void Download(Action<float> progress, Action<INetworkDownloadResult> complation, params string[] args)
        {
        }

        // public void SubscribeMessageRecvier(Type type)
        // {
        //     if (typeof(IMessageRecvierPipeline).IsAssignableFrom(type) is false)
        //     {
        //         Debug.LogError(new NotImplementedException(type.FullName));
        //         return;
        //     }
        //
        //     if (messageRecvierPipelines.TryGetValue(type, out IMessageRecvierPipeline messageRecvierPipeline))
        //     {
        //         Debug.LogError("已存在相同的消息管道");
        //         return;
        //     }
        //
        //     messageRecvierPipelines.Add(type, messageRecvierPipeline = (IMessageRecvierPipeline)Activator.CreateInstance(type));
        // }
        //
        // public void UnsubscribeMessageRecvier(Type type)
        // {
        //     if (messageRecvierPipelines.TryGetValue(type, out IMessageRecvierPipeline messageRecvierPipeline) is false)
        //     {
        //         return;
        //     }
        //
        //     messageRecvierPipeline.Dispose();
        //     messageRecvierPipelines.Remove(type);
        // }
        //
        // public void RegisterMessageOpcode(Type type)
        // {
        //     throw new NotImplementedException();
        // }
        //
        // public void UnRegisterMessageOpcode(Type type)
        // {
        //     throw new NotImplementedException();
        // }

        public void Active(IChannel channel)
        {
            channels.Add(channel.address, channel);
            if (connectCallback.TryGetValue(channel.address, out Action<IChannel> callback))
            {
                connectCallback.Remove(channel.address);
                callback(channel);
            }
        }

        public void Recvie(IChannel channel, byte[] bytes)
        {
            if (bytes is null || bytes.Length == 0)
            {
                return;
            }

            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
        }

        public void Dispose()
        {
        }
    }
}