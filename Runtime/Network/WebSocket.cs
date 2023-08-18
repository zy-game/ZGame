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
        public bool connected { get; set; }

        public void Close()
        {
            _websocket.Close();
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
            _websocket.Send(bytes);
        }

        private void OnWebSocketClosed(object sender, CloseEventArgs e)
        {
            connected = false;
        }

        private void OnWebSocketRecvied(object sender, MessageEventArgs e)
        {
            NetworkManager.instance.DispatchMessage(this, e.RawData);
        }

        private void OnWebSocketCrashErrord(object sender, ErrorEventArgs e)
        {
            connected = false;
            NetworkManager.instance.Close(address);
        }

        private void OnConnectCompletion(object sender, EventArgs e)
        {
            connected = true;
        }

        public void Release()
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