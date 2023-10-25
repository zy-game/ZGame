using System;
using UnityWebSocket;

namespace ZGame.Networking
{
    class WebChannel : IChannel
    {
        public string guid { get; } = ID.New();
        public string address { get; }
        public bool connected { get; set; }
        private WebSocket webSocket;
        private DefaultNetworkSystem handle;

        public WebChannel(DefaultNetworkSystem handle)
        {
            this.handle = handle;
        }

        public void Connect(string address)
        {
            webSocket = new WebSocket(address);
            webSocket.OnClose += OnHandleCloseEvent;
            webSocket.OnError += OnHandleErrorEvent;
            webSocket.OnMessage += OnHandleMessageEvent;
            webSocket.OnOpen += OnHandleOpenEvent;
            webSocket.ConnectAsync();
        }

        private void OnHandleOpenEvent(object s, OpenEventArgs e)
        {
            connected = true;
            handle.Active(this);
        }

        private void OnHandleMessageEvent(object s, MessageEventArgs e)
        {
            handle.Recvie(this, e.RawData);
        }

        private void OnHandleErrorEvent(object s, ErrorEventArgs e)
        {
            connected = false;
            webSocket.CloseAsync();
        }

        private void OnHandleCloseEvent(object s, CloseEventArgs e)
        {
            connected = false;
        }

        public void Close()
        {
            webSocket.CloseAsync();
        }

        public void WriteAndFlush(byte[] bytes)
        {
            if (connected is false)
            {
                return;
            }

            webSocket.SendAsync(bytes);
        }

        public void Dispose()
        {
            webSocket = null;
            connected = false;
            handle.Dispose();
        }
    }
}