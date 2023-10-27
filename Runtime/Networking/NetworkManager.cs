using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Networking;

namespace ZGame.Networking
{
    public class NetworkManager : INetworkManager
    {
        public string guid { get; } = ID.New();
        private List<IDispatcher> messageRecvierPipelines = new List<IDispatcher>();
        private Dictionary<string, IChannel> channels = new Dictionary<string, IChannel>();

        public async UniTask<IChannel> Connect(string address)
        {
            if (channels.TryGetValue(address, out IChannel channel))
            {
                return channel;
            }

            if (address.StartsWith("ws"))
            {
                channel = new WebChannel(this);
            }
            else
            {
                channel = new TCPChannel(this);
            }

            await channel.Connect(address);
            return channel;
        }

        public bool WriteAndFlush(string address, IMessage message)
        {
            if (channels.TryGetValue(address, out IChannel channel) is false)
            {
                return false;
            }

            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memoryStream);
            writer.Write(Crc32.GetCRC32Str(message.GetType().FullName));
            Serializer.Serialize(memoryStream, message);
            channel.WriteAndFlush(memoryStream.ToArray());
            return true;
        }

        public async UniTask<T> WriteAndFlushAsync<T>(string address, IMessage message) where T : IMessage
        {
            UniTaskCompletionSource<T> taskCompletionSource = new UniTaskCompletionSource<T>();
            IDispatcher messageRecvier = IDispatcher.Create<T>(taskCompletionSource);
            if (WriteAndFlush(address, message) is false)
            {
                return default;
            }

            messageRecvierPipelines.Add(messageRecvier);
            return await taskCompletionSource.Task;
        }

        public async UniTask<IChannel> Close(string address)
        {
            if (channels.TryGetValue(address, out IChannel channel) is false)
            {
                return default;
            }

            await channel.Close();
            return channel;
        }

        public async UniTask<T> Get<T>(string url)
        {
            IWebRequestPipeline webRequestPipeline = IWebRequestPipeline.Create(url);
            return await webRequestPipeline.GetAsync<T>();
        }

        public async UniTask<T> Post<T>(string url, object data, Dictionary<string, string> header = null)
        {
            IWebRequestPipeline webRequestPipeline = IWebRequestPipeline.Create(url);
            return await webRequestPipeline.PostAsync<T>(header, data);
        }

        public UniTask<IDownloadPipelineHandle> Download(Action<float> progress, params string[] args)
        {
            UniTaskCompletionSource<IDownloadPipelineHandle> taskCompletionSource = new UniTaskCompletionSource<IDownloadPipelineHandle>();
            IDownloadPipelineHandle.Create(progress, taskCompletionSource, args);
            return taskCompletionSource.Task;
        }

        public void SubscribeMessageRecvier(Type type)
        {
            if (typeof(IDispatcher).IsAssignableFrom(type) is false)
            {
                Debug.LogError(new NotImplementedException(type.FullName));
                return;
            }

            messageRecvierPipelines.Add((IDispatcher)Activator.CreateInstance(type));
        }

        public void UnsubscribeMessageRecvier(Type type)
        {
            for (int i = messageRecvierPipelines.Count - 1; i >= 0; i--)
            {
                if (messageRecvierPipelines[i].GetType() != type)
                {
                    continue;
                }

                messageRecvierPipelines[i].Dispose();
                messageRecvierPipelines.Remove(messageRecvierPipelines[i]);
            }
        }

        public void Recvie(IChannel channel, byte[] bytes)
        {
            if (bytes is null || bytes.Length == 0)
            {
                return;
            }

            MemoryStream memoryStream = new MemoryStream(bytes);
            BinaryReader reader = new BinaryReader(new MemoryStream(bytes));
            uint opcode = reader.ReadUInt32();
            for (int i = messageRecvierPipelines.Count - 1; i >= 0; i--)
            {
                messageRecvierPipelines[i].RecvieHandle(channel, opcode, memoryStream);
            }
        }

        public void Dispose()
        {
            foreach (var VARIABLE in channels.Values)
            {
                VARIABLE.Dispose();
            }

            channels.Clear();
            messageRecvierPipelines.ForEach(x => x.Dispose());
            messageRecvierPipelines.Clear();
        }
    }
}