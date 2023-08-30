using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;

namespace ZEngine.Network
{
    class NetworkManager : Single<NetworkManager>
    {
        private Dictionary<uint, Type> map;
        private Dictionary<string, IChannel> channels;
        private Dictionary<Type, ISubscribeHandle> waiting;
        private List<ISubscribeMessageExecuteHandle> subscribeMessageExecuteHandles;

        public NetworkManager()
        {
            map = new Dictionary<uint, Type>();
            channels = new Dictionary<string, IChannel>();
            waiting = new Dictionary<Type, ISubscribeHandle>();
            subscribeMessageExecuteHandles = new List<ISubscribeMessageExecuteHandle>();
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
            foreach (var VARIABLE in waiting.Values)
            {
                Engine.Class.Release(VARIABLE);
            }

            waiting.Clear();
            subscribeMessageExecuteHandles.ForEach(Engine.Class.Release);
            subscribeMessageExecuteHandles.Clear();
            channels.Clear();
            Engine.Console.Log("关闭所有网络链接");
        }

        public void SubscribeMessageHandle(Type type)
        {
            if (typeof(ISubscribeMessageExecuteHandle).IsAssignableFrom(type) is false)
            {
                Engine.Console.Error("当前注册的消息管道没有实现", typeof(ISubscribeMessageExecuteHandle).Name);
                return;
            }

            subscribeMessageExecuteHandles.Add((ISubscribeMessageExecuteHandle)Engine.Class.Loader(type));
        }

        public void UnsubscribeMessageHandle(Type type)
        {
            ISubscribeMessageExecuteHandle subscribeMessageExecuteHandle = subscribeMessageExecuteHandles.Find(x => x.GetType() == type);
            if (subscribeMessageExecuteHandle is null)
            {
                return;
            }

            subscribeMessageExecuteHandles.Remove(subscribeMessageExecuteHandle);
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

            MemoryStream memoryStream = new MemoryStream(bytes, sizeof(uint), bytes.Length - sizeof(uint));
            IMessagePacket message = (IMessagePacket)RuntimeTypeModel.Default.Deserialize(memoryStream, null, msgType);
            if (waiting.TryGetValue(msgType, out ISubscribeHandle subscribeHandle))
            {
                subscribeHandle.Execute(message);
            }

            for (int i = 0; i < subscribeMessageExecuteHandles.Count; i++)
            {
                subscribeMessageExecuteHandles[i].OnHandleMessage(message);
            }
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
            IWebRequestExecuteHandle<T> webRequestExecuteHandle = IWebRequestExecuteHandle<T>.Create(url, data, header, method);
            webRequestExecuteHandle.Execute();
            return webRequestExecuteHandle;
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="urlList">下载列表</param>
        /// <returns></returns>
        public IDownloadExecuteHandle Download(params DownloadOptions[] urlList)
        {
            IDownloadExecuteHandle downloadExecuteHandle = IDownloadExecuteHandle.Create(urlList);
            downloadExecuteHandle.Execute();
            return downloadExecuteHandle;
        }

        /// <summary>
        /// 连接远程地址
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <returns></returns>
        public INetworkConnectExecuteHandle Connect(string address, int id = 0)
        {
            if (channels.TryGetValue(address, out IChannel channel))
            {
                Engine.Console.Log("链接已存在：" + address);
                return default;
            }

            INetworkConnectExecuteHandle networkConnectExecuteHandle = INetworkConnectExecuteHandle.Create(address, id);
            networkConnectExecuteHandle.Subscribe(ISubscribeHandle<INetworkConnectExecuteHandle>.Create(args => channels.Add(address, args.channel)));
            networkConnectExecuteHandle.Execute();
            return networkConnectExecuteHandle;
        }

        /// <summary>
        /// 写入网络消息，如果网络未连接则自动尝试链接，并在链接成功后写入消息
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="messagePackage">需要写入的消息</param>
        /// <returns></returns>
        public IWriteMessageExecuteHandle WriteAndFlush(string address, IMessagePacket messagePackage)
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

            IWriteMessageExecuteHandle writeNetworkMessageExecuteHandle = IWriteMessageExecuteHandle.Create(channel, messagePackage);
            writeNetworkMessageExecuteHandle.Execute();
            return writeNetworkMessageExecuteHandle;
        }

        /// <summary>
        /// 写入网络消息,并等待响应
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="messagePackage">需要写入的消息</param>
        /// <typeparam name="T">等待响应的消息类型</typeparam>
        /// <returns></returns>
        public IRecvieMessageExecuteHandle<T> WriteAndFlush<T>(string address, IMessagePacket messagePackage) where T : IMessagePacket
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

            IWriteMessageExecuteHandle writeNetworkMessageExecuteHandle = IWriteMessageExecuteHandle.Create(channel, messagePackage);
            IRecvieMessageExecuteHandle<T> internalRecvieMessageExecuteHandle = IRecvieMessageExecuteHandle<T>.Create(channel);
            writeNetworkMessageExecuteHandle.Execute();
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

            INetworkClosedExecuteHandle networkClosedExecuteHandle = INetworkClosedExecuteHandle.Create(channel);
            networkClosedExecuteHandle.Subscribe(ISubscribeHandle.Create(() => channels.Remove(address)));
            networkClosedExecuteHandle.Execute();
            return networkClosedExecuteHandle;
        }
    }
}