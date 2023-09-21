using System.Threading.Tasks;
using WebSocketSharp;
using System.Collections.Generic;
using System;
using System.Collections;
using System.IO;
using ProtoBuf;
using ZEngine;
using ErrorEventArgs = WebSocketSharp.ErrorEventArgs;

namespace ZEngine.Network
{
    public sealed class WebSocketChannel : IChannel
    {
        private WebSocketSharp.WebSocket _websocket;
        public string address { get; private set; }
        public bool connected { get; set; }

        class WebSocketCloseExecuteHandle : AbstractExecuteHandle, IChannelClosedExecuteHandle
        {
            public IChannel channel { get; }

            public WebSocketChannel socketChannel;

            protected override IEnumerator ExecuteCoroutine()
            {
                yield break;
            }

            public static WebSocketCloseExecuteHandle Create(WebSocketChannel channel)
            {
                WebSocketCloseExecuteHandle webSocketCloseExecuteHandle = Activator.CreateInstance<WebSocketCloseExecuteHandle>();
                webSocketCloseExecuteHandle.socketChannel = channel;
                webSocketCloseExecuteHandle.Execute();
                return webSocketCloseExecuteHandle;
            }
        }

        class WebSocketConnectExecuteHandle : AbstractExecuteHandle, IChannelConnectExecuteHandle
        {
            public int id { get; set; }
            public string address { get; set; }

            public IChannel channel
            {
                get => socketChannel;
            }

            public WebSocketChannel socketChannel;

            public static WebSocketConnectExecuteHandle Create(WebSocketChannel channel, string address, int id)
            {
                WebSocketConnectExecuteHandle webSocketConnectExecuteHandle = Activator.CreateInstance<WebSocketConnectExecuteHandle>();
                webSocketConnectExecuteHandle.address = address;
                webSocketConnectExecuteHandle.socketChannel = channel;
                webSocketConnectExecuteHandle.id = id;
                webSocketConnectExecuteHandle.Execute();
                return webSocketConnectExecuteHandle;
            }

            protected override IEnumerator ExecuteCoroutine()
            {
                this.address = address;
                socketChannel._websocket = new WebSocketSharp.WebSocket(address);
                socketChannel._websocket.OnOpen += (object sender, EventArgs e) => { socketChannel.connected = true; };
                socketChannel._websocket.OnError += (object sender, ErrorEventArgs e) =>
                {
                    socketChannel.connected = false;
                    NetworkManager.instance.Close(address);
                };
                socketChannel._websocket.OnMessage += (object sender, MessageEventArgs e) => { MessageDispatcher.instance.Enqueue(socketChannel, e.RawData); };

                socketChannel._websocket.OnClose += (s, e) => { socketChannel.connected = false; };
                socketChannel._websocket.Connect();
                yield return WaitFor.Create(() => socketChannel.connected == true);
            }
        }

        class WebSocketWriteExecuteHandle : AbstractExecuteHandle, IChannelWriteExecuteHandle
        {
            public IChannel channel
            {
                get => socketChannel;
            }

            public byte[] bytes { get; set; }
            private WebSocketChannel socketChannel;

            protected override IEnumerator ExecuteCoroutine()
            {
                socketChannel._websocket.Send(bytes);
                yield break;
            }

            public static WebSocketWriteExecuteHandle Create(WebSocketChannel channel, byte[] bytes)
            {
                WebSocketWriteExecuteHandle webSocketWriteExecuteHandle = Activator.CreateInstance<WebSocketWriteExecuteHandle>();
                webSocketWriteExecuteHandle.bytes = bytes;
                webSocketWriteExecuteHandle.socketChannel = channel;
                webSocketWriteExecuteHandle.Execute();
                return webSocketWriteExecuteHandle;
            }
        }

        public IChannelClosedExecuteHandle Close()
        {
            return WebSocketCloseExecuteHandle.Create(this);
        }

        public IChannelConnectExecuteHandle Connect(string address, int id = 0)
        {
            return WebSocketConnectExecuteHandle.Create(this, address, id);
        }

        public IChannelWriteExecuteHandle WriteAndFlush(byte[] bytes)
        {
            return WebSocketWriteExecuteHandle.Create(this, bytes);
        }

        public void Dispose()
        {
            if (connected)
            {
                Close();
            }

            _websocket = null;
            address = String.Empty;
        }
    }
}