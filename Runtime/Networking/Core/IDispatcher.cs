using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DotNetty.Buffers;

namespace ZGame.Networking
{
    public interface IDispatcher : IReference
    {
        /// <summary>
        /// 链接成功
        /// </summary>
        /// <param name="client"></param>
        void Active(INetClient client);

        /// <summary>
        /// 派发消息
        /// </summary>
        /// <param name="client"></param>
        /// <param name="packet"></param>
        void Dispatch(INetClient client, Packet packet);

        /// <summary>
        /// 链接断开
        /// </summary>
        /// <param name="client"></param>
        void Inactive(INetClient client);
    }

    public class MessageDispatcher : IDispatcher
    {
        private Dictionary<int, Delegate> maping = new Dictionary<int, Delegate>();

        private INetClient _client;

        public virtual void Release()
        {
            _client = null;
        }

        public virtual void Active(INetClient client)
        {
            _client = client;
        }

        public virtual void Dispatch(INetClient client, Packet packet)
        {
            if (maping.Count == 0 || maping.TryGetValue(packet.opcode, out var handler) is false)
            {
                return;
            }

            handler.DynamicInvoke(client, packet);
        }

        public virtual void Inactive(INetClient client)
        {
            _client = null;
        }

        public void Subscribe<T>(int opcode, Action<T> callback) where T : IMsg
        {
            if (maping.TryGetValue(opcode, out var handler) is false)
            {
                maping.Add(opcode, (Delegate)callback);
            }
            else
            {
                maping[opcode] = Delegate.Combine(handler, (Delegate)callback);
            }
        }

        public void Unsubscribe<T>(int opcode, Action<T> callback) where T : IMsg
        {
            if (maping.TryGetValue(opcode, out var handler) is false)
            {
                return;
            }

            maping[opcode] = Delegate.Remove(handler, (Delegate)callback);
        }

        public async UniTask WriteAndFlushAsync(int opcode, IMsg msg)
        {
            if (_client is null || _client.isConnected is false)
            {
                return;
            }

            _client.WriteAndFlushAsync(Packet.Serialized(IMsg.GetMsgId(), opcode, IMsg.Encode(msg)));
        }
    }
}