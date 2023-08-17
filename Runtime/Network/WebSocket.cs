using System.Threading.Tasks;
using WebSocketSharp;
using System.Collections.Generic;
using System;
using ZEngine;

namespace ZEngine.Network
{
    public sealed class WebSocket : IChannel
    {
        private WebSocketSharp.WebSocket _websocket;
        public string address { get; private set; }
        public bool connected => _websocket == null ? false : _websocket.IsConnected;

        public void Close()
        {
        }

        public void Connect(string address)
        {
            this.address = address;
            _websocket = new WebSocketSharp.WebSocket(address);
            _websocket.OnOpen += OnConnectCompletion;
            _websocket.OnError += OnWebSocketCrashErrord;
            _websocket.OnMessage += OnWebSocketRecvied;
            _websocket.OnClose += OnWebSocketClosed;
            _websocket.Connect();
        }

        public void WriteAndFlush(byte[] bytes)
        {
            throw new NotImplementedException();
        }

        private void OnWebSocketClosed(object sender, CloseEventArgs e)
        {
        }

        private void OnWebSocketRecvied(object sender, MessageEventArgs e)
        {
        }

        private void OnWebSocketCrashErrord(object sender, ErrorEventArgs e)
        {
        }

        private void OnConnectCompletion(object sender, EventArgs e)
        {
        }

        public void Release()
        {
        }
    }
}