using System;
using Cysharp.Threading.Tasks;
using UnityWebSocket;

namespace ZGame.Networking
{
    class WebChannel : IChannel
    {
        public string address { get; }
        public bool connected { get; set; }
        private WebSocket webSocket;
        private NetworkManager handle;
        private UniTaskCompletionSource taskCompletionSource;

        public WebChannel(NetworkManager handle)
        {
            this.handle = handle;
        }

        public UniTask Connect(string address)
        {
            taskCompletionSource = new UniTaskCompletionSource();
            webSocket = new WebSocket(address);
            webSocket.OnClose += OnHandleCloseEvent;
            webSocket.OnError += OnHandleErrorEvent;
            webSocket.OnMessage += OnHandleMessageEvent;
            webSocket.OnOpen += OnHandleOpenEvent;
            webSocket.ConnectAsync();
            return taskCompletionSource.Task;
        }

        private void OnHandleOpenEvent(object s, OpenEventArgs e)
        {
            connected = true;
            taskCompletionSource.TrySetResult();
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
            taskCompletionSource?.TrySetResult();
        }

        public UniTask Close()
        {
            taskCompletionSource = new UniTaskCompletionSource();
            webSocket.CloseAsync();
            return taskCompletionSource.Task;
        }

        public void WriteAndFlush(byte[] bytes)
        {
            if (connected)
            {
                webSocket.SendAsync(bytes);
            }
        }

        public void Dispose()
        {
            webSocket = null;
            connected = false;
            handle.Dispose();
        }
    }
}