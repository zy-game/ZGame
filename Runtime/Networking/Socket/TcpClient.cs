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

namespace ZGame.Networking
{
    public class TcpClient : INetClient
    {
        private IChannel channel;
        private MultithreadEventLoopGroup group;
        public int cid { get; private set; }
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

        public async UniTask<Status> ConnectAsync(int cid, string host, ushort port, IDispatcher dispatcher)
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
                    pipeline.AddLast("echo", new TcpClientHandler(this, dispatcher));
                }));

                channel = await bootstrap.ConnectAsync(host, port);
                this.isConnected = true;
                this.cid = cid;
                return Status.Success;
            }
            catch (Exception e)
            {
                CoreAPI.Logger.LogError(e);
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

        class TcpClientHandler : ChannelHandlerAdapter
        {
            private INetClient _client;
            private IDispatcher _dispatcher;

            public TcpClientHandler(INetClient client, IDispatcher dispatcher)
            {
                this._client = client;
                this._dispatcher = dispatcher;
            }

            public override void ChannelActive(IChannelHandlerContext context)
            {
                this._dispatcher?.Active(_client);
            }

            public override void ChannelInactive(IChannelHandlerContext context)
            {
                this._dispatcher?.Inactive(_client);
            }

            public override void ChannelRead(IChannelHandlerContext context, object message)
            {
                var byteBuffer = message as IByteBuffer;
                Packet packet = Packet.Deserialized(byteBuffer.Array);
                CoreAPI.Logger.LogFormat("Received from server: {0}", byteBuffer.ToString(Encoding.UTF8));
                this._dispatcher?.Dispatch(_client, packet);
                byteBuffer.Release();
            }

            public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

            public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
            {
                // handler?.Exception(_client, exception);
                CoreAPI.Logger.LogFormat("Exception: {0}", exception);
                context.CloseAsync();
            }
        }
    }
}