using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;

namespace ZEngine.Network
{
    public class NetworkManager : Single<NetworkManager>
    {
        private Dictionary<string, IChannel> channels;
        private Dictionary<uint, Type> messageMap = new Dictionary<uint, Type>();
        private Dictionary<Type, ISubscribeHandle<IRecviedMessagePackageExecuteHandle>> subscribes;

        public NetworkManager()
        {
            channels = new Dictionary<string, IChannel>();
            subscribes = new Dictionary<Type, ISubscribeHandle<IRecviedMessagePackageExecuteHandle>>();
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var VARIABLE in channels.Values)
            {
                VARIABLE.Close();
                Engine.Class.Release(VARIABLE);
            }

            foreach (var VARIABLE in subscribes.Values)
            {
                Engine.Class.Release(VARIABLE);
            }

            channels.Clear();
            messageMap.Clear();
            subscribes.Clear();
            Engine.Console.Log("关闭所有网络链接");
        }

        public void RegisterMessageType(Type type)
        {
            uint crc = Crc32.GetCRC32Str(type.Name, 0);
            if (messageMap.ContainsKey(crc))
            {
                return;
            }

            messageMap.Add(crc, type);
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="callback">回调</param>
        public void SubscribeMessagePackage(Type messageType, ISubscribeHandle<IRecviedMessagePackageExecuteHandle> callback)
        {
            if (!subscribes.TryGetValue(messageType, out ISubscribeHandle<IRecviedMessagePackageExecuteHandle> subscribe))
            {
                subscribes.Add(messageType, subscribe = callback);
                return;
            }

            subscribe.Merge(callback);
        }

        /// <summary>
        /// 取消订阅消息
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="callback">回调</param>
        public void UnsubscribeMessagePackage(Type messageType, ISubscribeHandle<IRecviedMessagePackageExecuteHandle> callback)
        {
            if (!subscribes.TryGetValue(messageType, out ISubscribeHandle<IRecviedMessagePackageExecuteHandle> subscribe))
            {
                return;
            }

            subscribe.Unmerge(callback);
            Engine.Class.Release(callback);
        }

        /// <summary>
        /// 分发消息
        /// </summary>
        /// <param name="messagePackage">消息数据</param>
        public void DispatchMessage(IChannel channel, byte[] bytes)
        {
            uint crc = Crc32.GetCRC32Byte(bytes, 0, sizeof(uint));
            if (!messageMap.TryGetValue(crc, out Type msgType))
            {
                Engine.Console.Log("位置的消息类型 crc：", crc);
                return;
            }

            if (!subscribes.TryGetValue(msgType, out ISubscribeHandle<IRecviedMessagePackageExecuteHandle> subscribe))
            {
                Engine.Console.Error("未知的消息类型，或没有找到订阅此消息的订阅者", msgType);
                return;
            }

            Engine.Class.Loader<InternalRecviedMessagePackageExecuteHandle>().Execute(channel, bytes, msgType, subscribe);
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
        public IWebRequestExecuteHandle<T> Request<T>(string url, object data, NetworkRequestMethod method, Dictionary<string, object> header = default)
        {
            DefaultWebRequestExecuteHandle<T> defaultWebRequestExecuteHandle = Engine.Class.Loader<DefaultWebRequestExecuteHandle<T>>();
            defaultWebRequestExecuteHandle.Execute(RequestOptions.Create(url, data, header, method));
            return defaultWebRequestExecuteHandle;
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="urlList">下载列表</param>
        /// <returns></returns>
        public IDownloadExecuteHandle Download(params DownloadOptions[] urlList)
        {
            DefaultDownloadExecuteHandle downloadExecuteHandle = new DefaultDownloadExecuteHandle();
            downloadExecuteHandle.Execute(urlList);
            return downloadExecuteHandle;
        }

        /// <summary>
        /// 连接远程地址
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <returns></returns>
        public INetworkConnectExecuteHandle Connect(string address)
        {
            if (channels.TryGetValue(address, out IChannel channel))
            {
                Engine.Console.Log("链接已存在：" + address);
                return default;
            }

            INetworkConnectExecuteHandle networkConnectExecuteHandle = Engine.Class.Loader<InternalNetworkConnectExecuteHandle>();
            networkConnectExecuteHandle.Subscribe(ISubscribeHandle.Create<INetworkConnectExecuteHandle>(args => channels.Add(address, args.channel)));
            networkConnectExecuteHandle.Execute(address);
            return networkConnectExecuteHandle;
        }

        /// <summary>
        /// 写入网络消息，如果网络未连接则自动尝试链接，并在链接成功后写入消息
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="messagePackage">需要写入的消息</param>
        /// <returns></returns>
        public IWriteMessageExecuteHandle WriteAndFlush(string address, IMessagePackage messagePackage)
        {
            IWriteMessageExecuteHandle writeNetworkMessageExecuteHandle = Engine.Class.Loader<InternalWriteMessageExecuteHandle>();
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

            writeNetworkMessageExecuteHandle.Execute(channel, messagePackage);
            return writeNetworkMessageExecuteHandle;
        }

        /// <summary>
        /// 写入网络消息,并等待响应
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="messagePackage">需要写入的消息</param>
        /// <typeparam name="T">等待响应的消息类型</typeparam>
        /// <returns></returns>
        public IWriteMessageExecuteHandle<T> WriteAndFlush<T>(string address, IMessagePackage messagePackage) where T : IMessagePackage
        {
            IWriteMessageExecuteHandle<T> writeNetworkMessageExecuteHandle = Engine.Class.Loader<InternalWriteMessageExecuteHandle<T>>();
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

            writeNetworkMessageExecuteHandle.Execute(channel, messagePackage);
            return writeNetworkMessageExecuteHandle;
        }

        /// <summary>
        /// 关闭网络连接
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <returns></returns>
        public INetworkClosedExecuteHandle Close(string address)
        {
            if (!channels.TryGetValue(address, out IChannel channel))
            {
                Engine.Console.Log("未连接：", address);
                return default;
            }

            INetworkClosedExecuteHandle networkClosedExecuteHandle = Engine.Class.Loader<InternalNetworkClosedExecuteHandle>();
            networkClosedExecuteHandle.Subscribe(ISubscribeHandle.Create(() => channels.Remove(address)));
            networkClosedExecuteHandle.Execute(channel);
            return networkClosedExecuteHandle;
        }
    }
}