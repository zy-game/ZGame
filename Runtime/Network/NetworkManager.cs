using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using ProtoBuf;
using ProtoBuf.Meta;
using Unity.Plastic.Newtonsoft.Json;

namespace ZEngine.Network
{
    class NetworkManager : Singleton<NetworkManager>
    {
        // private List<IMessageRecvierHandle> handles = new List<IMessageRecvierHandle>();
        // private Dictionary<string, IChannel> channels = new Dictionary<string, IChannel>();
        // private Queue<Tuple<IChannel, byte[]>> messages = new Queue<Tuple<IChannel, byte[]>>();


        // protected override void OnUpdate()
        // {
        //     if (messages.Count == 0)
        //     {
        //         return;
        //     }
        //
        //     while (messages.Count > 0)
        //     {
        //         Tuple<IChannel, byte[]> data = messages.Dequeue();
        //         BinaryReader reader = new BinaryReader(new MemoryStream(data.Item2));
        //         uint opcode = reader.ReadUInt32();
        //         string body = reader.ReadString();
        //         for (int i = handles.Count - 1; i >= 0; i--)
        //         {
        //             handles[i].OnHandleMessage(data.Item1.address, opcode, new MemoryStream(UTF8Encoding.UTF8.GetBytes(body)));
        //         }
        //     }
        // }

        public override void Dispose()
        {
            ZGame.Datable.Clear<IChannel>();
            ZGame.Console.Log("关闭所有网络链接");
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
        public UniTask<IWebRequestResult<T>> Get<T>(string url, Dictionary<string, object> header = default)
        {
            UniTaskCompletionSource<IWebRequestResult<T>> uniTaskCompletionSource = new UniTaskCompletionSource<IWebRequestResult<T>>();
            IWebRequestResult<T>.CreateRequest(url, header, uniTaskCompletionSource);
            return uniTaskCompletionSource.Task;
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
        public UniTask<IWebRequestResult<T>> Post<T>(string url, object data, Dictionary<string, object> header = default)
        {
            UniTaskCompletionSource<IWebRequestResult<T>> uniTaskCompletionSource = new UniTaskCompletionSource<IWebRequestResult<T>>();
            IWebRequestResult<T>.CreatePost(url, data, header, uniTaskCompletionSource);
            return uniTaskCompletionSource.Task;
        }

        /// <summary>
        /// 下载
        /// </summary>
        /// <param name="urlList">下载列表</param>
        /// <returns></returns>
        public UniTask<IDownloadResult> Download(IProgressHandle gameProgressHandle, params DownloadOptions[] urlList)
        {
            UniTaskCompletionSource<IDownloadResult> uniTaskCompletionSource = new UniTaskCompletionSource<IDownloadResult>();
            IDownloadResult.Create(gameProgressHandle, urlList, uniTaskCompletionSource);
            return uniTaskCompletionSource.Task;
        }

        /// <summary>
        /// 连接远程地址
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <returns></returns>
        public async UniTask<IChannel> Connect<T>(string address, int id = 0) where T : IChannel
        {
            if (ZGame.Datable.TryGetValue(address, out IChannel channel) is false)
            {
                ZGame.Datable.Add(address, channel = Activator.CreateInstance<T>());
                await channel.Connect(address, id);
            }

            return channel;
        }

        /// <summary>
        /// 写入网络消息，如果网络未连接则自动尝试链接，并在链接成功后写入消息
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <param name="messagePackage">需要写入的消息</param>
        /// <returns></returns>
        public async void WriteAndFlush(string address, IMessaged messagePackage)
        {
            if (ZGame.Datable.TryGetValue(address, out IChannel channel) is false)
            {
                ZGame.Console.Log("未找到指定的链接", address);
                return;
            }

            if (channel.connected is false)
            {
                ZGame.Console.Error("连接已断开：", address);
                return;
            }

            await channel.WriteAndFlush(IMessaged.Serialize(messagePackage));
        }

        /// <summary>
        /// 关闭网络连接
        /// </summary>
        /// <param name="address">远程地址</param>
        /// <returns></returns>
        public UniTask<IChannel> Close(string address)
        {
            if (!ZGame.Datable.TryGetValue(address, out IChannel channel))
            {
                ZGame.Console.Log("未连接：", address);
                return UniTask.FromResult(channel);
            }

            return channel.Close();
        }
    }
}