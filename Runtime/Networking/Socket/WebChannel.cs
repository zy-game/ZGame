using System;
using Cysharp.Threading.Tasks;
using UnityWebSocket;

namespace ZGame.Networking
{
    class WebChannel<T> : IChannel where T : ISerialize
    {
        public string address { get; }
        public bool connected { get; set; }
        public ISerialize serialize { get; }
        private WebSocket _webSocket;
        private UniTaskCompletionSource _taskCompletionSource;

        public UniTask Connect(string address)
        {
            _taskCompletionSource = new UniTaskCompletionSource();
            _webSocket = new WebSocket(address);
            _webSocket.OnClose += OnHandleCloseEvent;
            _webSocket.OnError += OnHandleErrorEvent;
            _webSocket.OnMessage += OnHandleMessageEvent;
            _webSocket.OnOpen += OnHandleOpenEvent;
            _webSocket.ConnectAsync();
            return _taskCompletionSource.Task;
        }

        private void OnHandleOpenEvent(object s, OpenEventArgs e)
        {
            connected = true;
            _taskCompletionSource.TrySetResult();
            _taskCompletionSource = null;
        }

        private void OnHandleMessageEvent(object s, MessageEventArgs e)
        {
            IMessage message = serialize.Deserialize(e.RawData, out uint opcode);
            if (message is null)
            {
                return;
            }

            //ModuleManager.OnAction(opcode.ToString(), message);
        }

        private void OnHandleErrorEvent(object s, ErrorEventArgs e)
        {
            connected = false;
            _webSocket.CloseAsync();
        }

        private void OnHandleCloseEvent(object s, CloseEventArgs e)
        {
            connected = false;
            _taskCompletionSource?.TrySetResult();
            _taskCompletionSource = null;
        }

        public UniTask Close()
        {
            _taskCompletionSource = new UniTaskCompletionSource();
            _webSocket.CloseAsync();
            return _taskCompletionSource.Task;
        }

        public void WriteAndFlush(IMessage message)
        {
            if (connected)
            {
                _webSocket.SendAsync(serialize.Serialize(message));
            }
        }

        public UniTask<T1> WriteAndFlushAsync<T1>(IMessage message) where T1 : IMessage
        {
            return default;
        }

        public void Dispose()
        {
            _webSocket = null;
            connected = false;
        }
    }
}