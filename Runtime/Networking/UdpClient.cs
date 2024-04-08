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
using MemoryPack;

namespace ZGame.Networking
{
    public class UdpClient : INetClient
    {
        private bool isSsl = false;
        private IChannel channel;
        private MultithreadEventLoopGroup group;

        public bool isConnected { get; private set; }
        public int id => channel.Id.GetHashCode();
        public string address { get; private set; }


        public async UniTask WriteAndFlushAsync(byte[] message)
        {
            if (isConnected is false)
            {
                return;
            }

            await channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(message));
        }

        public async UniTask<Status> ConnectAsync(string host, ushort port, IMessageHandler handler)
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
                    pipeline.AddLast("echo", new UdpClientHandler(handler, this));
                }));
                channel = await bootstrap.ConnectAsync(host, port).ConfigureAwait(false);

                this.isConnected = true;
                return Status.Success;
            }
            catch (Exception e)
            {
                GameFrameworkEntry.Logger.LogError(e);
                GameFrameworkFactory.Release(this);
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
            private IMessageHandler handler;
            private INetClient client;

            public UdpClientHandler(IMessageHandler handler, INetClient client)
            {
                this.handler = handler;
                this.client = client;
            }

            public override void ChannelActive(IChannelHandlerContext context)
            {
                handler?.Active(client);
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var packet = message as DatagramPacket;
                handler?.Receive(client, packet.Content);
                packet.Release();
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                handler?.Exception(client, exception);
                GameFrameworkEntry.Logger.LogFormat("Exception: {0}", exception);
                context.CloseAsync();
            }
        }
    }
}