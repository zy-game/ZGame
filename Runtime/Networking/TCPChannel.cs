using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Networking
{
    public class TCPChannel : IChannel
    {
        public string guid { get; } = ID.New();
        public string address { get; set; }
        public bool connected => socket == null ? false : socket.Connected;

        private Socket socket;
        private bool isSendWork = false;
        private byte[] recBytes = new byte[1024 * 1024];
        private DefaultNetworkSystem handle;

        public TCPChannel(DefaultNetworkSystem handle)
        {
            this.handle = handle;
        }

        public async void Connect(string address)
        {
            UriBuilder builder = new UriBuilder(address);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Debug.Log($"Connected address:{builder.Host} port:{builder.Port}");
            await socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(builder.Host), builder.Port));
            if (connected is false)
            {
                Dispose();
                Debug.LogError("链接失败");
                return;
            }

            Task.Factory.StartNew(OnStartRecvie);
            handle.Active(this);
        }

        public void Close()
        {
            if (connected)
            {
                socket.Disconnect(false);
            }

        }

        public async void WriteAndFlush(byte[] bytes)
        {
            int result = await socket.SendAsync(new Memory<byte>(bytes), SocketFlags.None);
            if (result < bytes.Length)
            {
                Debug.LogError("发送数据错误");
                return;
            }

        }

        void OnStartRecvie()
        {
            while (connected)
            {
                // recvToken = new CancellationTokenSource();
                int lenght = socket.Receive(recBytes, SocketFlags.None);
                if (lenght <= 0)
                {
                    Debug.LogError(string.Format("{0},{1},{2}", address, "发送了一个0字节的消息", lenght));
                    return;
                }

                byte[] bytes = new byte[lenght];
                Array.Copy(recBytes, 0, bytes, 0, lenght);
                handle.Recvie(this, bytes);
            }
        }


        public void Dispose()
        {
            address = String.Empty;
            socket = null;
            isSendWork = false;
            handle.Dispose();
        }
    }
}