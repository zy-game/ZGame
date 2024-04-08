using System;
using Cysharp.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using MemoryPack;
using ProtoBuf;
using ZGame;
using ZGame.Networking;

namespace TrueSync
{
    /// <summary>
    /// Truesync's ICommunicator implementation based on PUN.
    /// </summary>
    public class SimulatorNetworkHandle : IMessageHandler
    {
        INetClient _client;
        private Action<int, byte[]> onEventReceived;
        private static Lazy<SimulatorNetworkHandle> _instance = new Lazy<SimulatorNetworkHandle>();

        public static SimulatorNetworkHandle instance
        {
            get { return _instance.Value; }
        }

        public async UniTask Initialize()
        {
            await GameFrameworkEntry.Network.Connect<UdpClient>("127.0.0.1", 8090, this);
        }

        public int RoundTripTime()
        {
            return 0; //loadBalancingPeer.RoundTripTime;
        }

        public async void OpRaiseEvent(byte eventCode, byte[] message)
        {
            if (_client is null || _client.isConnected is false)
            {
                return;
            }

            GameFrameworkEntry.Logger.Log("发送消息：" + eventCode);
            await _client.WriteAndFlushAsync(Packet.Create(eventCode, message));
        }

        public void AddEventListener(Action<int, byte[]> onEventReceived)
        {
            this.onEventReceived = onEventReceived;
            GameFrameworkEntry.Logger.Log("UDP 网络事件监听已添加");
        }

        public void Release()
        {
        }

        public void Active(INetClient client)
        {
            _client = client;
            GameFrameworkEntry.Logger.Log("UDP 网络连接已激活：" + client.address);
        }

        public void Inactive(INetClient client)
        {
            GameFrameworkEntry.Logger.Log("UDP 网络连接已断开：" + client.address);
        }

        public async void Receive(INetClient client, IByteBuffer message)
        {
            Packet msg = Packet.Deserialized(message.Array);
            GameFrameworkEntry.Logger.Log($"UDP :{client.address} 收到opcode：{msg.opcode}");
            await UniTask.SwitchToMainThread();
            onEventReceived?.Invoke(msg.opcode, msg.Data);
        }

        public void Exception(INetClient client, Exception exception)
        {
        }
    }
}