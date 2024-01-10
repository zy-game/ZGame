using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using Inworld;
using Inworld.Grpc;
using Newtonsoft.Json;
using UnityEngine;
using ZGame;


namespace ZGame.IM
{
    public interface IMHandler : IDisposable
    {
        string id { get; }

        void OnRecvieChatHandle(IMChatItem chatItem);
    }
}