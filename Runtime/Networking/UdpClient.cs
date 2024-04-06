using System;
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

        public bool Connected { get; private set; }
        public int id => channel.Id.GetHashCode();
        public string address { get; private set; }

        public async UniTask WriteAsync(IMessage message)
        {
            if (Connected is false)
            {
                return;
            }

            await channel.WriteAsync(Unpooled.WrappedBuffer(MemoryPackSerializer.Serialize(message)));
        }

        public async UniTask WriteAndFlushAsync(IMessage message)
        {
            if (Connected is false)
            {
                return;
            }

            await channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(MemoryPackSerializer.Serialize(message)));
        }

        public async UniTask<Status> ConnectAsync(string host, ushort port)
        {
            address = $"{host}:{port}";
            var group = new MultithreadEventLoopGroup(1);
            try
            {
                var bootstrap = new Bootstrap();
                bootstrap.Group(group).ChannelFactory(() => new SocketDatagramChannel(AddressFamily.InterNetwork)).Handler(new ActionChannelInitializer<IChannel>(c =>
                {
                    IChannelPipeline pipeline = c.Pipeline;
                    pipeline.AddLast(new LoggingHandler("CLIENT-CONN"));
                    pipeline.AddLast(new IdleStateHandler(0, 0, 20));
                    pipeline.AddLast("echo", new UdpClientHandler());
                }));
                channel = await bootstrap.ConnectAsync(host, port).ConfigureAwait(false);
                this.Connected = true;
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
            this.Connected = false;
        }

        class UdpClientHandler : ChannelHandlerAdapter
        {
            readonly IByteBuffer initialMessage;

            public UdpClientHandler()
            {
                this.initialMessage = Unpooled.Buffer(256);
                byte[] messageBytes = Encoding.UTF8.GetBytes("Hello world");
                this.initialMessage.WriteBytes(messageBytes);
            }

            public override void ChannelActive(IChannelHandlerContext context) => context.WriteAndFlushAsync(this.initialMessage).ConfigureAwait(false);

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var packet = message as DatagramPacket;
                UnityEngine.Debug.LogFormat("Received from server: {0}", packet.Content.ToString(Encoding.UTF8));
                packet.Release();
            }

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                UnityEngine.Debug.LogFormat("Exception: {0}", exception);
                context.CloseAsync();
            }
        }
    }
}