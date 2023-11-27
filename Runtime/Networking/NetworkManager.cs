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
            messageRecvierPipelines.Add(messageRecvier);
            WriteAndFlush(address, message);
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