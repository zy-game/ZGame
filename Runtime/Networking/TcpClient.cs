using System;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Handlers.Tls;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using MemoryPack;

namespace ZGame.Networking
{
    public class TcpClient : INetClient
    {
        private bool isSsl = false;
        private IChannel channel;
        private MultithreadEventLoopGroup group;
        public int id => channel.Id.GetHashCode();
        public string address { get; private set; }
        public bool Connected { get; private set; }

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
            group = new MultithreadEventLoopGroup();
            try
            {
                var bootstrap = new Bootstrap();
                bootstrap.Group(group).Channel<TcpSocketChannel>().Option(ChannelOption.TcpNodelay, true).Handler(new ActionChannelInitializer<ISocketChannel>(c =>
                {
                    IChannelPipeline pipeline = c.Pipeline;
                    pipeline.AddLast(new LoggingHandler("CLIENT-CONN"));
                    pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                    pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                    pipeline.AddLast("echo", new TcpClientHandler());
                }));

                channel = await bootstrap.ConnectAsync(host, port);
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

        class TcpClientHandler : ChannelHandlerAdapter
        {
            readonly IByteBuffer initialMessage;

            public TcpClientHandler()
            {
                this.initialMessage = Unpooled.Buffer(256);
                byte[] messageBytes = Encoding.UTF8.GetBytes("Hello world");
                this.initialMessage.WriteBytes(messageBytes);
            }

            public override void ChannelActive(IChannelHandlerContext context) => context.WriteAndFlushAsync(this.initialMessage);

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var byteBuffer = message as IByteBuffer;
                GameFrameworkEntry.Logger.LogFormat("Received from server: {0}", byteBuffer.ToString(Encoding.UTF8));
                byteBuffer.Release();
            }

            public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                GameFrameworkEntry.Logger.LogFormat("Exception: {0}", exception);
                context.CloseAsync();
            }
        }
    }
}