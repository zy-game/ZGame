using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace ZEngine.Network
{
    public class TCPSocket : IChannel
    {
        public string address { get; set; }
        public bool connected => socket == null ? false : socket.Connected;
        private Socket socket;
        private SocketAsyncEventArgs recv;
        private SocketAsyncEventArgs send;
        private bool isSendWork = false;
        private Queue<byte[]> waiting = new Queue<byte[]>();

        private CancellationTokenSource recvToken;
        private CancellationTokenSource sendToken;


        public async IScheduleHandle<IChannel> Connect(string address, int id = 0)
        {
            this.address = address;
        }

        private async void DOConnected()
        {
            UriBuilder builder = new UriBuilder(address);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            await SocketTaskExtensions.ConnectAsync(socket, new IPEndPoint(IPAddress.Parse(builder.Host), builder.Port));
            if (connected is false)
            {
                Dispose();
                return;
            }

            Task.Factory.StartNew(OnStartRecvie);
        }

        async void OnStartRecvie()
        {
            Memory<byte> recvBuf = new Memory<byte>(new byte[1024 * 1028]);
            while (connected)
            {
                recvToken = new CancellationTokenSource();
                int lenght = await socket.ReceiveAsync(recvBuf, SocketFlags.None, recvToken.Token);
                if (lenght <= 0)
                {
                    Engine.Console.Error("链接断开");
                    Close();
                    return;
                }

                Memory<byte> memory = recvBuf.Slice(0, lenght);
                NetworkManager.instance.Dispacher(this, memory.ToArray());
            }
        }


        public IScheduleHandle<int> WriteAndFlush(byte[] bytes)
        {
            IScheduleHandleToken<int> scheduleHandleToken = IScheduleHandleToken.Create<int>();
            IScheduleHandle<int> scheduleHandle = IScheduleHandle.Schedule<int>(scheduleHandleToken);
            DOWriteAndFlush(scheduleHandleToken, bytes);
            return scheduleHandle;
        }

        private async void DOWriteAndFlush(IScheduleHandleToken<int> scheduleHandleToken, byte[] bytes)
        {
            sendToken = new CancellationTokenSource();
            int result = await socket.SendAsync(new Memory<byte>(bytes), SocketFlags.None, sendToken.Token);
            if (result < bytes.Length)
            {
                Engine.Console.Error("发送数据错误");
                scheduleHandleToken.Complate(0);
                return;
            }

            scheduleHandleToken.Complate(result);
            sendToken = null;
        }


        public IScheduleHandle<IChannel> Close()
        {
            recvToken?.Cancel();
            sendToken?.Cancel();
            socket.Disconnect(false);
            socket = null;
            Dispose();
            return IScheduleHandle.Complate<IChannel>(this);
        }

        public void Dispose()
        {
            address = String.Empty;
            socket = null;
            recv = null;
            send = null;
            sendToken = null;
            recvToken = null;
            waiting.Clear();
            isSendWork = false;
        }
    }
}