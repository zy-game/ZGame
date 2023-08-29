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
        private Dictionary<uint, Type> map = new Dictionary<uint, Type>();
        private Dictionary<Type, Delegate> subscribes;
        private Dictionary<Type, ISubscribeHandle> waiting;

        public NetworkManager()
        {
            channels = new Dictionary<string, IChannel>();
            subscribes = new Dictionary<Type, Delegate>();
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var VARIABLE in channels.Values)
            {
                VARIABLE.Close();
                Engine.Class.Release(VARIABLE);
            }

            map.Clear();
            channels.Clear();
            subscribes.Clear();
            Engine.Console.Log("关闭所有网络链接");
        }


        /// <summary>
        /// 分发消息
        /// </summary>
        /// <param name="messagePackage">消息数据</param>
        public void DispatchMessage(IChannel channel, byte[] bytes)
        {
            uint crc = Crc32.GetCRC32Byte(bytes, 0, sizeof(uint));
            if (!map.TryGetValue(crc, out Type msgType))
            {
                Engine.Console.Log("位置的消息类型 crc：", crc);
                return;
            }

            if (!subscribes.TryGetValue(msgType, out Delegate handle))
            {
                Engine.Console.Log("没有订阅此消息", msgType.Name);
                return;
            }

            MemoryStream memoryStream = new MemoryStream(bytes, sizeof(uint), bytes.Length - sizeof(uint));
            IMessagePackage message = (IMessagePackage)RuntimeTypeModel.Default.Deserialize(memoryStream, null, msgType);
            handle.DynamicInvoke(message);
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
            networkConnectExecuteHandle.Subscribe(ISubscribeHandle<INetworkConnectExecuteHandle>.Create(args => channels.Add(address, args.channel)));
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
        public IRecvieMessageExecuteHandle<T> WriteAndFlush<T>(string address, IMessagePackage messagePackage) where T : IMessagePackage
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

            IRecvieMessageExecuteHandle<T> internalRecvieMessageExecuteHandle = IRecvieMessageExecuteHandle<T>.Create();
            ISubscribeHandle<T>.Create(args => { internalRecvieMessageExecuteHandle.Execute(channel, args); });
            writeNetworkMessageExecuteHandle.Execute(channel, messagePackage);
            return internalRecvieMessageExecuteHandle;
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