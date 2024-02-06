using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ZGame.Networking
{
    public class TcpChannel<T> : IChannel where T : ISerialize
    {
        private Socket _socket;
        private bool isSendWork = false;
        private byte[] _recBytes = new byte[1024 * 1024];
        public string address { get; set; }
        public bool connected => _socket == null ? false : _socket.Connected;
        public ISerialize serialize { get; set; }

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

            serialize = Activator.CreateInstance<T>();
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

        public async void WriteAndFlush(IMessage message)
        {
            byte[] bytes = serialize.Serialize(message);
            int result = await _socket.SendAsync(new Memory<byte>(bytes), SocketFlags.None);
            if (result < bytes.Length)
            {
                Debug.LogError("发送数据错误");
                return;
            }
        }

        public async UniTask<T> WriteAndFlushAsync<T>(IMessage message) where T : IMessage
        {
            return default;
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
                IMessage message = serialize.Deserialize(bytes, out uint opcode);
                if (message is null)
                {
                    Debug.LogError("消息解析失败");
                    return;
                }

                CommandManager.OnExecuteCommand(opcode.ToString(), message);
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