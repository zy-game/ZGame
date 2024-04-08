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
        public bool isConnected { get; private set; }

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
                    pipeline.AddLast("echo", new TcpClientHandler(handler, this));
                }));

                channel = await bootstrap.ConnectAsync(host, port);
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

        class TcpClientHandler : ChannelHandlerAdapter
        {
            private IMessageHandler handler;
            private INetClient _client;

            public TcpClientHandler(IMessageHandler handler, INetClient client)
            {
                this.handler = handler;
                this._client = client;
            }

            public override void ChannelActive(IChannelHandlerContext context)
            {
                handler?.Active(_client);
            }

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                handler?.Inactive(_client);
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var byteBuffer = message as IByteBuffer;
                handler?.Receive(_client, byteBuffer);
                GameFrameworkEntry.Logger.LogFormat("Received from server: {0}", byteBuffer.ToString(Encoding.UTF8));
                byteBuffer.Release();
            }

            public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                handler?.Exception(_client, exception);
                GameFrameworkEntry.Logger.LogFormat("Exception: {0}", exception);
                context.CloseAsync();
            }
        }
    }
}