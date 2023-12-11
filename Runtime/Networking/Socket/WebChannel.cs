// using System;
// using Cysharp.Threading.Tasks;
// using UnityWebSocket;
//
// namespace ZGame.Networking
// {
//     class WebChannel : IChannel
//     {
//         public string address { get; }
//         public bool connected { get; set; }
//         private WebSocket _webSocket;
//         private UniTaskCompletionSource _taskCompletionSource;
//
//         public UniTask Connect(string address)
//         {
//             _taskCompletionSource = new UniTaskCompletionSource();
//             _webSocket = new WebSocket(address);
//             _webSocket.OnClose += OnHandleCloseEvent;
//             _webSocket.OnError += OnHandleErrorEvent;
//             _webSocket.OnMessage += OnHandleMessageEvent;
//             _webSocket.OnOpen += OnHandleOpenEvent;
//             _webSocket.ConnectAsync();
//             return _taskCompletionSource.Task;
//         }
//
//         private void OnHandleOpenEvent(object s, OpenEventArgs e)
//         {
//             connected = true;
//             _taskCompletionSource.TrySetResult();
//         }
//
//         private void OnHandleMessageEvent(object s, MessageEventArgs e)
//         {
//             NetworkManager.instance.Receiver(this, e.RawData);
//         }
//
//         private void OnHandleErrorEvent(object s, ErrorEventArgs e)
//         {
//             connected = false;
//             _webSocket.CloseAsync();
//         }
//
//         private void OnHandleCloseEvent(object s, CloseEventArgs e)
//         {
//             connected = false;
//             _taskCompletionSource?.TrySetResult();
//         }
//
//         public UniTask Close()
//         {
//             _taskCompletionSource = new UniTaskCompletionSource();
//             _webSocket.CloseAsync();
//             return _taskCompletionSource.Task;
//         }
//
//         public void WriteAndFlush(byte[] bytes)
//         {
//             if (connected)
//             {
//                 _webSocket.SendAsync(bytes);
//             }
//         }
//
//         public void Dispose()
//         {
//             _webSocket = null;
//             connected = false;
//         }
//     }
// }