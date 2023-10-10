using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProtoBuf;
using ProtoBuf.Meta;

namespace ZEngine.Network
{
    class NetworkManager : Singleton<NetworkManager>
    {
        private List<IMessageRecvierHandle> handles = new List<IMessageRecvierHandle>();
        private Dictionary<string, IChannel> channels = new Dictionary<string, IChannel>();
        private Dictionary<Type, IScheduleHandleToken> wait = new Dictionary<Type, IScheduleHandleToken>();
        private Queue<DispatcherData> messages = new Queue<DispatcherData>();

        class DispatcherData : IDisposable
        {
            public IChannel channel;
            public byte[] bytes;

            public void Dispose()
            {
                channel = null;
                bytes = null;
                GC.SuppressFinalize(this);
            }
        }

        protected override void OnUpdate()
        {
            if (messages.Count == 0)
            {
                return;
            }

            while (messages.Count > 0)
            {
                DispatcherData data = messages.Dequeue();
                uint opcode = Crc32.GetCRC32Byte(data.bytes, 0, sizeof(uint));
                MemoryStream memoryStream = new MemoryStream(data.bytes, sizeof(uint), data.bytes.Length - sizeof(uint));
                for (int i = handles.Count - 1; i >= 0; i--)
                {
                    handles[i].OnHandleMessage(data.channel.address, opcode, memoryStream);
                }

                data.Dispose();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            foreach (var VARIABLE in channels.Values)
            {
                VARIABLE.Close();
                VARIABLE.Dispose();
            }

            handles.ForEach(x => x.Dispose());
            handles.Clear();
            foreach (var VARIABLE in wait.Values)
            {
                VARIABLE.Dispose();
            }

            wait.Clear();

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
        public IScheduleHandle<IChannel> Connect<T>(string address, int id = 0) where T : IChannel
        {
            if (channels.TryGetValue(address, out IChannel channel))
            {
                Engine.Console.Log("链接已存在：" + address);
                return IScheduleHandle.Failur<IChannel>();
            }

            channel = Activator.CreateInstance<T>();
            channels.Add(address, channel);
            return channel.Connect(address, id);
        }

        /// <summary>
        /// 写入网络消息，如果网络未连接则自动尝试链接，并在链接成功后写入消息
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="messagePackage">需要写入的消息</param>
        /// <returns></returns>
        public IScheduleHandle<int> WriteAndFlush(string address, IMessaged messagePackage)
        {
            if (!channels.TryGetValue(address, out IChannel channel))
            {
                Engine.Console.Log("未找到指定的链接", address);
                return IScheduleHandle.Complate<int>(0);
            }

            if (channel.connected is false)
            {
                Engine.Console.Error("连接已断开：", address);
                return IScheduleHandle.Complate<int>(0);
            }

            MemoryStream memoryStream = new MemoryStream();
            Serializer.Serialize(memoryStream, messagePackage);
            return channel.WriteAndFlush(memoryStream.ToArray());
        }

        public void SubscribeMessageRecvieHandle(Type types)
        {
            if (typeof(IMessageRecvierHandle).IsAssignableFrom(types) is false)
            {
                Engine.Console.Error(new NotImplementedException(types.FullName));
                return;
            }

            if (handles.Find(x => x.GetType() == types) is not null)
            {
                Engine.Console.Error("已经存在相同类型的消息处理管道", types);
                return;
            }

            handles.Add((IMessageRecvierHandle)Activator.CreateInstance(types));
        }

        public void UnsubscribeMessageRecvieHandle(Type types)
        {
            if (typeof(IMessageRecvierHandle).IsAssignableFrom(types) is false)
            {
                Engine.Console.Error(new NotImplementedException(types.FullName));
                return;
            }

            IMessageRecvierHandle messageRecvierHandle = handles.Find(x => x.GetType() == types);
            if (messageRecvierHandle is null)
            {
                Engine.Console.Error("不存在消息处理管道", types);
                return;
            }

            handles.Remove(messageRecvierHandle);
        }

        public void Dispacher(IChannel channel, byte[] bytes)
        {
            messages.Enqueue(new DispatcherData()
            {
                channel = channel,
                bytes = bytes
            });
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