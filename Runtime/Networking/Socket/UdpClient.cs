using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Timeout;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using LiteNetLib;
using ZGame.Events;


namespace ZGame.Networking
{
    public enum NetEvent : byte
    {
        None,
        Active,
        Inactive,
        Error,
        Recived,
    }

    public sealed class NetworkEventArgs : IGameEventArgs
    {
        public NetEvent type;
        public MSGPacket packet;
        public ISocketClient socket;
        public SocketError Error;

        public void Release()
        {
            type = NetEvent.None;
            RefPooled.Free(packet);
            packet = null;
            socket = null;
        }
    }

    public class UdpClient : ISocketClient, INetEventListener
    {
        private NetManager _netClient;
        private UniTaskCompletionSource _connectTask;
        public int cid { get; private set; }
        public ushort port { get; private set; }
        public string address { get; private set; }


        public void Release()
        {
            if (_netClient != null)
                _netClient.Stop();
        }


        public async UniTask Connect(string address, ushort port)
        {
            this.port = port;
            this.address = address;
            _connectTask = new UniTaskCompletionSource();
            _netClient = new NetManager(this);
            this.cid = _netClient.GetHashCode();
            _netClient.UpdateTime = 15;
            _netClient.Start();
            _netClient.Connect(address, port, "sample_app");
            await _connectTask.Task;
        }

        public void DoUpdate()
        {
            _netClient.PollEvents();
        }

        public void Write(byte[] message)
        {
            if (_netClient is null || _netClient.FirstPeer is null || _netClient.FirstPeer.ConnectionState is not ConnectionState.Connected)
            {
                return;
            }

            _netClient.FirstPeer.Send(message, DeliveryMethod.ReliableOrdered);
        }

        public void OnPeerConnected(NetPeer peer)
        {
            AppCore.Logger.Log("[CLIENT] We connected to " + peer);
            using (NetworkEventArgs args = RefPooled.Alloc<NetworkEventArgs>())
            {
                args.socket = this;
                args.type = NetEvent.Active;
                AppCore.Events.Dispatch(NetEvent.Active, args);
            }

            _connectTask.TrySetResult();
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
        {
            AppCore.Logger.Log("[CLIENT] We received error " + socketErrorCode);
            using (NetworkEventArgs args = RefPooled.Alloc<NetworkEventArgs>())
            {
                args.socket = this;
                args.type = NetEvent.Error;
                args.Error = socketErrorCode;
                AppCore.Events.Dispatch(NetEvent.Error, args);
            }
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            MSGPacket packet = MSGPacket.Deserialize(reader.GetRemainingBytes());
            AppCore.Logger.Log($"[CLIENT] We received message:{packet.opcode}status:{packet.status} message:{packet.message} lenght:{packet.Content.Length}");
            using (NetworkEventArgs args = RefPooled.Alloc<NetworkEventArgs>())
            {
                args.socket = this;
                args.type = NetEvent.Recived;
                args.packet = packet;
                AppCore.Events.Dispatch(NetEvent.Recived, args);
            }
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            if (messageType == UnconnectedMessageType.BasicMessage && _netClient.ConnectedPeersCount == 0 && reader.GetInt() == 1)
            {
                AppCore.Logger.Log("[CLIENT] Received discovery response. Connecting to: " + remoteEndPoint);
                _netClient.Connect(remoteEndPoint, "sample_app");
            }
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            AppCore.Logger.Log("[CLIENT] We disconnected because " + disconnectInfo.Reason);
            using (NetworkEventArgs args = RefPooled.Alloc<NetworkEventArgs>())
            {
                args.socket = this;
                args.type = NetEvent.Inactive;
                AppCore.Events.Dispatch(NetEvent.Inactive, args);
            }
        }
    }
}