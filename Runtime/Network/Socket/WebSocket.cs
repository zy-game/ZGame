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

        public void Close()
        {
            _websocket.Close();
        }

        public void Connect(string address, int id = 0)
        {
            this.address = address;
            _websocket = new WebSocketSharp.WebSocket(address);
            _websocket.OnOpen += (object sender, EventArgs e) => { connected = true; };
            _websocket.OnError += (object sender, ErrorEventArgs e) =>
            {
                connected = false;
                NetworkManager.instance.Close(address);
            };
            _websocket.OnMessage += (object sender, MessageEventArgs e) => { RPCHandle.instance.Dispacher(this, e.RawData); };
            _websocket.OnClose += (s, e) => { connected = false; };
            _websocket.Connect();
        }

        public void WriteAndFlush(byte[] bytes)
        {
            _websocket.Send(bytes);
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