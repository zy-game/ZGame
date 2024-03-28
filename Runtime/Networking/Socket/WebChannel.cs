using System;
using Cysharp.Threading.Tasks;
using UnityWebSocket;

namespace ZGame.Networking
{
    class WebChannel<T> : IChannel where T : ISerialize, new()
    {
        public string address { get; }
        public bool connected { get; set; }
        public ISerialize serialize { get; private set; }
        private WebSocket _webSocket;
        private UniTaskCompletionSource _taskCompletionSource;
        private Action<IMessage> _recveCallback;

        public UniTask Connect(string address)
        {
            _taskCompletionSource = new UniTaskCompletionSource();
            serialize = GameFrameworkFactory.Spawner<T>();
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

            if (_recveCallback is not null)
            {
                _recveCallback(message);
                return;
            }

            GameFrameworkEntry.Notify.Notify(message);
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
            UniTaskCompletionSource<T1> taskCompletionSource2 = new UniTaskCompletionSource<T1>();
            _recveCallback = new Action<IMessage>(args => { taskCompletionSource2.TrySetResult((T1)args); });
            _webSocket.SendAsync(serialize.Serialize(message));
            return taskCompletionSource2.Task;
        }

        public void Dispose()
        {
            _webSocket = null;
            connected = false;
        }
    }
}