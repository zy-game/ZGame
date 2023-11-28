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
    public class TcpChannel : IChannel
    {
        public string address { get; set; }
        public bool connected => _socket == null ? false : _socket.Connected;

        private Socket _socket;
        private bool isSendWork = false;
        private byte[] _recBytes = new byte[1024 * 1024];

        public async UniTask Connect(string address)
        {
            UriBuilder builder = new UriBuilder(address);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Debug.Log($"Connected address:{builder.Host} port:{builder.Port}");
            await _socket.ConnectAsync(new IPEndPoint(IPAddress.Parse(builder.Host), builder.Port));
            if (connected is false)
            {
                Dispose();
                Debug.LogError("链接失败");
                return;
            }

            Task.Factory.StartNew(OnStartReceiver);
        }

        public UniTask Close()
        {
            if (connected)
            {
                _socket.Disconnect(false);
            }

            return UniTask.CompletedTask;
        }

        public async void WriteAndFlush(byte[] bytes)
        {
            int result = await _socket.SendAsync(new Memory<byte>(bytes), SocketFlags.None);
            if (result < bytes.Length)
            {
                Debug.LogError("发送数据错误");
                return;
            }
        }

        void OnStartReceiver()
        {
            while (connected)
            {
                // recvToken = new CancellationTokenSource();
                int lenght = _socket.Receive(_recBytes, SocketFlags.None);
                if (lenght <= 0)
                {
                    Debug.LogError(string.Format("{0},{1},{2}", address, "发送了一个0字节的消息", lenght));
                    return;
                }

                byte[] bytes = new byte[lenght];
                Array.Copy(_recBytes, 0, bytes, 0, lenght);
                NetworkManager.instance.Receiver(this, bytes);
            }
        }


        public void Dispose()
        {
            address = String.Empty;
            _socket = null;
            isSendWork = false;
        }
    }
}