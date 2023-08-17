using System;
using System.Collections;
using System.Collections.Generic;

namespace ZEngine.Network
{
    public class NetworkManager : Single<NetworkManager>
    {
        private Dictionary<string, IChannel> channels;
        private Dictionary<Type, IRecviedMessagePackageExecuteHandle> recviedMessagePackageExecuteHandles;

        public NetworkManager()
        {
            channels = new Dictionary<string, IChannel>();
            recviedMessagePackageExecuteHandles = new Dictionary<Type, IRecviedMessagePackageExecuteHandle>();
        }

        public override void Dispose()
        {
            base.Dispose();
            Engine.Console.Log("关闭所有网络链接");
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="callback">回调</param>
        public void SubscribeMessagePackage(Type messageType, ISubscribeHandle<IRecviedMessagePackageExecuteHandle> callback)
        {
            if (!recviedMessagePackageExecuteHandles.TryGetValue(messageType, out IRecviedMessagePackageExecuteHandle recviedMessagePackageExecuteHandle))
            {
                recviedMessagePackageExecuteHandles.Add(messageType, recviedMessagePackageExecuteHandle = Engine.Class.Loader<RecviedMessagePackageExecuteHandle>());
            }

            recviedMessagePackageExecuteHandle.Subscribe(callback);
        }

        /// <summary>
        /// 取消订阅消息
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="callback">回调</param>
        public void UnsubscribeMessagePackage(Type messageType, ISubscribeHandle<IRecviedMessagePackageExecuteHandle> callback)
        {
            if (!recviedMessagePackageExecuteHandles.TryGetValue(messageType, out IRecviedMessagePackageExecuteHandle recviedMessagePackageExecuteHandle))
            {
                return;
            }

            recviedMessagePackageExecuteHandle.Unsubscribe(callback);
        }

        /// <summary>
        /// 分发消息
        /// </summary>
        /// <param name="messagePackage">消息数据</param>
        public void DispatchMessage(IMessagePackage messagePackage)
        {
            if (!recviedMessagePackageExecuteHandles.TryGetValue(messagePackage.GetType(), out IRecviedMessagePackageExecuteHandle handle))
            {
                Engine.Console.Error("未知的消息类型，或没有找到订阅此消息的订阅者", messagePackage.GetType());
                return;
            }

            handle.Execute(messagePackage);
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
            networkConnectExecuteHandle.Subscribe(args => channels.Add(address, args.channel));
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
                INetworkConnectExecuteHandle networkConnectExecuteHandle = Connect(address);
                networkConnectExecuteHandle.Subscribe(() => { writeNetworkMessageExecuteHandle.Execute(networkConnectExecuteHandle.channel, messagePackage); });
                return writeNetworkMessageExecuteHandle;
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
                INetworkConnectExecuteHandle networkConnectExecuteHandle = Connect(address);
                networkConnectExecuteHandle.Subscribe(() => { writeNetworkMessageExecuteHandle.Execute(networkConnectExecuteHandle.channel, messagePackage); });
                return writeNetworkMessageExecuteHandle;
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
            networkClosedExecuteHandle.Subscribe(() => channels.Remove(address));
            networkClosedExecuteHandle.Execute(channel);
            return networkClosedExecuteHandle;
        }
    }
}