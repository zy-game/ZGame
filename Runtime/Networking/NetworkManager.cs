using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using Cysharp.Threading.Tasks;
using ProtoBuf;
using ProtoBuf.Meta;
using UnityEngine;
using UnityEngine.Networking;
using ZGame.FileSystem;

namespace ZGame.Networking
{
    public class DownloadOptions : IDisposable
    {
        public string name;
        public string url;
        public uint version;

        public void Dispose()
        {
        }
    }

    public struct NetworkDownloadPipelineHandle : IDisposable
    {
        public void Dispose()
        {
            name = String.Empty;
            url = String.Empty;
            bytes = Array.Empty<byte>();
            GC.SuppressFinalize(this);
        }

        public string name;
        public float progress;
        public bool isDone;
        public string url;
        public byte[] bytes;
        public string error;
    }

    public class NetworkManager : SingletonBehaviour<NetworkManager>
    {
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

        public async UniTask<string> Head(string url, string headName)
        {
            IWebRequestPipeline webRequestPipeline = IWebRequestPipeline.Create(url);
            return await webRequestPipeline.Head(headName);
        }

        public async UniTask<NetworkDownloadPipelineHandle[]> Download(Action<float> progress, params string[] args)
        {
            if (args is null || args.Length == 0)
            {
                return default;
            }

            UniTaskCompletionSource<NetworkDownloadPipelineHandle[]> taskCompletionSource = new UniTaskCompletionSource<NetworkDownloadPipelineHandle[]>();
            NetworkDownloadPipelineHandle[] handles = new NetworkDownloadPipelineHandle[args.Length];
            StartCoroutine(UpdateProgress());
            for (int i = 0; i < args.Length; i++)
            {
                handles[i] = new NetworkDownloadPipelineHandle()
                {
                    name = Path.GetFileName(args[i]),
                    url = args[i],
                    isDone = false,
                    progress = 0,
                    bytes = Array.Empty<byte>()
                };
                StartCoroutine(Download(handles[i]));
            }

            IEnumerator UpdateProgress()
            {
                while (handles.Where(x => x.isDone == false).Count() > 0)
                {
                    progress?.Invoke(handles.Sum(x => x.progress));
                    yield return new WaitForSeconds(0.1f);
                }

                taskCompletionSource.TrySetResult(handles);
            }

            IEnumerator Download(NetworkDownloadPipelineHandle downloadData)
            {
                UnityWebRequest request = UnityWebRequest.Get(downloadData.url);
                request.SendWebRequest();
                while (!request.isDone)
                {
                    downloadData.progress = request.downloadProgress;
                    yield return new WaitForSeconds(0.01f);
                }

                downloadData.isDone = true;
                if (request.result is not UnityWebRequest.Result.Success)
                {
                    downloadData.error = request.downloadHandler.text;
                    yield break;
                }

                downloadData.bytes = request.downloadHandler.data;
            }

            return await taskCompletionSource.Task;
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