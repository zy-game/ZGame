using System.Threading.Tasks;
using WebSocketSharp;
using System.Collections.Generic;
using System;
using System.Collections;
using System.IO;
using Cysharp.Threading.Tasks;
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

        public UniTask<IChannel> Close()
        {
            _websocket.Close();
            return UniTask.FromResult<IChannel>(this);
        }

        public UniTask<IChannel> Connect(string address, int id = 0)
        {
            this.address = address;
            _websocket = new WebSocketSharp.WebSocket(address);
            _websocket.OnOpen += (object sender, EventArgs e) => { connected = true; };
            _websocket.OnError += (object sender, ErrorEventArgs e) =>
            {
                connected = false;
                NetworkManager.instance.Close(address);
            };
            _websocket.OnMessage += (object sender, MessageEventArgs e) => { IMessageRecvierHandle.PublishMessage(this, e.RawData); };
            _websocket.OnClose += (s, e) => { connected = false; };
            _websocket.Connect();
            return UniTask.FromResult<IChannel>(this);
        }

        public UniTask WriteAndFlush(byte[] bytes)
        {
            _websocket.Send(bytes);
            return UniTask.CompletedTask;
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