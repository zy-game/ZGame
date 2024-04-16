using System;
using System.IO;
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


namespace ZGame.Networking
{
    public class UdpClient : INetClient
    {
        private bool isSsl = false;
        private IChannel channel;
        private MultithreadEventLoopGroup group;

        public bool isConnected { get; private set; }
        public int cid { get; private set; }
        public string address { get; private set; }


        public async UniTask WriteAndFlushAsync(byte[] message)
        {
            if (isConnected is false)
            {
                return;
            }

            await channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(message));
        }

        public void Receive(Packet packet)
        {
            
        }

        public async UniTask<Status> ConnectAsync(int cid, string host, ushort port, IDispatcher dispatcher)
        {
            address = $"{host}:{port}";
            var group = new MultithreadEventLoopGroup(1);
            try
            {
                var bootstrap = new Bootstrap();
                bootstrap.Group(group).ChannelFactory(() => new SocketDatagramChannel(AddressFamily.InterNetwork)).Handler(new ActionChannelInitializer<IChannel>(c =>
                {
                    IChannelPipeline pipeline = c.Pipeline;
                    pipeline.AddLast(new IdleStateHandler(0, 0, 20));
                    pipeline.AddLast("echo", new UdpClientHandler(this, dispatcher));
                }));
                channel = await bootstrap.ConnectAsync(host, port).ConfigureAwait(false);
                ZG.Logger.Log("Connect Success：" + address);
                this.isConnected = true;
                this.cid = cid;
                return Status.Success;
            }
            catch (Exception e)
            {
                ZG.Logger.LogError(e);
                RefPooled.Release(this);
                return Status.Fail;
            }
        }


        public async void Release()
        {
            if (this.channel != null)
                await channel.DisconnectAsync();
            if (group != null)
                await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
            this.isConnected = false;
        }

        class UdpClientHandler : ChannelHandlerAdapter
        {
            private INetClient _client;
            private IDispatcher _dispatcher;

            public UdpClientHandler(INetClient client, IDispatcher dispatcher)
            {
                this._client = client;
                this._dispatcher = dispatcher;
            }

            public override void ChannelActive(IChannelHandlerContext context)
            {
                this._dispatcher?.Active(_client);
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var packet = message as DatagramPacket;
                Packet p2 = Packet.Deserialized(packet.Content.Array);
                this._dispatcher?.Dispatch(_client, p2);
                packet.Release();
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                ZG.Logger.LogFormat("Exception: {0}", exception);
                context.CloseAsync();
            }
        }
    }
}