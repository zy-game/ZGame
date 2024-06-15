// using System;
// using System.Net.Security;
// using System.Security.Cryptography.X509Certificates;
// using System.Text;
// using System.Threading.Tasks;
// using Cysharp.Threading.Tasks;
// using DotNetty.Buffers;
// using DotNetty.Codecs;
// using DotNetty.Handlers.Logging;
// using DotNetty.Handlers.Tls;
// using DotNetty.Transport.Bootstrapping;
// using DotNetty.Transport.Channels;
// using DotNetty.Transport.Channels.Sockets;
//
// namespace ZGame.Networking
// {
//     public class TcpClient : ISocketClient
//     {
//         private IChannel channel;
//         private MultithreadEventLoopGroup group;
//         private IMessageRecvieHandle _recvieHandle;
//         public uint cid { get; private set; }
//         public string address { get; private set; }
//         public bool isConnected { get; private set; }
//
//         public async UniTask WriteAsync(byte[] message)
//         {
//             if (isConnected is false)
//             {
//                 return;
//             }
//
//             await channel.WriteAndFlushAsync(Unpooled.WrappedBuffer(message));
//         }
//
//         public void Recvie(MSG msg)
//         {
//             this._recvieHandle.RecvieHandler(this, msg);
//         }
//
//         public async UniTask<Status> ConnectAsync(string host, ushort port, IMessageRecvieHandle recvieHandle)
//         {
//             _recvieHandle = recvieHandle;
//             group = new MultithreadEventLoopGroup();
//             try
//             {
//                 var bootstrap = new Bootstrap();
//                 bootstrap.Group(group).Channel<TcpSocketChannel>().Option(ChannelOption.TcpNodelay, true).Handler(new ActionChannelInitializer<ISocketChannel>(c =>
//                 {
//                     IChannelPipeline pipeline = c.Pipeline;
//                     pipeline.AddLast(new LoggingHandler("CLIENT-CONN"));
//                     pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
//                     pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
//                     pipeline.AddLast("echo", new TcpClientHandler(this));
//                 }));
//
//                 channel = await bootstrap.ConnectAsync(host, port);
//                 this.isConnected = true;
//                 this.cid = Crc32.GetCRC32Str(Guid.NewGuid().ToString());
//                 this._recvieHandle.Active(this);
//                 return Status.Success;
//             }
//             catch (Exception e)
//             {
//                 AppCore.Logger.LogError(e);
//                 RefPooled.Free(this);
//                 return Status.Fail;
//             }
//         }
//
//         public async void Release()
//         {
//             if (this.channel != null)
//                 await channel.DisconnectAsync();
//             if (group != null)
//                 await group.ShutdownGracefullyAsync(TimeSpan.FromMilliseconds(100), TimeSpan.FromSeconds(1));
//             this.isConnected = false;
//             this._recvieHandle.Inactive(this);
//         }
//
//         class TcpClientHandler : ChannelHandlerAdapter
//         {
//             private ISocketClient _client;
//
//             public TcpClientHandler(ISocketClient client)
//             {
//                 this._client = client;
//             }
//
//             public override void ChannelRead(IChannelHandlerContext context, object message)
//             {
//                 var byteBuffer = message as IByteBuffer;
//                 _client.Recvie(MSG.Depack(byteBuffer.Array));
//                 AppCore.Logger.LogFormat("Received from server: {0}", byteBuffer.ToString(Encoding.UTF8));
//                 byteBuffer.Release();
//             }
//
//             public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();
//
//             public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
//             {
//                 AppCore.Logger.LogFormat("Exception: {0}", exception);
//                 context.CloseAsync();
//             }
//         }
//     }
// }