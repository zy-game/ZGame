using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

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
        private byte[] recBytes = new byte[1024 * 1024];

        public async UniTask<IChannel> Connect(string address, int id = 0)
        {
            UriBuilder builder = new UriBuilder(address);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            ZGame.Console.Log("Connected", builder.Host, builder.Port);
            await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(builder.Host), builder.Port));
            if (connected is false)
            {
                Dispose();
                ZGame.Console.Error("链接失败");
                return this;
            }

            Task.Factory.StartNew(OnStartRecvie);
            return this;
        }


        void OnStartRecvie()
        {
            while (connected)
            {
                // recvToken = new CancellationTokenSource();
                int lenght = socket.Receive(recBytes, SocketFlags.None);
                if (lenght <= 0)
                {
                    ZGame.Console.Error(address, "发送了一个0字节的消息", lenght);
                    return;
                }

                byte[] bytes = new byte[lenght];
                Array.Copy(recBytes, 0, bytes, 0, lenght);
                NetworkManager.instance.Dispacher(this, bytes);
            }
        }


        public async UniTask WriteAndFlush(byte[] bytes)
        {
            int result = await socket.SendAsync(new Memory<byte>(bytes), SocketFlags.None);
            if (result < bytes.Length)
            {
                ZGame.Console.Error("发送数据错误");
                return;
            }
        }

        public UniTask<IChannel> Close()
        {
            socket.Disconnect(false);
            socket = null;
            Dispose();
            return UniTask.FromResult<IChannel>(this);
        }

        public void Dispose()
        {
            address = String.Empty;
            socket = null;
            recv = null;
            send = null;
            isSendWork = false;
        }
    }
}